//namespaces =
using System.Collections.Generic;





public class PrintQueue
{
    //This is a Queue of all Jobs queued in this Queue.
    public Queue<PrintJob> Jobs = new Queue<PrintJob>();


    //This is a variable which keeps a track of total number of PrintJobs in this Queue.
    public int Count
    {
        get
        {
            return Jobs.Count;
        }
    }





    //To Enqueue a PrintJob.
    public void Enqueue(PrintJob Job)
    {
        Jobs.Enqueue(Job);
    }

    //To Dequeue a PrintJob.
    public PrintJob Dequeue()
    {
        if (Jobs.Count == 0)
        {
            return null;
        }

        return Jobs.Dequeue();
    }

    //To get the reference of the PrintJob in the front of the Queue.
    public PrintJob Peek()
    {
        if (Jobs.Count == 0)
        {
            return null;
        }

        return Jobs.Peek();
    }






    //To check if the Queue has any PrintJobs or not.
    public bool HasJobs()
    {
        return Jobs.Count > 0;
    }

    //to check if the Queue already contains a PrintJob or not.
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




    


    //To display all PrintJobs in this Queue.
    public void DisplayQueueInfo()
    {
        foreach (PrintJob Job in Jobs)
        {
            UnityEngine.Debug.Log("OrderID = " + Job.OrderID);
        }
    }









    //To clear this Queue of all PrintJobs.
    public void Clear()
    {
        Jobs.Clear();
    }


    //This only returns the Waiting PrintJobs, not the current PrintJob.
    public List<PrintJob> RemoveWaitingJobs()
    {
        List<PrintJob> WaitingJobs = new List<PrintJob>();


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

        PrintJob CurrentJob = Jobs.Dequeue();


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


    //This function is different than Clear() as it =
    //a. Dequeues PrintJobs one - by - one.
    //b. Returns reference of a list of each Dequeued PrintJob after Dequeuing them, to the function which calls RemoveAllJobs().

    public List<PrintJob> RemoveAllJobs()
    {
        List<PrintJob> JobsToRemove = new List<PrintJob>();


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