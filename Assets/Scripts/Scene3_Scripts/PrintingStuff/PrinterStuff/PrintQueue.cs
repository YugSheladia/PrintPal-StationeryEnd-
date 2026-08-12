using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;





public class PrintQueue
{


    public Queue<PrintJob> Jobs = new Queue<PrintJob>();

    public int Count
    {
        get
        {
            return Jobs.Count;
        }
    }






    public void Enqueue(PrintJob Job)
    {
        Jobs.Enqueue(Job);
    }

    public PrintJob Dequeue()
    {
        if (Jobs.Count == 0)
        {
            return null;
        }

        return Jobs.Dequeue();
    }

    public PrintJob Peek()
    {
        if (Jobs.Count == 0)
        {
            return null;
        }

        return Jobs.Peek();
    }

    public bool HasJobs()
    {
        return Jobs.Count > 0;
    }

    public bool ContainsOrder(int OrderID)
    {
        foreach (PrintJob Job in Jobs)
        {
            if (Job.OrderID == OrderID)
            {
                return true;
            }
        }

        return false;
    }

    public void Clear()
    {
        Jobs.Clear();
    }

    public void DisplayQueueInfo()
    {
        foreach (PrintJob Job in Jobs)
        {
            UnityEngine.Debug.Log("OrderID = " + Job.OrderID);
        }
    }





    public List<PrintJob> RemoveWaitingJobs()
    {
        List<PrintJob> WaitingJobs =
            new List<PrintJob>();


        //--------------------------------------------------
        // Zero or one job means there is nothing waiting
        // behind the current job.
        //--------------------------------------------------

        if (Jobs.Count <= 1)
        {
            return WaitingJobs;
        }


        //--------------------------------------------------
        // Preserve the current job.
        //
        // PrinterWorker is processing Jobs.Peek().
        //--------------------------------------------------

        PrintJob CurrentJob =
            Jobs.Dequeue();


        //--------------------------------------------------
        // Everything remaining is waiting.
        //--------------------------------------------------

        while (Jobs.Count > 0)
        {
            WaitingJobs.Add
            (
                Jobs.Dequeue()
            );
        }


        //--------------------------------------------------
        // Restore the current job.
        //--------------------------------------------------

        Jobs.Enqueue(CurrentJob);


        return WaitingJobs;
    }
    public List<PrintJob> RemoveAllJobs()
    {
        List<PrintJob> JobsToRemove =
            new List<PrintJob>();


        //--------------------------------------------------
        // Remove every waiting job currently in this queue.
        //
        // IMPORTANT:
        // The currently printing job MUST already have been
        // removed by PrinterWorker before this function is
        // called.
        //--------------------------------------------------

        while (Jobs.Count > 0)
        {
            JobsToRemove.Add
            (
                Jobs.Dequeue()
            );
        }


        return JobsToRemove;
    }
}