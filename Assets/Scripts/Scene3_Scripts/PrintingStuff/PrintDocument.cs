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






    //================================================================
    //Time stuff ( all in seconds )
    //================================================================

    //This is the amount of time it will take for this Document to be printed.
    public int TimeForCompletion;

    //This is a bool which is switched on when TimeForCompletion is successfully calculated.
    //This exists so that TimeForCompletion of each Document is calculated only once.
    public bool Calculated_TimeForCompletion = false;

    //This is TimeLeft ( currently ) for the completion of this Document.
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






    public PrintDocument(PrintDetails printDetails)
    {
        Print = printDetails;
    }


    //Clears all time estimates, so they are calculated again ( e.g. when the job moves to a printer with a different PPM ).
    public void ResetTimeEstimates()
    {
        TimeForCompletion = 0;
        Calculated_TimeForCompletion = false;

        TimeLeft = 0;
        TimeLeft_Exhausted = false;

        AdditionalDelay = 0;

        PagesNeeded = 0;

        LastTimeLeftUpdate = Time.realtimeSinceStartup;
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