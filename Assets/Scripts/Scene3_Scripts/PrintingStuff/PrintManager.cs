using System.Collections.Generic;
using UnityEngine;


public class PrintManager : MonoBehaviour
{
    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;



    public static PrintManager Instance;

    // One worker per physical printer. Each worker owns its own queue and
    // runs independently, so all printers can be busy at once.
    public List<PrinterWorker> Workers = new List<PrinterWorker>();

    int NextWorkerIndex = 0;




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
        Workers.Clear();
        NextWorkerIndex = 0;

        foreach (Printer P in Script1.SelectedPrinters)
        {
            if (!P.IsSelected)
            {
                continue;
            }

            // The server only ever sends back a Name (see
            // GetApprovedPrintersFromServer) - PrinterID is left blank.
            // Fall back to Name as the unique key: Windows already
            // requires printer names to be unique on a system, and it's
            // what WindowsPrinter/SumatraArguments target anyway.
            if (string.IsNullOrEmpty(P.PrinterID))
            {
                P.PrinterID = P.Name;
            }

            // IsOnline now comes from the real Windows status merged in
            // during GetApprovedPrintersFromServer (see SelectPrinters /
            // Scene2Backend.GetPrinters) - no need to override it here
            // anymore.

            Workers.Add(new PrinterWorker(this, Scene3BackendScript, P));
        }

        if (Workers.Count == 0)
        {
            Debug.LogWarning("No approved/selected printers found for this stationery.");
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

                return null;
            }

            PrintDocument Document = new PrintDocument(Print);

            Job.Documents.Add(Document);
        }

        // Note: Job.Printer isn't set here anymore - it gets assigned once
        // QueuePrintJob picks which worker/printer will handle it.

        return Job;
    }
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

        return true;
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
    PrinterWorker PickNextWorker()
    {
        if (Workers.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < Workers.Count; i++)
        {
            int Index = (NextWorkerIndex + i) % Workers.Count;

            PrinterWorker Candidate = Workers[Index];

            //Check everytime if the printer is online or not, but for now, it's fine...



            /////////////////////////////////////////////////////////////////////////////////////
            /*
            if (Candidate.Printer.IsOnline)
            {
                Debug.Log($"{Candidate.Printer.Name} is online.");

                NextWorkerIndex = (Index + 1) % Workers.Count;

                return Candidate;
            }

            Debug.LogWarning($"{Candidate.Printer.Name} isn't online, giving the order to the next printer.");
            */

            Debug.Log("Worker selected = " + Candidate);

            NextWorkerIndex = (Index + 1) % Workers.Count;

            return Candidate;
        }

        Scene3ManagerScript.MessageText.text = "No connected printer found. Make sure you have Selected the correct printers & that The printers are turned on.";
        Scene3ManagerScript.EnableMessagePanel();

        return null;
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
    public void RerouteFailedJob(PrintJob Job, PrinterWorker FailedWorker, string Reason)
    {
        Job.AttemptedPrinterIDs.Add(FailedWorker.Printer.PrinterID);
        Job.RetryCount++;

        //Kill the reqesut for the FailedWorker.

        PrinterWorker NextWorker = PickNextAvailableWorker(Job, FailedWorker);

        if (NextWorker == null)
        {
            Debug.Log
            (
                $"Order {Job.OrderID} failed on every available printer. Last error: {Reason}"
            );

            Job.Status = PrintJobStatus.Failed;
            Job.StatusMessage = $"Failed on all printers: {Reason}";

            HomePageScript.PausePrinting();

            return;
        }

        Debug.LogWarning
        (
            $"Order {Job.OrderID} failed on {FailedWorker.Printer.Name}, rerouting to {NextWorker.Printer.Name}."
        );

        // Reset anything that wasn't already completed so it gets retried
        // cleanly on the new printer - documents that already finished on
        // the failed printer stay marked Completed and won't be reprinted
        // (see PrinterWorker.Coroutine_PrintJob).
        foreach (PrintDocument Document in Job.Documents)
        {
            if (Document.Status != PrintDocumentStatus.Completed)
            {
                Document.Status = PrintDocumentStatus.Waiting;
                Document.PagesPrinted = 0;
                Document.StatusMessage = "Waiting to retry on another printer...";
            }
        }

        Job.Status = PrintJobStatus.Waiting;
        Job.StatusMessage = $"Rerouted to {NextWorker.Printer.Name}...";

        NextWorker.Enqueue(Job);
    }

    // Starts searching from the printer *after* the one that just failed
    // (e.g. Printer#2 fails -> try Printer#3 next), skipping any printer
    // already attempted for this specific job, and wrapping around.
    // Same live-check approach as PickNextWorker - see comment there.
    PrinterWorker PickNextAvailableWorker(PrintJob Job, PrinterWorker FailedWorker)
    {
        int FailedIndex = Workers.IndexOf(FailedWorker);

        for (int i = 1; i <= Workers.Count; i++)
        {
            int Index = (FailedIndex + i) % Workers.Count;

            PrinterWorker Candidate = Workers[Index];

            if (Job.AttemptedPrinterIDs.Contains(Candidate.Printer.PrinterID))
            {
                continue;
            }

            bool Responding = Win32PrintSpooler.IsPrinterResponding(Candidate.Printer.Name);

            Candidate.Printer.IsOnline = Responding;

            if (!Responding)
            {
                continue;
            }

            return Candidate;
        }

        return null;
    }
}