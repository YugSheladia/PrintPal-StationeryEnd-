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
            Name = "Canon E470 series"
        };
    }



    public PrintJob CreatePrintJob ( OrderSummary Order, Printer Printer )
    {
        PrintJob Job = new PrintJob();

        Job.OrderID = Order.OrderID;
        Job.Printer = Printer;

        Job.TotalDocuments = Order.Prints.Count;
        Job.DocumentsPrinted = 0;

        Job.TotalPages = 0;
        Job.PagesPrinted = 0;

        Job.Progress = 0f;

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

            Job.TotalPages += Print.Pages;

            PrintDocument Document = new PrintDocument(Print);

            Document.Print = Print;

            Job.Documents.Add(Document);
        }

        return Job;
    }





    public void QueuePrintJob ( PrintJob Job )
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
            //StartNextJob();
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

    IEnumerator Coroutine_PrintJob( PrintJob Job )
    {
        Debug.Log($"Printing Order {Job.OrderID}");

        Job.Status = PrintJobStatus.Printing;

        foreach (PrintDocument Document in Job.Documents)
        {
            bool Success = false;

            yield return StartCoroutine
            (
                Coroutine_PrintDocument
                (
                    Document,
                    Result =>
                    {
                        Success = Result;
                    }
                )
            );

            if (!Success)
            {
                Debug.LogError
                (
                    $"Failed to print PrintID {Document.Print.PrintID}"
                );

                Job.Status = PrintJobStatus.Failed;

                Queue.Dequeue();

                StartNextJob();

                yield break;
            }

            Job.Progress =
                (float)(Job.Documents.IndexOf(Document) + 1)
                / Job.Documents.Count;
        }

        Job.Status = PrintJobStatus.Completed;

        Queue.Dequeue();

        Debug.Log($"Finished Order {Job.OrderID}");

        yield return StartCoroutine
        (
            Scene3BackendScript.CompletePrintedOrder(Job.OrderID)
        );

        StartNextJob();
    }

    IEnumerator Coroutine_PrintDocument ( PrintDocument Document, Action<bool> OnComplete )
    {
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

            OnComplete(false);

            yield break;
        }

        //--------------------------------------------------
        // Wait until Windows finishes printing.
        //--------------------------------------------------

        bool Finished = false;
        bool Success = false;
        string Message = "";

        yield return StartCoroutine
        (
            PrintMonitor.WaitForCompletion
            (
                SelectedPrinter,
                Document,
                (ResultSuccess, ResultMessage) =>
                {
                    Finished = true;
                    Success = ResultSuccess;
                    Message = ResultMessage;
                }
            )
        );

        while (!Finished)
        {
            yield return null;
        }

        if (!Success)
        {
            Debug.LogError(Message);

            OnComplete(false);

            yield break;
        }

        Debug.Log
        (
            $"Finished printing PrintID {Document.Print.PrintID}"
        );

        OnComplete(true);
    }
}