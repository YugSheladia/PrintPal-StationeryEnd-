using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class PrintJob
{
    //==================================================
    // Basic Information
    //==================================================

    public int OrderID;

    public Printer Printer;

    public List<PrintDocument> Documents = new();

    //==================================================
    // Status
    //==================================================

    public PrintJobStatus Status = PrintJobStatus.Waiting;

    public string StatusMessage = "Waiting...";

    //==================================================
    // Document Progress
    //==================================================

    public int TotalDocuments;

    public int DocumentsPrinted;

    //==================================================
    // Page Progress
    //==================================================

    public int TotalPages;

    public int PagesPrinted;

    //==================================================
    // Overall Progress
    //==================================================

    public float Progress;

    //==================================================
    // Retry
    //==================================================

    public int RetryCount;







    public void PrintJobDetails()
    {
        UnityEngine.Debug.Log("OrderID = " + OrderID);

        UnityEngine.Debug.Log("Printer info = ");
        UnityEngine.Debug.Log("Printer name                 = " + Printer.Name);
        UnityEngine.Debug.Log("Printer ID                   = " + Printer.PrinterID);
        UnityEngine.Debug.Log("Printer IsSelected           = " + Printer.IsSelected);
        UnityEngine.Debug.Log("Printer IsOnline             = " + Printer.IsOnline);
        UnityEngine.Debug.Log("Printer IsBusy               = " + Printer.IsBusy);
        UnityEngine.Debug.Log("Printer Automatic printing   = " + Printer.AutomaticPrinting);

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


