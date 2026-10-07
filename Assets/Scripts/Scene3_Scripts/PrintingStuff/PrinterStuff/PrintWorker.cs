//namespaces =
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;







// Owns one physical printer's queue and print loop.
// PrintManager creates a PrinterWorker object per printer and dispatches jobs into PrintWorker.
// PrintWorkers run fully independently of each other, which is what lets N printers work in
// parallel instead of a single global queue serializing everything.

public class PrinterWorker
{
    //Reference to the main Printer object.
    public Printer Printer;

    //This is the worker's Queue =
    public PrintQueue Queue = new PrintQueue();




    //Reference to important Scripts =
    readonly PrintManager Manager;
    readonly Scene3Backend Scene3BackendScript;



    //Bool which is used to figure if the Printer is currenlty printing or not.
    bool IsPrinting;



    //Bool to figure out whether the printer is working or not.
    public bool WorkedLastTime = true;



    //Constructor =
    public PrinterWorker(PrintManager Manager, Scene3Backend Scene3BackendScript, Printer Printer)
    {
        this.Manager = Manager;
        this.Scene3BackendScript = Scene3BackendScript;
        this.Printer = Printer;
    }






    //=====================================================================================
    //Enqueue() -> StartNextJob() -> Couroutine_PrintJob() -> Coroutine_PrintDocuemnt()
    //=====================================================================================

    //This function is used to Enqueue a job in this PrintWorker's Queue.
    public void Enqueue(PrintJob Job)
    {
        //Providing Job a reference to the Printer, which is going to print it.
        Job.Printer = Printer;

        //Enqueing the Job.
        Queue.Enqueue(Job);

        Debug.Log($"Queued Order {Job.OrderID} on {Printer.Name}");

        //If the printer isn't printing anything currently, we StartNextJob().
        if (!IsPrinting)
        {
            StartNextJob();
        }
    }

    //To Start the next job.
    void StartNextJob()
    {
        //==================================================
        // NEVER start another job while this printer is
        // already processing one.
        //==================================================

        if (IsPrinting)
        {
            return;
        }


        //==================================================
        // NEVER start a job if this printer is marked
        // as non-working.
        //
        // This covers both:
        //
        // 1. Automatic failure detection.
        // 2. Manual DisablePrinter().
        //==================================================

        if (!WorkedLastTime)
        {
            Debug.Log
            (
                $"[{Printer.Name}] cannot start next job " +
                $"because WorkedLastTime = false."
            );

            Printer.IsBusy = false;

            return;
        }


        //==================================================
        // Nothing queued.
        //==================================================

        if (!Queue.HasJobs())
        {
            IsPrinting = false;
            Printer.IsBusy = false;

            return;
        }


        //==================================================
        // Lock this worker before starting the coroutine.
        //==================================================

        IsPrinting = true;
        Printer.IsBusy = true;

        //This returns the Job which is in the front of the Queue.
        PrintJob Job =
            Queue.Peek();


        Debug.Log
        (
            $"Starting Order {Job.OrderID} on {Printer.Name}"
        );


        Manager.StartCoroutine
        (
            Coroutine_PrintJob(Job)
        );
    }

    //StartNextJob() -> Coroutine_PrintJob().
    IEnumerator Coroutine_PrintJob(PrintJob Job)
    {
        //--------------------------------------------------
        // Safety check.
        //
        // A printer may have been manually disabled between
        // Enqueue() and actually starting the job.
        //--------------------------------------------------

        if (!WorkedLastTime)
        {
            Debug.LogWarning
            (
                $"[{Printer.Name}] refused to start " +
                $"Order {Job.OrderID} because the printer " +
                $"is marked as NOT WORKING."
            );

            IsPrinting = false;
            Printer.IsBusy = false;

            yield break;
        }


        Job.Status =
            PrintJobStatus.Printing;

        Job.StatusMessage =
            "Printing...";


        //--------------------------------------------------
        // Process every document in this order ONE BY ONE.
        //--------------------------------------------------

        foreach (PrintDocument Document in Job.Documents)
        {
            if
            (
                Document.Status ==
                PrintDocumentStatus.Completed
            )
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
                        Success =
                            ResultSuccess;

                        FailureMessage =
                            ResultMessage;
                    }
                )
            );


            //--------------------------------------------------
            // Current document failed.
            //--------------------------------------------------

            if (!Success)
            {
                Debug.LogWarning
                (
                    $"Failed to print PrintID " +
                    $"{Document.Print.PrintID} on " +
                    $"{Printer.Name}: " +
                    $"{FailureMessage}"
                );


                Document.Status =
                    PrintDocumentStatus.Failed;

                Document.StatusMessage =
                    FailureMessage;


                //--------------------------------------------------
                // Remove the failed CURRENT job from the worker.
                //--------------------------------------------------

                PrintJob RemovedJob =
                    Queue.Dequeue();



                if (RemovedJob != Job)
                {
                    Debug.LogError
                    (
                        $"Queue mismatch on {Printer.Name}. " +
                        $"Expected Order {Job.OrderID} " +
                        $"but dequeued a different job."
                    );
                }


                //--------------------------------------------------
                // Mark printer failed.
                //
                // This redistributes ONLY the jobs remaining
                // behind this current job.
                //--------------------------------------------------

                SetPrinterAsNotWorking();


                //--------------------------------------------------
                // Reroute the CURRENT failed order.
                //--------------------------------------------------

                Manager.RerouteFailedJob
                (
                    Job,
                    this,
                    FailureMessage
                );


                //--------------------------------------------------
                // Release worker.
                //--------------------------------------------------

                IsPrinting = false;
                Printer.IsBusy = false;


                //--------------------------------------------------
                // Will refuse to start because this printer
                // is now marked as non-working.
                //--------------------------------------------------

                StartNextJob();

                yield break;
            }


            //--------------------------------------------------
            // Document successfully completed.
            //--------------------------------------------------

            Document.Status =
                PrintDocumentStatus.Completed;

            Document.StatusMessage =
                "Completed";


            Debug.Log
            (
                $"[{Printer.Name}] " +
                $"PrintID {Document.Print.PrintID} " +
                $"completed."
            );


            Debug.Log
            (
                $"[{Printer.Name}] " +
                $"Order {Job.OrderID} Progress = " +
                $"{Job.Progress}"
            );
        }


        //--------------------------------------------------
        // Entire PrintJob completed.
        //--------------------------------------------------

        Job.Status =
            PrintJobStatus.Completed;

        Job.StatusMessage =
            "Completed";


        PrintJob CompletedJob =
            Queue.Dequeue();


        if (CompletedJob != Job)
        {
            Debug.LogError
            (
                $"Queue mismatch on {Printer.Name}. " +
                $"Expected Order {Job.OrderID} " +
                $"but dequeued a different job."
            );
        }


        Debug.Log
        (
            $"Finished Order {Job.OrderID} " +
            $"on {Printer.Name}"
        );


        //--------------------------------------------------
        // Backend completion happens ONLY after the entire
        // physical order has completed.
        //--------------------------------------------------

        yield return Manager.StartCoroutine
        (
            Scene3BackendScript.CompletePrintedOrder
            (
                Job.OrderID
            )
        );


        //--------------------------------------------------
        // Successful printing means this printer is working.
        //
        // This is important because a previously failed
        // printer can only become automatically "working"
        // again after a successful print.
        //--------------------------------------------------

        SetPrinterAsWorking();


        //--------------------------------------------------
        // Release worker.
        //--------------------------------------------------

        IsPrinting = false;
        Printer.IsBusy = false;


        StartNextJob();
    }

    //Coroutine_PrintJob() -> Coroutine_PrintDocument() for each document inside PrintJob.
    IEnumerator Coroutine_PrintDocument
    (
        PrintJob Job,
        PrintDocument Document,
        Action<bool, string> OnComplete
    )
    {
        Debug.Log
        (
            $"[{Printer.Name}] " +
            $"Printing PrintID = " +
            $"{Document.Print.PrintID}"
        );


        Document.StatusMessage =
            "Submitting to printer...";


        //--------------------------------------------------
        // Submit exactly ONE request to Windows.
        //
        // This now runs on a background thread via
        // WindowsPrinter.PrintAsync(), so a printer that
        // hangs (e.g. an unreachable Bluetooth device) no
        // longer blocks Unity's main thread. We just wait
        // for the Task to finish, yielding every frame.
        //--------------------------------------------------

        Task<PrintResult> PrintTask =
            WindowsPrinter.PrintAsync
            (
                Document,
                Printer
            );


        yield return new WaitUntil
        (
            () => PrintTask.IsCompleted
        );


        PrintResult Result =
            PrintTask.Result;


        //--------------------------------------------------
        // Sumatra / Windows submission failed.
        //--------------------------------------------------

        if (!Result.Success)
        {
            Debug.LogError
            (
                Result.Message
            );


            if (Result.Exception != null)
            {
                Debug.LogException
                (
                    Result.Exception
                );
            }


            OnComplete
            (
                false,
                Result.Message
            );


            yield break;
        }


        Debug.Log
        (
            $"[{Printer.Name}] " +
            $"Submitted PrintID " +
            $"{Document.Print.PrintID} " +
            "to the Windows print queue. " +
            "Watching for completion..."
        );


        Document.Status =
            PrintDocumentStatus.Printing;

        Document.StatusMessage =
            "Printing...";


        //--------------------------------------------------
        // These values are populated ONLY by PrintMonitor.
        //--------------------------------------------------

        bool Success = false;
        string Message = "";

        bool MonitorFinished = false;


        //--------------------------------------------------
        // Monitor THIS ONE physical print request.
        //--------------------------------------------------

        yield return Manager.StartCoroutine
        (
            PrintMonitor.MonitorPrintJob
            (
                Printer,
                Document,

                OnProgress:
                (
                    PagesPrinted,
                    TotalPages
                ) =>
                {
                    Document.PagesPrinted =
                        PagesPrinted;
                },


                OnPaused:
                (
                    Reason,
                    PauseMessage
                ) =>
                {
                    Job.Status =
                        PrintJobStatus.Paused;

                    Job.StatusMessage =
                        $"[{Printer.Name}] {PauseMessage}";

                    Document.StatusMessage =
                        PauseMessage;


                    Debug.LogWarning
                    (
                        $"Order {Job.OrderID} " +
                        $"paused on {Printer.Name}: " +
                        $"{PauseMessage}"
                    );
                },


                OnResumed:
                () =>
                {
                    Job.Status =
                        PrintJobStatus.Printing;

                    Job.StatusMessage =
                        "Printing...";

                    Document.StatusMessage =
                        "Printing...";


                    Debug.Log
                    (
                        $"Order {Job.OrderID} " +
                        $"resumed on {Printer.Name}."
                    );
                },


                OnComplete:
                (
                    ResultSuccess,
                    ResultMessage
                ) =>
                {
                    Success =
                        ResultSuccess;

                    Message =
                        ResultMessage;

                    MonitorFinished =
                        true;
                }
            )
        );


        //--------------------------------------------------
        // Safety check.
        //--------------------------------------------------

        if (!MonitorFinished)
        {
            Debug.LogError
            (
                $"[{Printer.Name}] " +
                $"PrintMonitor returned without " +
                $"reporting completion for PrintID " +
                $"{Document.Print.PrintID}."
            );


            OnComplete
            (
                false,
                "Print monitor ended without reporting completion."
            );


            yield break;
        }


        //--------------------------------------------------
        // Report pages used - ONLY on a confirmed success.
        //--------------------------------------------------

        if (Success)
        {
            Debug.Log("Starting = 1");
            int PagesUsed =
                Document.Print.EffectiveSheets *
                Document.Print.Copies;

            Debug.Log("2");
            Scene3BackendScript.PagesUsedUpdationProcess
            (
                Printer.StationeryPrinterID,
                PagesUsed,
                this
            );
        }
        

        //--------------------------------------------------
        // Current physical print request is finished.
        //--------------------------------------------------

        OnComplete
        (
            Success,
            Message
        );
    }












    //===================================================================================================================
    //Called by Scene3Backend's script -> PagesUsedUpdationProccess_Coroutine() to locally update the PagesLeft of a
    //stationery once the PagesUsedUpdationProcess() has been completed successfully.
    //===================================================================================================================

    //===========================================================================================================================
    //flow =
    //Coroutine_PrintDocument() -> Scene3BackendScript.PagesLeftUpdationProcess() -> PagesLeftUpdationProcess_Coroutine() ->
    //UpdatePagesUsed() + GetPagesLeft() + UpdatePagesLeft_Locally()
    //===========================================================================================================================
    public void UpdatePagesLeft_Locally(int PagesLeft)
    {
        Printer.PagesLeft = PagesLeft;

        Manager.QueueManagerScript.RefreshQueuedOrders();
    }









    //This function is called when -
    //a. A printer succesfully prints all documents of an order. ( i.e. by Coroutine_PrintJob() )
    //OR
    //b. When a stationery sets it as Operational through PrinterQueuePanel ( i.e. by PrintManager script -> JumpStartPrinter() ).
    public void SetPrinterAsWorking()
    {
        WorkedLastTime = true;

        Printer.IsOnline = true;


        Debug.Log
        (
            $"[{Printer.Name}] marked as WORKING."
        );


        //--------------------------------------------------
        // If this printer has jobs waiting in its queue,
        // allow it to resume processing them.
        //--------------------------------------------------

        if (!IsPrinting && Queue.HasJobs())
        {
            StartNextJob();
        }
    }

    //This function is called when -
    //a. A printer is not able to prints any documents of an order. ( i.e. by Coroutine_PrintJob() )
    //OR
    //b. When a stationery sets a printer as Non Operational through PrinterQueuePanel ( i.e. by PrintManager script -> DisablePrinter() ).
    public void SetPrinterAsNotWorking()
    {
        //--------------------------------------------------
        // Already marked as non-working.
        //--------------------------------------------------

        if (!WorkedLastTime)
        {
            Debug.Log
            (
                $"[{Printer.Name}] is already marked " +
                $"as NOT WORKING."
            );

            return;
        }


        //--------------------------------------------------
        // Mark printer unavailable immediately.
        //--------------------------------------------------

        WorkedLastTime = false;

        Printer.IsOnline = false;


        Debug.LogWarning
        (
            $"[{Printer.Name}] marked as NOT WORKING."
        );


        //--------------------------------------------------
        // IMPORTANT:
        //
        // We DO NOT interrupt the currently printing order
        // here.
        //
        // If this method was triggered by PrintMonitor,
        // the current job has already failed and has already
        // been removed from the queue.
        //
        // If this method was triggered manually while the
        // printer is currently printing, the current physical
        // job is allowed to finish, but this printer will NOT
        // receive another order afterward.
        //
        // Waiting orders are redistributed immediately.
        //--------------------------------------------------

        Manager.RedistributeJobsFromWorker(this);
    }




    //This is called by PrintManager script -> JumpStartPrinter() incase the Printer had any Waiting orders.
    public void TryStartNextJob()
    {
        StartNextJob();
    }
}