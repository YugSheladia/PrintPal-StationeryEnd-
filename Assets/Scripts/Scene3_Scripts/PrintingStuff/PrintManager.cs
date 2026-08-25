using System.Collections.Generic;
using UnityEngine;


public class PrintManager : MonoBehaviour
{
    //Script references
    [SerializeField] Scene3Manager  Scene3ManagerScript;
    [SerializeField] Scene3Backend  Scene3BackendScript;
    [SerializeField] HomePage       HomePageScript;
    [SerializeField] QueueManager   QueueManagerScript;

    public static PrintManager Instance;







    private PrintJob LastFailedJob;

    public List<PrinterWorker> Workers = new List<PrinterWorker>();

    int NextWorkerIndex = 0;







    //This HashSet contains all orders which are processing, so an order doesn't arrive in the same queue again,
    //or 2 workers dont have the same order at the same time.
    
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
    // GetApprovedPrintersFromServer).

    // Call this again from those scripts
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










    //Job Creation from a downloaded Order
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




    //QueuePrintJob -> PickNextWorker.
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
        // Select the best compatible printer.
        //--------------------------------------------------

        PrinterWorker Worker = PickBestWorker(Job);

        if (Worker == null)
        {
            Job.Status = PrintJobStatus.Failed;

            Job.StatusMessage = "No compatible printer available.";

            return false;
        }


        //--------------------------------------------------
        // Give the complete order to the selected printer.
        //--------------------------------------------------

        Worker.Enqueue(Job);

        QueueManagerScript.RefreshQueuedOrders();

        return true;
    }
    PrinterWorker PickBestWorker(PrintJob Job)
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

        bool NeedsColor = OrderNeedsColor(Job);
        bool NeedsDuplex = OrderNeedsDuplex(Job);

        for (int i = 0; i < Workers.Count; i++)
        {
            int Index = (NextWorkerIndex + i) % Workers.Count;

            PrinterWorker Candidate = Workers[Index];

            if (Candidate == null)
                continue;

            if (!Candidate.WorkedLastTime)
                continue;

            if (NeedsColor && !Candidate.Printer.SupportsColorPrinting)
                continue;

            if (NeedsDuplex && !Candidate.Printer.SupportsDoubleSidedPrinting)
                continue;

            Debug.Log
            (
                $"Compatible printer selected = {Candidate.Printer.Name}"
            );

            NextWorkerIndex = (Index + 1) % Workers.Count;

            return Candidate;
        }

        Debug.LogWarning
        (
            $"No printer supports this order. Color={NeedsColor}, Duplex={NeedsDuplex}"
        );

        Scene3ManagerScript.MessageText.text =
            "No compatible printer is available for this order.\n" +
            "Check printer capabilities in Settings.\n" +
            "This usually happens when no selected printer supports Double Sided Printing, Color Printing, or both";

        Scene3ManagerScript.EnableMessagePanel();

        return null;
    }
    bool OrderNeedsColor(PrintJob Job)
    {
        foreach (PrintDocument Document in Job.Documents)
        {
            if (Document.Print.Color == ColorEnum.Color)
            {
                return true;
            }
        }

        return false;
    }
    bool OrderNeedsDuplex(PrintJob Job)
    {
        foreach (PrintDocument Document in Job.Documents)
        {
            if (Document.Print.SideSelection == SideSelectionEnum.DoubleSided)
            {
                return true;
            }
        }

        return false;
    }











    //--------------------------------------------------
    // Round-robin dispatch:
    // Order#1 -> Printer#1,
    // Order#2 -> Printer#2,
    // ... wrapping back around once every printer's had a turn.


    // Rather than trusting Printer.IsOnline (which only reflects a status
    // string fetched once at login, and goes stale if a printer's actual
    // state changes mid-session), each candidate is actively checked here
    // via a live OpenPrinter call before being handed a job - if it
    // doesn't respond, move on to the next one.
    //--------------------------------------------------


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

        //Just to make sure that when no Printer is working and when the PrintingProcess is paused, all Queues are empty.
        ClearAllPrinterQueues();

        Scene3ManagerScript.MessageText.text =
            "No working printers available. " +
            "Make sure all connected printers are working, and that they are reactivated through Queue button on homepage.";

        //No Printer Available, so we are hitting pause on Automatic Printing. ( Removed the line below because PausePrinting() is already being called
        //in it's plae somewhre else ).
        //HomePageScript.PausePrinting();

        Scene3ManagerScript.EnableMessagePanel();

        return null;
    }


    //PrintWorker calls RedistributeJobsFromWorker() when it fails to print a document from an order.
    //In such a scenario, PrintWorker also marks itself as 'Not Working', which further sets the PrinterQueuePanel as 'Non Operational' upon Refresh.
    //The PrintWorker also Removes all orders from it's queues before calling RedistributeJobsFromWorker().
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

                //Removing this line of code for now, as I dont want a failed worker to have any order in it's queue.
                //FailedWorker.Queue.Enqueue(Job);

                HomePageScript.PausePrinting();

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





    //ReorouteFailedJob() -> PickNextAvailableWorker()

    //--------------------------------------------------
    // Also called by a PrinterWorker when it fails to print a job.
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













    //JumpStartPrinter() is called when a Printer is 'fixed', so that it can again accept orders.
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

    //DisablePrinter is called when a Printer is to be disabled, so that it doesn't accept orders anymore.
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










    //To clear the Queues of all Printers once it is finalized that none of them work.
    public void ClearAllPrinterQueues()
    {
        foreach (PrinterWorker Worker in Workers)
        {
            Worker.Queue.RemoveAllJobs();
        }
    }

}