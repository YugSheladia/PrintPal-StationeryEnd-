using System.Collections;
using System.IO;
using UnityEngine;

public static class PrintMonitor
{
    //--------------------------------------------------
    // Wait until a document finishes printing.
    //--------------------------------------------------

    public static IEnumerator WaitForCompletion
    (
        Printer Printer,
        PrintDocument Document,
        System.Action<bool, string> OnComplete
    )
    {
        string DocumentName =
            Path.GetFileName(Document.Print.LocalPDFPath);

        Debug.Log
        (
            $"Monitoring print job: {DocumentName}"
        );

        //--------------------------------------------------
        // Wait until Windows actually receives the job.
        //--------------------------------------------------

        float Timeout = 10f;
        float Timer = 0f;

        while
        (
            !Win32PrintSpooler.IsDocumentInQueue
            (
                Printer.Name,
                DocumentName
            )
        )
        {
            Timer += 0.25f;

            if (Timer >= Timeout)
            {
                OnComplete
                (
                    false,
                    "Print job never entered Windows queue."
                );

                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log("Job entered queue.");

        //--------------------------------------------------
        // Wait until Windows removes it.
        //--------------------------------------------------

        while
        (
            Win32PrintSpooler.IsDocumentInQueue
            (
                Printer.Name,
                DocumentName
            )
        )
        {
            var Job =
                Win32PrintSpooler.FindJob
                (
                    Printer.Name,
                    DocumentName
                );

            if (Job.HasValue)
            {
                int Status = Job.Value.Status;

                if
                (
                    (Status & Win32Structures.JOB_STATUS_ERROR) != 0
                )
                {
                    OnComplete(false, "Printer reported an error.");

                    yield break;
                }

                if
                (
                    (Status & Win32Structures.JOB_STATUS_OFFLINE) != 0
                )
                {
                    OnComplete(false, "Printer is offline.");

                    yield break;
                }

                if
                (
                    (Status & Win32Structures.JOB_STATUS_PAPEROUT) != 0
                )
                {
                    OnComplete(false, "Printer is out of paper.");

                    yield break;
                }

                if
                (
                    (Status & Win32Structures.JOB_STATUS_USER_INTERVENTION) != 0
                )
                {
                    OnComplete(false, "Printer requires user intervention.");

                    yield break;
                }

                Debug.Log
                (
                    $"Printing... {Job.Value.PagesPrinted}/{Job.Value.TotalPages}"
                );
            }

            yield return new WaitForSeconds(0.5f);
        }

        //--------------------------------------------------
        // Finished.
        //--------------------------------------------------

        Debug.Log("Document finished printing.");

        OnComplete
        (
            true,
            "Finished"
        );
    }
}