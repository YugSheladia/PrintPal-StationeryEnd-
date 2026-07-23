using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PrintManager : MonoBehaviour
{
    [SerializeField] Scene3Backend Scene3BackendScript;



    public static PrintManager Instance;

    public PrintQueue Queue = new PrintQueue();

    public Printer SelectedPrinter;

    bool IsPrinting;





    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SelectedPrinter = new Printer()
        {
            //Name = "Canon E470 series"
            Name = "Canon MF3010 (Copy 2)"
        };
    }



    public PrintJob CreatePrintJob(OrderSummary Order, Printer Printer)
    {
        PrintJob Job = new PrintJob();

        Job.OrderID = Order.OrderID;
        Job.Printer = Printer;

        Job.Status = PrintJobStatus.Waiting;
        Job.StatusMessage = "Waiting...";

        foreach (PrintDetails Print in Order.Prints)
        {
            if (string.IsNullOrEmpty(Print.LocalPDFPath))
            {
                Debug.LogError
                (
                    $"Print {Print.PrintID} has no downloaded PDF."
                );

                return null;
            }

            // TotalDocuments / TotalPages / PagesPrinted / Progress on
            // PrintJob are all computed from Documents, so they don't
            // (and can't) get assigned here directly.

            PrintDocument Document = new PrintDocument(Print);

            Job.Documents.Add(Document);
        }

        return Job;
    }





    public void QueuePrintJob(PrintJob Job)
    {
        if (Queue.ContainsOrder(Job.OrderID))
        {
            Debug.Log
            (
                $"Order {Job.OrderID} is already queued."
            );

            return;
        }

        Queue.Enqueue(Job);

        Debug.Log
        (
            $"Queued Order {Job.OrderID}"
        );



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

            Debug.Log("Print Queue is empty.");

            return;
        }

        IsPrinting = true;

        PrintJob Job = Queue.Peek();

        Debug.Log($"Starting Order {Job.OrderID}");

        StartCoroutine
        (
            Coroutine_PrintJob(Job)
        );
    }

    IEnumerator Coroutine_PrintJob(PrintJob Job)
    {
        Debug.Log($"Printing Order {Job.OrderID}");

        Job.Status = PrintJobStatus.Printing;
        Job.StatusMessage = "Printing...";

        foreach (PrintDocument Document in Job.Documents)
        {
            bool Success = false;
            string FailureMessage = "";

            yield return StartCoroutine
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
                Debug.LogError
                (
                    $"Failed to print PrintID {Document.Print.PrintID}: {FailureMessage}"
                );

                Document.Status = PrintDocumentStatus.Failed;
                Document.StatusMessage = FailureMessage;

                Job.Status = PrintJobStatus.Failed;
                Job.StatusMessage = FailureMessage + "\nJob failed while printing " + Document.Print.PrintID;

                Queue.Dequeue();

                StartNextJob();

                yield break;
            }

            Document.Status = PrintDocumentStatus.Completed;
            Document.StatusMessage = "Completed";

            // Job.Progress is computed from Documents/PagesPrinted, so
            // it's already accurate here - no manual bump needed.
            Debug.Log("Progress = " + Job.Progress);
        }

        Job.Status = PrintJobStatus.Completed;
        Job.StatusMessage = "Completed";

        Queue.Dequeue();

        Debug.Log($"Finished Order {Job.OrderID}");

        yield return StartCoroutine
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
        Debug.Log("Printing PrintID = " + Document.Print.PrintID);

        Document.StatusMessage = "Submitting to printer...";

        //--------------------------------------------------
        // Submit to Windows.
        //--------------------------------------------------

        PrintResult Result =
            WindowsPrinter.Print
            (
                Document,
                SelectedPrinter
            );

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
            $"Submitted PrintID {Document.Print.PrintID} to the Windows print queue. Watching for completion..."
        );

        //--------------------------------------------------
        // Don't call this "done" yet - Sumatra exiting only means
        // Windows accepted the job, not that it finished printing.
        // Watch the actual spooler job through to completion.
        //--------------------------------------------------

        Document.Status = PrintDocumentStatus.Printing;
        Document.StatusMessage = "Printing...";

        bool Success = false;
        string Message = "";

        yield return StartCoroutine
        (
            PrintMonitor.MonitorPrintJob
            (
                SelectedPrinter,
                Document,
                OnProgress: (PagesPrinted, TotalPages) =>
                {
                    Document.PagesPrinted = PagesPrinted;
                },
                OnPaused: (Reason, PauseMessage) =>
                {
                    // Something needs the user's attention (paper,
                    // offline, jam...). Surface it on the job without
                    // killing the queue - it'll auto-resume if fixed.
                    Job.Status = PrintJobStatus.Paused;
                    Job.StatusMessage = PauseMessage;
                    Document.StatusMessage = PauseMessage;

                    Debug.LogWarning($"Order {Job.OrderID} paused: {PauseMessage}");
                },
                OnResumed: () =>
                {
                    Job.Status = PrintJobStatus.Printing;
                    Job.StatusMessage = "Printing...";
                    Document.StatusMessage = "Printing...";

                    Debug.Log($"Order {Job.OrderID} resumed.");
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