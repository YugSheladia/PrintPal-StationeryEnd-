//namespaces =
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







    public void PrintJobDetails()
    {
        UnityEngine.Debug.Log("OrderID = " + OrderID);

        UnityEngine.Debug.Log("Printer info = ");

        if (Printer != null)
        {
            UnityEngine.Debug.Log("Printer name                 = " + Printer.Name);
            UnityEngine.Debug.Log("Printer IsSelected           = " + Printer.IsSelected);
            UnityEngine.Debug.Log("Printer IsOnline             = " + Printer.IsOnline);
            UnityEngine.Debug.Log("Printer IsBusy               = " + Printer.IsBusy);
            UnityEngine.Debug.Log("Printer Automatic printing   = " + Printer.AutomaticPrinting);
        }
        else
        {
            UnityEngine.Debug.Log("Printer is NULL.");
        }


        UnityEngine.Debug.Log("PrintID of all prints = ");
        foreach (PrintDocument document in Documents)
        {
            UnityEngine.Debug.Log("PrintID = " + document.Print.PrintID);
        }

        UnityEngine.Debug.Log("Status               = " + Status);
        UnityEngine.Debug.Log("Status message       = " + StatusMessage);
        UnityEngine.Debug.Log("Total documents      = " + TotalDocuments);
        UnityEngine.Debug.Log("Documents printed    = " + DocumentsPrinted);
        UnityEngine.Debug.Log("Total pages          = " + TotalPages);
        UnityEngine.Debug.Log("Pages printed        = " + PagesPrinted);
        UnityEngine.Debug.Log("Progress             = " + Progress);
        UnityEngine.Debug.Log("Retry count          = " + RetryCount);
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

