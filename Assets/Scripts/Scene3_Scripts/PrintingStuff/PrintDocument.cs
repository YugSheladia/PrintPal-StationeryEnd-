using System;
using UnityEngine;


[System.Serializable]

public class PrintDocument
{
    // Actual print data
    public PrintDetails Print;


    // Runtime status
    public PrintDocumentStatus Status = PrintDocumentStatus.Waiting;


    // Printing progress
    public int PagesPrinted;


    public float Progress
    {
        get
        {
            if (Print == null || Print.Pages <= 0)
                return 0f;

            return Mathf.Clamp01((float)PagesPrinted / Print.Pages);
        }
    }


    public string StatusMessage = "Waiting...";



    public PrintDocument(PrintDetails printDetails)
    {
        Print = printDetails;
    }
}



public enum PrintDocumentStatus
{
    Waiting,

    Downloading,

    Downloaded,

    Printing,

    Completed,

    Failed,

    Cancelled
}