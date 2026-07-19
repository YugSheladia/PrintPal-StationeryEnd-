using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;





public class PrintQueue
{


    private Queue<PrintJob> Jobs = new Queue<PrintJob>();

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
}