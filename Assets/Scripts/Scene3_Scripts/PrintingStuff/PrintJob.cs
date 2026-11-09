//namespaces =
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class PrintJob
{
    //==================================================
    // Basic Information
    //==================================================

    //Order's ID =
    public int OrderID;

    //User's ID =
    public int UserID;

    //Username =
    public string Username;

    //Name of the Printer, jiske Queue mai PrintJob exists.
    public Printer Printer;

    //List of PrintDocuments belonging to the PrintJob / Order.
    public List<PrintDocument> Documents = new();







    //==================================================
    // Status
    //==================================================

    public PrintJobStatus Status = PrintJobStatus.Waiting;

    public string StatusMessage = "Waiting...";







    //==================================================
    // Multi-printer failover
    //==================================================

    // AttemptedPrinterIDs contains PrinterIDs that have already failed to
    // print any document of this job.

    // Used by PrintManager.RerouteFailedJob so a job never bounces back to a
    // printer that already failed it, and eventually gives up once
    // every known printer has been tried.

    public List<string> AttemptedPrinterIDs = new();









    //==================================================
    // Document Progress
    //==================================================

    public int TotalDocuments
    {
        get
        {
            return Documents.Count;
        }
    }

    public int DocumentsPrinted
    {
        get
        {
            int count = 0;

            foreach (PrintDocument document in Documents)
            {
                if (document.Status == PrintDocumentStatus.Completed)
                {
                    count++;
                }
            }

            return count;
        }
    }



    //==================================================
    // Page Progress
    //==================================================

    public int TotalPages
    {
        get
        {
            int pages = 0;

            foreach (PrintDocument document in Documents)
            {
                if (document.Print != null)
                {
                    pages += document.Print.Pages;
                }
            }

            return pages;
        }
    }

    public int PagesPrinted
    {
        get
        {
            int pages = 0;

            foreach (PrintDocument document in Documents)
            {
                pages += document.PagesPrinted;
            }

            return pages;
        }
    }




    //==================================================
    // Overall Progress
    //==================================================

    public float Progress
    {
        get
        {
            if (TotalPages == 0)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)PagesPrinted / TotalPages);
        }
    }










    //==================================================
    // Retry
    //==================================================

    public int RetryCount;










    //================================================================
    //Time stuff ( all in seconds )
    //( Calculated by PrintManager.Calculate_TimeForCompletion_OfEachJob() )
    //================================================================

    //This is the amount of time it will take for this Job to be completed.
    public int TimeForCompletion = 0;

    //This is a bool which is switched on when TimeForCompletion is successfully calculated.
    //This exists so that TimeForCompletion of each Job is calculated only once.
    public bool Calculated_TimeForCompletion = false;

    //This is TimeLeft ( currently ) for the completion of this Job.
    public int TimeLeft;

    //This bool is switched on when TimeLeft hits 0;
    public bool TimeLeft_Exhausted = false;

    //This variable takes into account any delay, i.e. the time difference between TimeForCompletion 
    //& the actual time this Job stays in a queue.
    public int AdditionalDelay = 0;

    //The ( real ) time at which TimeLeft was last updated.
    //PrintManager uses it to subtract the time which has REALLY passed, no matter how often it is called.
    public float LastTimeLeftUpdate;






    //This is the amount of pages which will be needed to print this document.
    public int PagesNeeded;



    //Clears this Job's time estimates ( and those of its unfinished documents ), so they are calculated again.
    //Called whenever the job is given to a printer ( a different printer may have a different PPM ).
    public void ResetTimeEstimates()
    {
        TimeForCompletion = 0;
        Calculated_TimeForCompletion = false;

        TimeLeft = 0;
        TimeLeft_Exhausted = false;

        AdditionalDelay = 0;

        PagesNeeded = 0;

        LastTimeLeftUpdate = Time.realtimeSinceStartup;

        foreach (PrintDocument Document in Documents)
        {
            //Documents which are already printed keep their values.
            if (Document.Status == PrintDocumentStatus.Completed)
            {
                continue;
            }

            Document.ResetTimeEstimates();
        }
    }







    public void PrintJobDetails()
    {
        //UnityEngine.Debug.Log("OrderID = " + OrderID);

        //UnityEngine.Debug.Log("Printer info = ");

        if (Printer != null)
        {
            //UnityEngine.Debug.Log("Printer name                 = " + Printer.Name);
            //UnityEngine.Debug.Log("Printer IsSelected           = " + Printer.IsSelected);
            //UnityEngine.Debug.Log("Printer IsOnline             = " + Printer.IsOnline);
            //UnityEngine.Debug.Log("Printer IsBusy               = " + Printer.IsBusy);
            //UnityEngine.Debug.Log("Printer Automatic printing   = " + Printer.AutomaticPrinting);
        }
        else
        {
            UnityEngine.Debug.Log("Printer is NULL.");
        }


        //UnityEngine.Debug.Log("PrintID of all prints = ");
        //foreach (PrintDocument document in Documents)
        //{
        //UnityEngine.Debug.Log("PrintID = " + document.Print.PrintID);
        //}

        //UnityEngine.Debug.Log("Status               = " + Status);
        //UnityEngine.Debug.Log("Status message       = " + StatusMessage);
        //UnityEngine.Debug.Log("Total documents      = " + TotalDocuments);
        //UnityEngine.Debug.Log("Documents printed    = " + DocumentsPrinted);
        //UnityEngine.Debug.Log("Total pages          = " + TotalPages);
        //UnityEngine.Debug.Log("Pages printed        = " + PagesPrinted);
        //UnityEngine.Debug.Log("Progress             = " + Progress);
        //UnityEngine.Debug.Log("Retry count          = " + RetryCount);
    }
}




public enum PrintJobStatus
{
    Waiting,

    Downloading,

    WaitingForPrinter,

    Printing,

    Completed,

    Failed,

    Cancelled,

    Paused
}