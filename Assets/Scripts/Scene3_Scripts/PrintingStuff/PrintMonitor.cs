using System;
using System.Collections;
using System.IO;
using UnityEngine;

public enum PrintJobPauseReason
{
    None,
    PrinterOffline,
    OutOfPaper,
    NeedsUserIntervention,
    QueuePaused
}

public static class PrintMonitor
{
    //--------------------------------------------------
    // Wait until a document finishes printing, reporting
    // live progress and recoverable "stuck" states along
    // the way instead of just success/fail.
    //--------------------------------------------------
    public static IEnumerator MonitorPrintJob
    (
        Printer Printer,
        PrintDocument Document,
        Action<int, int> OnProgress,               // pagesPrinted, totalPages
        Action<PrintJobPauseReason, string> OnPaused,
        Action OnResumed,
        Action<bool, string> OnComplete,
        float StuckTimeoutSeconds = 300f
    )
    {
        string DocumentName =
            Path.GetFileName(Document.Print.LocalPDFPath);

        Debug.Log($"Monitoring print job: {DocumentName}");

        //--------------------------------------------------
        // Step 1: wait for the job to appear in the Windows
        // queue, then pin it by its unique JobId. Matching by
        // filename alone would be ambiguous if two jobs share
        // a name.
        //--------------------------------------------------

        float FindTimeout = 10f;
        float FindPollInterval = 0.1f;
        float FindTimer = 0f;

        Win32Structures.JOB_INFO_1? FoundJob = null;

        while (FoundJob == null)
        {
            FoundJob = Win32PrintSpooler.FindJob(Printer.Name, DocumentName);

            if (FoundJob == null)
            {
                FindTimer += FindPollInterval;

                if (FindTimer >= FindTimeout)
                {
                    // We never spotted it in the spooler queue. Since
                    // Sumatra already reported a clean handoff to get us
                    // here, the far more likely explanation is that a
                    // small/fast job was spooled and cleared between our
                    // polls (some drivers barely keep jobs enumerable),
                    // not that printing actually failed - a genuinely
                    // offline/broken printer normally leaves the job
                    // sitting in the queue with an error/offline flag
                    // instead of making it disappear instantly.
                    Debug.LogWarning
                    (
                        $"Never saw '{DocumentName}' in the spooler queue " +
                        "after submission - assuming it printed and cleared " +
                        "too fast to catch, since Sumatra reported success."
                    );

                    OnComplete(true, "Assumed success (job cleared the queue before it could be tracked).");
                    yield break;
                }

                yield return new WaitForSeconds(FindPollInterval);
            }
        }

        int JobId = FoundJob.Value.JobId;

        Debug.Log($"Job entered queue with JobId {JobId}.");

        //--------------------------------------------------
        // Step 2: poll that specific JobId until it's gone.
        //--------------------------------------------------

        PrintJobPauseReason CurrentPauseReason = PrintJobPauseReason.None;
        float StuckTimer = 0f;

        int LastKnownPagesPrinted = 0;
        int LastKnownTotalPages = 0;
        bool SawPrintedFlag = false;
        bool SawErrorFlag = false;
        string LastErrorMessage = "";

        while (true)
        {
            Win32Structures.JOB_INFO_1? Job =
                Win32PrintSpooler.GetJob(Printer.Name, JobId);

            if (Job == null)
            {
                // The spooler dropped the job. This normally means it
                // finished, but drivers vary in how cleanly they report
                // that, so fall back on whatever we last observed rather
                // than trusting a single status flag.
                bool LooksComplete =
                    SawPrintedFlag
                    || LastKnownTotalPages == 0
                    || LastKnownPagesPrinted >= LastKnownTotalPages;

                if (SawErrorFlag && !LooksComplete)
                {
                    OnComplete(false, LastErrorMessage);
                }
                else
                {
                    OnComplete(true, "Finished");
                }

                yield break;
            }

            LastKnownPagesPrinted = Job.Value.PagesPrinted;
            LastKnownTotalPages = Job.Value.TotalPages;

            OnProgress(LastKnownPagesPrinted, LastKnownTotalPages);

            int Status = Job.Value.Status;

            if ((Status & Win32Structures.JOB_STATUS_PRINTED) != 0)
            {
                SawPrintedFlag = true;
            }

            //----------------------------------------
            // Hard failure: the job was explicitly removed by
            // someone (cancelled from the printer / spooler UI)
            // before it finished. We only trust this if we never
            // saw a "printed" flag first.
            //----------------------------------------

            if
            (
                !SawPrintedFlag
                &&
                (
                    (Status & Win32Structures.JOB_STATUS_DELETING) != 0
                    || (Status & Win32Structures.JOB_STATUS_DELETED) != 0
                )
            )
            {
                OnComplete(false, "Job was cancelled before it finished printing.");
                yield break;
            }

            //----------------------------------------
            // Recoverable pause conditions: something needs the
            // user's attention (paper, offline, jam, etc). Don't
            // fail the job immediately - report it and keep
            // watching. Only fail if it's stuck too long.
            //----------------------------------------

            PrintJobPauseReason NewPauseReason = PrintJobPauseReason.None;
            string PauseMessage = null;

            if ((Status & Win32Structures.JOB_STATUS_OFFLINE) != 0)
            {
                NewPauseReason = PrintJobPauseReason.PrinterOffline;
                PauseMessage = "Printer is offline.";
            }
            else if ((Status & Win32Structures.JOB_STATUS_PAPEROUT) != 0)
            {
                NewPauseReason = PrintJobPauseReason.OutOfPaper;
                PauseMessage = "Printer is out of paper.";
            }
            else if ((Status & Win32Structures.JOB_STATUS_USER_INTERVENTION) != 0)
            {
                NewPauseReason = PrintJobPauseReason.NeedsUserIntervention;
                PauseMessage = "Printer needs attention (jam, cover open, etc).";
            }
            else if ((Status & Win32Structures.JOB_STATUS_PAUSED) != 0)
            {
                NewPauseReason = PrintJobPauseReason.QueuePaused;
                PauseMessage = "Print queue is paused.";
            }
            else if ((Status & Win32Structures.JOB_STATUS_ERROR) != 0)
            {
                NewPauseReason = PrintJobPauseReason.NeedsUserIntervention;
                PauseMessage = "Printer reported an error.";
            }

            if (NewPauseReason != PrintJobPauseReason.None)
            {
                SawErrorFlag = true;
                LastErrorMessage = PauseMessage;

                if (CurrentPauseReason == PrintJobPauseReason.None)
                {
                    // Just entered a stuck state.
                    CurrentPauseReason = NewPauseReason;
                    StuckTimer = 0f;
                    OnPaused(NewPauseReason, PauseMessage);
                }
                else
                {
                    StuckTimer += 0.5f;

                    if (StuckTimer >= StuckTimeoutSeconds)
                    {
                        OnComplete(false, $"Print job stuck for over {StuckTimeoutSeconds}s: {PauseMessage}");
                        yield break;
                    }
                }
            }
            else if (CurrentPauseReason != PrintJobPauseReason.None)
            {
                // We were stuck, and now we're not.
                CurrentPauseReason = PrintJobPauseReason.None;
                StuckTimer = 0f;
                OnResumed();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}