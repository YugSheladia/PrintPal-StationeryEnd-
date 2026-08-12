using System.Collections.Generic;
using UnityEngine;


public class PrintManager : MonoBehaviour
{
    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;

    [SerializeField] QueueManager QueueManagerScript;



    public static PrintManager Instance;

    private PrintJob LastFailedJob;

    // One worker per physical printer. Each worker owns its own queue and
    // runs independently, so all printers can be busy at once.
    public List<PrinterWorker> Workers = new List<PrinterWorker>();

    int NextWorkerIndex = 0;



    //Resume revising the architecture from here.
    private readonly HashSet<int> ProcessingOrders = new();
    public bool IsOrderBeingProcessed(int OrderID)
    {
        return ProcessingOrders.Contains(OrderID);
    }

    public void BeginProcessingOrder(int OrderID)
    {
        ProcessingOrders.Add(OrderID);
    }

    public void EndProcessingOrder(int OrderID)
    {
        ProcessingOrders.Remove(OrderID);
    }

    public bool IsOrderAlreadyHandled(int OrderID)
    {
        return
                ProcessingOrders.Contains(OrderID)
                ||
                AnyWorkerHasOrder(OrderID);
    }

    public bool AnyWorkerHasOrder(int OrderID)
    {
        foreach (PrinterWorker Worker in Workers)
        {
            if (Worker.Queue.ContainsOrder(OrderID))
            {
                return true;
            }
        }

        return false;
    }

    public void EmptyProcessingOrdersHashSet()
    {
        ProcessingOrders.Clear();
    }








    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshWorkersFromApprovedPrinters();
    }

    // Rebuilds the worker list from Script1.SelectedPrinters (the approved
    // printer list fetched at login via Scene2Backend/Scene5Backend's
    // GetApprovedPrintersFromServer). Call this again from those scripts
    // right after Script1.SetPrinters(...) if PrintManager is a persistent
    // singleton that might Start() before login finishes, or whenever the
    // stationery's approved-printer list changes mid-session.
    public void RefreshWorkersFromApprovedPrinters()
    {
        //--------------------------------------------------
        // Preserve the current working/non-working state.
        //
        // This is important because RefreshWorkersFromApprovedPrinters()
        // should NOT automatically JumpStart a printer that the operator
        // intentionally disabled.
        //--------------------------------------------------

        Dictionary<string, bool> PreviousWorkingStates =
            new Dictionary<string, bool>();

        foreach (PrinterWorker ExistingWorker in Workers)
        {
            if (ExistingWorker == null || ExistingWorker.Printer == null)
            {
                continue;
            }

            string Key = ExistingWorker.Printer.PrinterID;

            if (string.IsNullOrEmpty(Key))
            {
                Key = ExistingWorker.Printer.Name;
            }

            if (!string.IsNullOrEmpty(Key))
            {
                PreviousWorkingStates[Key] =
                    ExistingWorker.WorkedLastTime;
            }
        }


        //--------------------------------------------------
        // Rebuild workers.
        //--------------------------------------------------

        Workers.Clear();

        NextWorkerIndex = 0;


        foreach (Printer P in Script1.SelectedPrinters)
        {
            if (!P.IsSelected)
            {
                continue;
            }


            //--------------------------------------------------
            // The server may only provide Name.
            //--------------------------------------------------

            if (string.IsNullOrEmpty(P.PrinterID))
            {
                P.PrinterID = P.Name;
            }


            //--------------------------------------------------
            // Create worker.
            //--------------------------------------------------

            PrinterWorker Worker =
                new PrinterWorker
                (
                    this,
                    Scene3BackendScript,
                    P
                );


            //--------------------------------------------------
            // Restore previous manual/failure state.
            //
            // New printers default to working.
            //--------------------------------------------------

            string Key = P.PrinterID;

            if (string.IsNullOrEmpty(Key))
            {
                Key = P.Name;
            }


            if
            (
                !string.IsNullOrEmpty(Key) &&
                PreviousWorkingStates.TryGetValue
                (
                    Key,
                    out bool PreviousState
                )
            )
            {
                Worker.WorkedLastTime =
                    PreviousState;

                Worker.Printer.IsOnline =
                    PreviousState;
            }
            else
            {
                Worker.WorkedLastTime =
                    true;
            }


            Workers.Add(Worker);
        }


        //--------------------------------------------------
        // Reset round-robin position.
        //--------------------------------------------------

        NextWorkerIndex = 0;


        if (Workers.Count == 0)
        {
            Debug.LogWarning
            (
                "No approved/selected printers found for this stationery."
            );
        }
    }









    public PrintJob CreatePrintJob(OrderSummary Order)
    {
        PrintJob Job = new PrintJob();

        Job.OrderID = Order.OrderID;

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

                //What should happen when the path of a PDF to be printed is null?

                return null;
            }

            PrintDocument Document = new PrintDocument(Print);

            Job.Documents.Add(Document);
        }

        // Note: Job.Printer isn't set here anymore - it gets assigned once
        // QueuePrintJob picks which worker/printer will handle it.

        return Job;
    }

    //Old QueuePrintJob()
    /*
    public bool QueuePrintJob(PrintJob Job)
    {
        if (AnyWorkerHasOrder(Job.OrderID))
        {
            return false;
        }

        PrinterWorker Worker = PickNextWorker();

        if (Worker == null)
        {
            Job.Status = PrintJobStatus.Failed;
            Job.StatusMessage = "No printers available.";

            return false;
        }

        Worker.Enqueue(Job);

        QueueManagerScript.RefreshQueuedOrders();

        return true;
    }
    */

    //New QueuePrintJob()
    public bool QueuePrintJob(PrintJob Job)
    {
        if (Job == null)
        {
            Debug.LogError
            (
                "Cannot queue a null PrintJob."
            );

            return false;
        }


        //--------------------------------------------------
        // Select a working printer.
        //--------------------------------------------------

        PrinterWorker Worker =
            PickNextWorker();


        if (Worker == null)
        {
            Job.Status =
                PrintJobStatus.Failed;

            Job.StatusMessage =
                "No working printers available.";

            return false;
        }


        //--------------------------------------------------
        // Give the complete ORDER to the selected printer.
        //
        // PrinterWorker will process all PrintDocuments inside
        // this PrintJob sequentially.
        //--------------------------------------------------

        Worker.Enqueue(Job);


        QueueManagerScript.RefreshQueuedOrders();


        return true;
    }









    //--------------------------------------------------
    // Round-robin dispatch: Order#1 -> Printer#1, Order#2 -> Printer#2,
    // ... wrapping back around once every printer's had a turn.
    //
    // Rather than trusting Printer.IsOnline (which only reflects a status
    // string fetched once at login, and goes stale if a printer's actual
    // state changes mid-session), each candidate is actively checked here
    // via a live OpenPrinter call before being handed a job - if it
    // doesn't respond, move on to the next one.
    //--------------------------------------------------



    //Old PickNextWorker()
    /*
    PrinterWorker PickNextWorker()
    {
        if (Workers.Count == 0)
        {
            Debug.LogWarning("0 printers available.");

            Scene3ManagerScript.MessageText.text = "0 printers connected\nGo to settings and connect a printer to begin printing.";

            Scene3ManagerScript.EnableMessagePanel();

            return null;
        }

        int Index = NextWorkerIndex % Workers.Count;

        PrinterWorker Candidate = Workers[Index];

        Debug.Log("Worker selected = " + Candidate.Printer.Name);

        NextWorkerIndex = (Index + 1) % Workers.Count;

        return Candidate;
    }
    */

    //New PickNextWorker()
    PrinterWorker PickNextWorker()
    {
        if (Workers.Count == 0)
        {
            Debug.LogWarning("0 printers available.");

            Scene3ManagerScript.MessageText.text =
                "0 printers connected\n" +
                "Go to settings and connect a printer to begin printing.";

            Scene3ManagerScript.EnableMessagePanel();

            return null;
        }


        //--------------------------------------------------
        // Search all workers starting from the current
        // round-robin position.
        //
        // Only printers marked as working are eligible.
        //--------------------------------------------------

        for (int i = 0; i < Workers.Count; i++)
        {
            int Index =
                (NextWorkerIndex + i) % Workers.Count;

            PrinterWorker Candidate =
                Workers[Index];


            if (Candidate == null)
            {
                continue;
            }


            //--------------------------------------------------
            // Printer has failed or has been manually disabled.
            //--------------------------------------------------

            if (!Candidate.WorkedLastTime)
            {
                Debug.Log
                (
                    $"Skipping {Candidate.Printer.Name} " +
                    $"because WorkedLastTime = false."
                );

                continue;
            }


            //--------------------------------------------------
            // Found an eligible printer.
            //--------------------------------------------------

            Debug.Log
            (
                $"Worker selected = " +
                $"{Candidate.Printer.Name}"
            );


            //--------------------------------------------------
            // Move round-robin pointer forward.
            //--------------------------------------------------

            NextWorkerIndex =
                (Index + 1) % Workers.Count;


            return Candidate;
        }


        //--------------------------------------------------
        // Every printer is currently unavailable.
        //--------------------------------------------------

        Debug.LogWarning
        (
            "No workers with WorkedLastTime = true are available."
        );

        //Just to make sure that when the PrintingProcess is paused, all queues are empty.
        ClearAllPrinterQueues();

        Scene3ManagerScript.MessageText.text =
            "No working printers available.\n" +
            "JumpStart a printer to continue printing.";

        //No Printer Available, so we are hitting pause on Automatic Printing.
        HomePageScript.PausePrinting();

        Scene3ManagerScript.EnableMessagePanel();

        return null;
    }

    public void RedistributeJobsFromWorker
    (
        PrinterWorker FailedWorker
    )
    {
        if (FailedWorker == null)
        {
            return;
        }


        Debug.LogWarning
        (
            $"Redistributing waiting jobs from " +
            $"{FailedWorker.Printer.Name}."
        );


        //--------------------------------------------------
        // The CURRENT failed job has already been removed
        // from FailedWorker.Queue by PrinterWorker.
        //
        // Therefore everything currently in this queue is
        // a WAITING order.
        //--------------------------------------------------

        List<PrintJob> JobsToRedistribute =
            FailedWorker.Queue.RemoveAllJobs();


        if (JobsToRedistribute.Count == 0)
        {
            Debug.Log
            (
                $"No waiting jobs to redistribute from " +
                $"{FailedWorker.Printer.Name}."
            );

            return;
        }


        //--------------------------------------------------
        // Redistribute using round-robin.
        //
        // Example:
        //
        // P1
        // P2 FAILED -> Order 5, Order 8
        // P3
        //
        // Order 5 -> P3
        // Order 8 -> P1
        //
        // This keeps redistribution even.
        //--------------------------------------------------

        foreach (PrintJob Job in JobsToRedistribute)
        {
            PrinterWorker Worker =
                PickNextWorker();


            //--------------------------------------------------
            // No working printer exists.
            //--------------------------------------------------

            if (Worker == null)
            {
                Debug.LogWarning
                (
                    $"Could not redistribute Order " +
                    $"{Job.OrderID}. " +
                    "No working printers are available."
                );


                //--------------------------------------------------
                // Keep the order on the failed worker.
                //
                // StartNextJob() will NOT process it because
                // WorkedLastTime == false.
                //--------------------------------------------------

                //Removing this line of code for now.
                //FailedWorker.Queue.Enqueue(Job);

                continue;
            }


            //--------------------------------------------------
            // Reset the PrintJob itself.
            //--------------------------------------------------

            Job.Status =
                PrintJobStatus.Waiting;

            Job.StatusMessage =
                $"Redistributed to {Worker.Printer.Name}...";


            //--------------------------------------------------
            // Reset unfinished documents.
            //
            // Documents already physically completed remain
            // completed and will not be printed again.
            //--------------------------------------------------

            foreach (PrintDocument Document in Job.Documents)
            {
                if
                (
                    Document.Status !=
                    PrintDocumentStatus.Completed
                )
                {
                    Document.Status =
                        PrintDocumentStatus.Waiting;

                    Document.PagesPrinted =
                        0;

                    Document.StatusMessage =
                        "Waiting to print...";
                }
            }


            //--------------------------------------------------
            // Give the order to the new printer.
            //--------------------------------------------------

            Worker.Enqueue(Job);


            Debug.Log
            (
                $"Redistributed Order {Job.OrderID} " +
                $"from {FailedWorker.Printer.Name} " +
                $"to {Worker.Printer.Name}."
            );
        }


        QueueManagerScript.RefreshQueuedOrders();
    }



    //For printers not supporting double sided printing ( Still havent integrated this )
    /*
    // Call this from the UI once the operator has physically flipped and
    // reloaded the stack for a manual duplex job (Document.Status ==
    // WaitingForManualDuplexFlip).
    public void ConfirmManualDuplexFlip(PrintJob Job)
    {
        PrinterWorker Worker = Workers.Find(W => W.Printer == Job.Printer);

        if (Worker == null)
        {
            Debug.LogError($"Couldn't find the worker printing Order {Job.OrderID} to confirm the duplex flip.");
            return;
        }

        Worker.ConfirmManualDuplexFlip();
    }
    */







    //--------------------------------------------------
    // Called by a PrinterWorker when it fails to print a job.
    // Tries to hand the job to the next available printer instead of
    // failing the order outright.
    //--------------------------------------------------
    public bool RerouteFailedJob
    (
        PrintJob Job,
        PrinterWorker FailedWorker,
        string Reason
    )
    {
        Debug.Log
        (
            "Inside RerouteFailedJob()"
        );


        if (Job == null)
        {
            Debug.LogError
            (
                "RerouteFailedJob() received a null Job."
            );

            return false;
        }


        if (FailedWorker == null)
        {
            Debug.LogError
            (
                $"Cannot reroute Order {Job.OrderID}: " +
                "FailedWorker is null."
            );

            return false;
        }


        //--------------------------------------------------
        // Remember that this printer has already been
        // attempted for this order.
        //--------------------------------------------------

        Job.AttemptedPrinterIDs.Add
        (
            FailedWorker.Printer.PrinterID
        );

        Job.RetryCount++;


        //--------------------------------------------------
        // Find another working printer.
        //--------------------------------------------------

        PrinterWorker NextWorker =
            PickNextAvailableWorker
            (
                Job,
                FailedWorker
            );


        //--------------------------------------------------
        // No other printer can handle this order.
        //--------------------------------------------------

        if (NextWorker == null)
        {
            Debug.LogWarning
            (
                $"Order {Job.OrderID} failed on every " +
                $"available printer. Last error: {Reason}"
            );


            Job.Status =
                PrintJobStatus.Failed;

            Job.StatusMessage =
                $"Failed on all printers: {Reason}";


            LastFailedJob =
                Job;


            EndProcessingOrder
            (
                Job.OrderID
            );


            HomePageScript.PausePrinting();


            QueueManagerScript.RefreshQueuedOrders();


            return false;
        }


        Debug.LogWarning
        (
            $"Order {Job.OrderID} failed on " +
            $"{FailedWorker.Printer.Name}. " +
            $"Rerouting to {NextWorker.Printer.Name}."
        );


        //--------------------------------------------------
        // Reset unfinished documents.
        //--------------------------------------------------

        foreach (PrintDocument Document in Job.Documents)
        {
            if
            (
                Document.Status !=
                PrintDocumentStatus.Completed
            )
            {
                Document.Status =
                    PrintDocumentStatus.Waiting;

                Document.PagesPrinted =
                    0;

                Document.StatusMessage =
                    "Waiting to retry on another printer...";
            }
        }


        //--------------------------------------------------
        // Reset job state.
        //--------------------------------------------------

        Job.Status =
            PrintJobStatus.Waiting;

        Job.StatusMessage =
            $"Rerouted to {NextWorker.Printer.Name}...";


        //--------------------------------------------------
        // Queue on new printer.
        //--------------------------------------------------

        NextWorker.Enqueue(Job);


        QueueManagerScript.RefreshQueuedOrders();


        return true;
    }

    // Starts searching from the printer *after* the one that just failed
    // (e.g. Printer#2 fails -> try Printer#3 next), skipping any printer
    // already attempted for this specific job, and wrapping around.
    // Same live-check approach as PickNextWorker - see comment there.
    PrinterWorker PickNextAvailableWorker
    (
        PrintJob Job,
        PrinterWorker FailedWorker
    )
    {
        if (Workers.Count == 0)
        {
            return null;
        }


        int FailedIndex =
            Workers.IndexOf(FailedWorker);


        //--------------------------------------------------
        // Search every printer after the failed printer,
        // wrapping around.
        //--------------------------------------------------

        for (int i = 1; i <= Workers.Count; i++)
        {
            int Index =
                (FailedIndex + i) % Workers.Count;


            PrinterWorker Candidate =
                Workers[Index];


            if (Candidate == null)
            {
                continue;
            }


            //--------------------------------------------------
            // Never retry the failed printer.
            //--------------------------------------------------

            if (Candidate == FailedWorker)
            {
                continue;
            }


            //--------------------------------------------------
            // Never retry a printer that already attempted
            // this particular order.
            //--------------------------------------------------

            if
            (
                Job.AttemptedPrinterIDs.Contains
                (
                    Candidate.Printer.PrinterID
                )
            )
            {
                continue;
            }


            //--------------------------------------------------
            // Printer must currently be considered working.
            //--------------------------------------------------

            if (!Candidate.WorkedLastTime)
            {
                Debug.Log
                (
                    $"Skipping {Candidate.Printer.Name} " +
                    $"because WorkedLastTime = false."
                );

                continue;
            }


            //--------------------------------------------------
            // Found a valid printer.
            //--------------------------------------------------

            return Candidate;
        }


        return null;
    }





    public void Reset_ListOfFailedAttemptsOf_LastFailedJob()
    {
        if (LastFailedJob == null)
        {
            Debug.LogWarning
            (
                "There is no last failed job to reset."
            );

            return;
        }


        Debug.Log
        (
            "Resetting failed-printer attempts of Order " +
            LastFailedJob.OrderID
        );


        LastFailedJob.AttemptedPrinterIDs.Clear();
    }






    public bool JumpStartPrinter(string PrinterName)
    {
        PrinterWorker Worker =
            Workers.Find
            (
                W =>
                    W != null &&
                    W.Printer != null &&
                    W.Printer.Name == PrinterName
            );


        if (Worker == null)
        {
            Debug.LogWarning
            (
                $"Could not find printer '{PrinterName}'."
            );

            return false;
        }


        Debug.Log
        (
            $"JumpStarting printer: {PrinterName}"
        );


        Worker.SetPrinterAsWorking();


        QueueManagerScript.RefreshQueuedOrders();


        //--------------------------------------------------
        // If this printer has waiting orders that were kept
        // here because there were no working printers,
        // allow it to start processing them now.
        //--------------------------------------------------

        Worker.TryStartNextJob();


        return true;
    }

    public bool DisablePrinter(string PrinterName)
    {
        PrinterWorker Worker =
            Workers.Find
            (
                W =>
                    W != null &&
                    W.Printer != null &&
                    W.Printer.Name == PrinterName
            );


        if (Worker == null)
        {
            Debug.LogWarning
            (
                $"Could not find printer '{PrinterName}'."
            );

            return false;
        }


        Debug.LogWarning
        (
            $"Manually disabling printer: {PrinterName}"
        );


        Worker.SetPrinterAsNotWorking();


        QueueManagerScript.RefreshQueuedOrders();


        return true;
    }




    public void ClearAllPrinterQueues()
    {
        foreach (PrinterWorker Worker in Workers)
        {
            Worker.Queue.RemoveAllJobs();
        }
    }

}