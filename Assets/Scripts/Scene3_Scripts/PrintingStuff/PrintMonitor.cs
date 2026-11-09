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
        Action<int, int> OnProgress,
        Action<PrintJobPauseReason, string> OnPaused,
        Action OnResumed,
        Action<bool, string> OnComplete,
        float StuckTimeoutSeconds = 15f,

        //Only used when the document was printed from a different PDF than Document.Print.LocalPDFPath
        //( e.g. the header merged into the document's PDF ). The spooler job is named after the file that was
        //actually printed, so that file's name is the one to search for.
        string PDFPathOverride = null
    )
    {
        //--------------------------------------------------
        // Completion guard.
        //
        // OnComplete can only be called ONCE.
        //--------------------------------------------------

        bool CompletionReported = false;

        void CompleteOnce
        (
            bool Success,
            string Message
        )
        {
            if (CompletionReported)
            {
                Debug.LogWarning
                (
                    $"[{Printer.Name}] PrintMonitor attempted " +
                    "to report completion more than once. Ignoring."
                );

                return;
            }

            CompletionReported = true;

            OnComplete
            (
                Success,
                Message
            );
        }


        //--------------------------------------------------
        // Document name.
        //--------------------------------------------------

        string DocumentName =
            Path.GetFileName
            (
                string.IsNullOrEmpty(PDFPathOverride)
                    ? Document.Print.LocalPDFPath
                    : PDFPathOverride
            );


        //Debug.Log
        //(
        //    $"[{Printer.Name}] Monitoring print job: {DocumentName}"
        //);


        //--------------------------------------------------
        // STEP 1:
        // Find the job in the Windows spooler.
        //--------------------------------------------------

        //--------------------------------------------------
        // This monitor is only started AFTER the print
        // process (SumatraPDF) has exited successfully, so
        // the job has already been handed to the spooler.
        // On a fast local printer with RAW jobs it can be
        // printed and removed before our first poll. So a
        // short grace period is enough; if the job is still
        // not visible, it already finished.
        //
        // Real elapsed time is used (not a loop counter),
        // because each FindJob call itself takes time.
        //--------------------------------------------------

        float FindTimeout = 3f;
        float FindPollInterval = 0.1f;
        float FindStartTime = Time.realtimeSinceStartup;

        Win32Structures.JOB_INFO_1? FoundJob = null;


        while (FoundJob == null)
        {
            FoundJob =
                Win32PrintSpooler.FindJob
                (
                    Printer.Name,
                    DocumentName
                );


            if (FoundJob == null)
            {
                if
                (
                    Time.realtimeSinceStartup - FindStartTime >=
                    FindTimeout
                )
                {
                    Debug.Log
                    (
                        $"[{Printer.Name}] Print job " +
                        $"'{DocumentName}' was not found in the " +
                        "Windows spooler after submission - it " +
                        "already finished. Treating as completed."
                    );


                    CompleteOnce
                    (
                        true,
                        "Finished"
                    );


                    yield break;
                }


                yield return
                    new WaitForSeconds
                    (
                        FindPollInterval
                    );
            }
        }


        //--------------------------------------------------
        // IMPORTANT:
        //
        // From this point onward, use JobId.
        // Do NOT search by filename again.
        //--------------------------------------------------

        int JobId =
            FoundJob.Value.JobId;


        Debug.Log
        (
            $"[{Printer.Name}] Job entered queue " +
            $"with JobId {JobId}."
        );


        //--------------------------------------------------
        // STEP 2:
        // Initial state.
        //--------------------------------------------------

        PrintJobPauseReason CurrentPauseReason =
            PrintJobPauseReason.None;


        int LastKnownPagesPrinted =
            FoundJob.Value.PagesPrinted;


        int LastKnownTotalPages =
            FoundJob.Value.TotalPages;


        int LastObservedPagesPrinted =
            LastKnownPagesPrinted;


        bool SawPrintedFlag = false;
        bool SawPrinterActivity = false;
        bool SawErrorFlag = false;


        string LastErrorMessage = "";


        //--------------------------------------------------
        // Actual 15-second safety timer.
        //
        // Reset when:
        //
        // 1. Pages actually increase.
        // 2. Printer reports active processing.
        //
        // Merely being online does NOT reset it.
        //--------------------------------------------------

        float StuckTimer = 0f;


        //--------------------------------------------------
        // Heartbeat.
        //--------------------------------------------------

        float HeartbeatTimer = 0f;


        const float PollInterval = 0.5f;
        const float HeartbeatInterval = 2f;


        //--------------------------------------------------
        // STEP 3:
        // Monitor.
        //--------------------------------------------------

        while (true)
        {
            //--------------------------------------------------
            // Get the specific spooler job.
            //--------------------------------------------------

            Win32Structures.JOB_INFO_1? Job =
                Win32PrintSpooler.GetJob
                (
                    Printer.Name,
                    JobId
                );


            //--------------------------------------------------
            // Get actual printer-level status.
            //--------------------------------------------------

            Win32PrintSpooler.PrinterStatusInfo PrinterStatus =
                Win32PrintSpooler.GetPrinterStatus
                (
                    Printer.Name
                );


            //--------------------------------------------------
            // JOB DISAPPEARED
            //--------------------------------------------------

            if (Job == null)
            {
                //--------------------------------------------------
                // Confirmed printed.
                //--------------------------------------------------

                if (SawPrintedFlag)
                {
                    Debug.Log
                    (
                        $"[{Printer.Name}] JobId {JobId} " +
                        "reported PRINTED."
                    );


                    CompleteOnce
                    (
                        true,
                        "Finished"
                    );


                    yield break;
                }


                //--------------------------------------------------
                // All pages printed.
                //--------------------------------------------------

                if
                (
                    LastKnownTotalPages > 0 &&
                    LastKnownPagesPrinted >=
                    LastKnownTotalPages
                )
                {
                    Debug.Log
                    (
                        $"[{Printer.Name}] JobId {JobId} " +
                        $"completed {LastKnownPagesPrinted}/" +
                        $"{LastKnownTotalPages} pages."
                    );


                    CompleteOnce
                    (
                        true,
                        "Finished"
                    );


                    yield break;
                }


                //--------------------------------------------------
                // Job disappeared WITHOUT any confirmed page/flag
                // evidence of completion.
                //
                // IMPORTANT:
                // For a lot of RAW-datatype jobs (which is what we
                // submit), Windows never populates TotalPages /
                // PagesPrinted at all, and the JOB_STATUS_PRINTED
                // flag can be set and cleared between two polls on
                // a fast local printer - we can easily never
                // observe it.
                //
                // So "disappeared with zero evidence either way" is
                // actually the NORMAL shape of a fast, successful
                // print - not a failure. We only treat disappearance
                // as a failure if we actually observed a real
                // problem (SawErrorFlag) while the job existed.
                //--------------------------------------------------

                if (SawErrorFlag)
                {
                    Debug.LogWarning
                    (
                        $"[{Printer.Name}] JobId {JobId} " +
                        "disappeared after an error/pause " +
                        "condition was observed."
                    );


                    CompleteOnce
                    (
                        false,
                        string.IsNullOrEmpty
                        (
                            LastErrorMessage
                        )
                            ?
                            "Print job disappeared before completion."
                            :
                            LastErrorMessage
                    );
                }
                else
                {
                    Debug.Log
                    (
                        $"[{Printer.Name}] JobId {JobId} " +
                        "disappeared from the spooler with no " +
                        "errors ever observed - treating as " +
                        "completed."
                    );


                    CompleteOnce
                    (
                        true,
                        "Finished"
                    );
                }


                yield break;
            }


            //--------------------------------------------------
            // Read job values.
            //--------------------------------------------------

            LastKnownPagesPrinted =
                Job.Value.PagesPrinted;


            LastKnownTotalPages =
                Job.Value.TotalPages;


            OnProgress
            (
                LastKnownPagesPrinted,
                LastKnownTotalPages
            );


            int JobStatus =
                Job.Value.Status;


            //--------------------------------------------------
            // PRINTED FLAG
            //--------------------------------------------------

            if
            (
                (JobStatus &
                Win32Structures.JOB_STATUS_PRINTED) != 0
            )
            {
                SawPrintedFlag = true;


                CompleteOnce
                (
                    true,
                    "Finished"
                );


                yield break;
            }


            //--------------------------------------------------
            // ACTUAL PAGE PROGRESS
            //--------------------------------------------------

            if
            (
                LastKnownPagesPrinted >
                LastObservedPagesPrinted
            )
            {
                LastObservedPagesPrinted =
                    LastKnownPagesPrinted;


                StuckTimer = 0f;


                Debug.Log
                (
                    $"[{Printer.Name}] JobId {JobId} " +
                    $"page progress: " +
                    $"{LastKnownPagesPrinted}/" +
                    $"{LastKnownTotalPages}"
                );
            }


            //--------------------------------------------------
            // PRINTER-LEVEL STATUS
            //--------------------------------------------------

            int PrinterStatusFlags =
                PrinterStatus.Success
                    ? PrinterStatus.Status
                    : 0;


            bool PrinterIsActive =
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_PRINTING) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_PROCESSING) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_IO_ACTIVE) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_BUSY) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_INITIALIZING) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_WARMING_UP) != 0
            );


            //--------------------------------------------------
            // Explicit printer failure.
            //--------------------------------------------------

            bool PrinterHasError =
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_ERROR) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_OFFLINE) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_PAPER_JAM) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_PAPER_OUT) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_PAPER_PROBLEM) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_DOOR_OPEN) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_USER_INTERVENTION) != 0
            )
            ||
            (
                (PrinterStatusFlags &
                 Win32Structures.PRINTER_STATUS_NO_TONER) != 0
            );


            //--------------------------------------------------
            // Active printer processing.
            //
            // This is real evidence that the printer is alive.
            // Reset the 15-second timer.
            //--------------------------------------------------

            if (PrinterIsActive)
            {
                if (!SawPrinterActivity)
                {
                    SawPrinterActivity = true;


                    Debug.Log
                    (
                        $"[{Printer.Name}] Printer reports active " +
                        $"processing. Status = " +
                        $"0x{PrinterStatusFlags:X}"
                    );
                }


                StuckTimer = 0f;
            }


            //--------------------------------------------------
            // Printer error.
            //--------------------------------------------------

            if (PrinterHasError)
            {
                SawErrorFlag = true;


                LastErrorMessage =
                    PrinterStatus.Message;


                if
                (
                    CurrentPauseReason ==
                    PrintJobPauseReason.None
                )
                {
                    CurrentPauseReason =
                        PrintJobPauseReason.NeedsUserIntervention;


                    OnPaused
                    (
                        CurrentPauseReason,
                        PrinterStatus.Message
                    );
                }
            }
            else
            {
                //--------------------------------------------------
                // Job-level errors.
                //--------------------------------------------------

                PrintJobPauseReason NewPauseReason =
                    PrintJobPauseReason.None;


                string PauseMessage = null;


                if
                (
                    (JobStatus &
                    Win32Structures.JOB_STATUS_OFFLINE) != 0
                )
                {
                    NewPauseReason =
                        PrintJobPauseReason.PrinterOffline;


                    PauseMessage =
                        "Printer is offline.";
                }
                else if
                (
                    (JobStatus &
                    Win32Structures.JOB_STATUS_PAPEROUT) != 0
                )
                {
                    NewPauseReason =
                        PrintJobPauseReason.OutOfPaper;


                    PauseMessage =
                        "Printer is out of paper.";
                }
                else if
                (
                    (JobStatus &
                    Win32Structures.JOB_STATUS_USER_INTERVENTION) != 0
                )
                {
                    NewPauseReason =
                        PrintJobPauseReason.NeedsUserIntervention;


                    PauseMessage =
                        "Printer needs attention.";
                }
                else if
                (
                    (JobStatus &
                    Win32Structures.JOB_STATUS_PAUSED) != 0
                )
                {
                    NewPauseReason =
                        PrintJobPauseReason.QueuePaused;


                    PauseMessage =
                        "Print queue is paused.";
                }
                else if
                (
                    (JobStatus &
                    Win32Structures.JOB_STATUS_ERROR) != 0
                )
                {
                    NewPauseReason =
                        PrintJobPauseReason.NeedsUserIntervention;


                    PauseMessage =
                        "Print job reported an error.";
                }


                //--------------------------------------------------
                // New pause/error state.
                //--------------------------------------------------

                if
                (
                    NewPauseReason !=
                    PrintJobPauseReason.None
                )
                {
                    SawErrorFlag = true;


                    LastErrorMessage =
                        PauseMessage;


                    if
                    (
                        CurrentPauseReason ==
                        PrintJobPauseReason.None
                    )
                    {
                        CurrentPauseReason =
                            NewPauseReason;


                        OnPaused
                        (
                            NewPauseReason,
                            PauseMessage
                        );
                    }
                }


                //--------------------------------------------------
                // Printer recovered.
                //--------------------------------------------------

                else if
                (
                    CurrentPauseReason !=
                    PrintJobPauseReason.None
                )
                {
                    CurrentPauseReason =
                        PrintJobPauseReason.None;


                    OnResumed();
                }
            }


            //--------------------------------------------------
            // Job explicitly deleted/cancelled.
            //--------------------------------------------------

            if
            (
                (JobStatus &
                Win32Structures.JOB_STATUS_DELETING) != 0
                ||
                (JobStatus &
                Win32Structures.JOB_STATUS_DELETED) != 0
            )
            {
                CompleteOnce
                (
                    false,
                    "Print job was cancelled before completion."
                );


                yield break;
            }


            //--------------------------------------------------
            // 15-SECOND SAFETY TIMER
            //
            // If there is no active physical printer
            // processing, the timer increases.
            //--------------------------------------------------

            if (!PrinterIsActive)
            {
                StuckTimer +=
                    PollInterval;
            }


            //--------------------------------------------------
            // HEARTBEAT
            //--------------------------------------------------

            HeartbeatTimer +=
                PollInterval;


            if
            (
                HeartbeatTimer >=
                HeartbeatInterval
            )
            {
                HeartbeatTimer = 0f;


                Debug.Log
                (
                    $"[{Printer.Name}] " +
                    $"JobId {JobId} | " +
                    $"PrinterStatus=0x{PrinterStatusFlags:X} | " +
                    $"PrinterMessage='{PrinterStatus.Message}' | " +
                    $"JobStatus=0x{JobStatus:X} | " +
                    $"Pages={LastKnownPagesPrinted}/" +
                    $"{LastKnownTotalPages} | " +
                    $"StuckTimer={StuckTimer:F1}/" +
                    $"{StuckTimeoutSeconds}s"
                );
            }


            //--------------------------------------------------
            // 15 SECOND TIMEOUT
            //--------------------------------------------------

            if
            (
                StuckTimer >=
                StuckTimeoutSeconds
            )
            {
                string FailureMessage;


                if
                (
                    SawErrorFlag &&
                    !string.IsNullOrEmpty
                    (
                        LastErrorMessage
                    )
                )
                {
                    FailureMessage =
                        $"Printer failed to make progress for " +
                        $"{StuckTimeoutSeconds}s. " +
                        $"{LastErrorMessage}";
                }
                else
                {
                    FailureMessage =
                        $"Printer failed to show any printing " +
                        $"activity for {StuckTimeoutSeconds}s.";
                }


                Debug.LogWarning
                (
                    $"[{Printer.Name}] {FailureMessage}"
                );


                //--------------------------------------------------
                // Cancel the stuck spooler job.
                //--------------------------------------------------

                bool Cancelled =
                    Win32PrintSpooler.CancelJob
                    (
                        Printer.Name,
                        JobId
                    );


                if (Cancelled)
                {
                    Debug.Log
                    (
                        $"[{Printer.Name}] Cancelled stuck " +
                        $"JobId {JobId}."
                    );
                }
                else
                {
                    Debug.LogWarning
                    (
                        $"[{Printer.Name}] Failed to cancel " +
                        $"JobId {JobId}."
                    );
                }


                //--------------------------------------------------
                // Report failure exactly once.
                //--------------------------------------------------

                CompleteOnce
                (
                    false,
                    FailureMessage
                );


                yield break;
            }


            //--------------------------------------------------
            // Poll again.
            //--------------------------------------------------

            yield return
                new WaitForSeconds
                (
                    PollInterval
                );
        }
    }
}