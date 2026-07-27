using System;
using System.Collections;
using UnityEngine;

// Owns one physical printer's queue and print loop. PrintManager creates
// one of these per printer and dispatches jobs into them; they run fully
// independently of each other, which is what lets N printers work in
// parallel instead of a single global queue serializing everything.
public class PrinterWorker
{
    public Printer Printer;
    public PrintQueue Queue = new PrintQueue();

    readonly PrintManager Manager;
    readonly Scene3Backend Scene3BackendScript;

    bool IsPrinting;

    public PrinterWorker(PrintManager Manager, Scene3Backend Scene3BackendScript, Printer Printer)
    {
        this.Manager = Manager;
        this.Scene3BackendScript = Scene3BackendScript;
        this.Printer = Printer;
    }

    public void Enqueue(PrintJob Job)
    {
        Job.Printer = Printer;

        Queue.Enqueue(Job);

        Debug.Log($"Queued Order {Job.OrderID} on {Printer.Name}");

        if (!IsPrinting)
        {
            StartNextJob();
        }
    }

    void StartNextJob()
    {
        if (!Queue.HasJobs())
        {
            IsPrinting = false;
            Printer.IsBusy = false;
            return;
        }

        IsPrinting = true;
        Printer.IsBusy = true;

        PrintJob Job = Queue.Peek();

        Debug.Log($"Starting Order {Job.OrderID} on {Printer.Name}");

        Manager.StartCoroutine(Coroutine_PrintJob(Job));
    }

    IEnumerator Coroutine_PrintJob(PrintJob Job)
    {
        Job.Status = PrintJobStatus.Printing;
        Job.StatusMessage = "Printing...";

        foreach (PrintDocument Document in Job.Documents)
        {
            // If this job got rerouted here after partially printing on
            // another printer, don't redo the documents that already
            // finished there.
            if (Document.Status == PrintDocumentStatus.Completed)
            {
                continue;
            }

            bool Success = false;
            string FailureMessage = "";

            yield return Manager.StartCoroutine
            (
                Coroutine_PrintDocument
                (
                    Job,
                    Document,
                    (ResultSuccess, ResultMessage) =>
                    {
                        Success = ResultSuccess;
                        FailureMessage = ResultMessage;
                    }
                )
            );

            if (!Success)
            {
                Debug.LogWarning
                (
                    $"Failed to print PrintID {Document.Print.PrintID} on {Printer.Name}: {FailureMessage}"
                );

                Document.Status = PrintDocumentStatus.Failed;
                Document.StatusMessage = FailureMessage;

                Queue.Dequeue();

                // Hand off to PrintManager to try another printer, rather
                // than failing the whole order outright.
                Manager.RerouteFailedJob(Job, this, FailureMessage);

                StartNextJob();

                yield break;
            }

            Document.Status = PrintDocumentStatus.Completed;
            Document.StatusMessage = "Completed";

            Debug.Log($"[{Printer.Name}] Progress = " + Job.Progress);
        }

        Job.Status = PrintJobStatus.Completed;
        Job.StatusMessage = "Completed";

        Queue.Dequeue();

        Debug.Log($"Finished Order {Job.OrderID} on {Printer.Name}");

        yield return Manager.StartCoroutine
        (
            Scene3BackendScript.CompletePrintedOrder(Job.OrderID)
        );

        StartNextJob();
    }

    IEnumerator Coroutine_PrintDocument
    (
        PrintJob Job,
        PrintDocument Document,
        Action<bool, string> OnComplete
    )
    {
        Debug.Log($"[{Printer.Name}] Printing PrintID = " + Document.Print.PrintID);

        Document.StatusMessage = "Submitting to printer...";

        //--------------------------------------------------
        // Submit to Windows.
        //--------------------------------------------------

        PrintResult Result = WindowsPrinter.Print(Document, Printer);

        if (!Result.Success)
        {
            Debug.LogError(Result.Message);

            if (Result.Exception != null)
            {
                Debug.LogException(Result.Exception);
            }

            OnComplete(false, Result.Message);

            yield break;
        }

        Debug.Log
        (
            $"[{Printer.Name}] Submitted PrintID {Document.Print.PrintID} to the Windows print queue. Watching for completion..."
        );

        Document.Status = PrintDocumentStatus.Printing;
        Document.StatusMessage = "Printing...";

        bool Success = false;
        string Message = "";

        yield return Manager.StartCoroutine
        (
            PrintMonitor.MonitorPrintJob
            (
                Printer,
                Document,
                OnProgress: (PagesPrinted, TotalPages) =>
                {
                    Document.PagesPrinted = PagesPrinted;
                },
                OnPaused: (Reason, PauseMessage) =>
                {
                    Job.Status = PrintJobStatus.Paused;
                    Job.StatusMessage = $"[{Printer.Name}] {PauseMessage}";
                    Document.StatusMessage = PauseMessage;

                    Debug.LogWarning($"Order {Job.OrderID} paused on {Printer.Name}: {PauseMessage}");
                },
                OnResumed: () =>
                {
                    Job.Status = PrintJobStatus.Printing;
                    Job.StatusMessage = "Printing...";
                    Document.StatusMessage = "Printing...";

                    Debug.Log($"Order {Job.OrderID} resumed on {Printer.Name}.");
                },
                OnComplete: (ResultSuccess, ResultMessage) =>
                {
                    Success = ResultSuccess;
                    Message = ResultMessage;
                }
            )
        );

        OnComplete(Success, Message);
    }
}