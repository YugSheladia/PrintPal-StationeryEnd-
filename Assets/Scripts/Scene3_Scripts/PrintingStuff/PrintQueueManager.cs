using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintQueueManager : MonoBehaviour
{
    public static PrintQueueManager Instance;


    private Queue<PrintJob> PrintQueue = new Queue<PrintJob>();

    private PrintJob CurrentJob;

    private bool IsProcessing = false;

    private bool IsPaused = false;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }



    // Adds a new order printing job
    public void AddPrintJob(PrintJob job)
    {
        job.Status = PrintJobStatus.Waiting;
        job.Progress = 0;

        PrintQueue.Enqueue(job);

        Debug.Log($"Print Job Added: Order {job.OrderID}");

        StartQueue();
    }



    // Starts queue processing
    public void StartQueue()
    {
        if (IsProcessing)
            return;

        if (IsPaused)
            return;

        if (PrintQueue.Count == 0)
            return;


        StartCoroutine(ProcessQueue());
    }



    IEnumerator ProcessQueue()
    {
        IsProcessing = true;


        while (PrintQueue.Count > 0 && !IsPaused)
        {
            CurrentJob = PrintQueue.Dequeue();


            yield return StartCoroutine(ProcessJob(CurrentJob));


            CurrentJob = null;
        }


        IsProcessing = false;
    }



    // Handles one complete PrintJob
    IEnumerator ProcessJob(PrintJob job)
    {
        Debug.Log($"Processing Order: {job.OrderID}");


        job.Status = PrintJobStatus.Downloading;


        // Download PDFs here
        // handled by DownloadManager later


        job.Status = PrintJobStatus.Printing;


        // Send documents to printer here
        // handled by WindowsPrinter later


        job.Progress = 1f;


        job.Status = PrintJobStatus.Completed;


        Debug.Log($"Order Completed: {job.OrderID}");


        yield return null;
    }



    // Cancel a job before it starts
    public bool CancelJob(int OrderID)
    {
        Queue<PrintJob> UpdatedQueue = new Queue<PrintJob>();


        bool Removed = false;


        while (PrintQueue.Count > 0)
        {
            PrintJob job = PrintQueue.Dequeue();


            if (job.OrderID == OrderID)
            {
                job.Status = PrintJobStatus.Cancelled;

                Removed = true;

                continue;
            }


            UpdatedQueue.Enqueue(job);
        }


        PrintQueue = UpdatedQueue;


        return Removed;
    }



    // Retry failed print
    public void RetryJob(PrintJob job)
    {
        job.RetryCount++;

        job.Status = PrintJobStatus.Waiting;

        AddPrintJob(job);
    }



    // Pause queue
    public void PauseQueue()
    {
        IsPaused = true;
    }



    // Resume queue
    public void ResumeQueue()
    {
        IsPaused = false;

        StartQueue();
    }



    // Clears waiting jobs
    public void ClearQueue()
    {
        while (PrintQueue.Count > 0)
        {
            PrintJob job = PrintQueue.Dequeue();

            job.Status = PrintJobStatus.Cancelled;
        }
    }



    // Get current printing job
    public PrintJob GetCurrentJob()
    {
        return CurrentJob;
    }



    // Get all waiting jobs
    public List<PrintJob> GetWaitingJobs()
    {
        return new List<PrintJob>(PrintQueue);
    }



    // Update progress externally
    public void UpdateJobProgress(PrintJob job, float progress)
    {
        job.Progress = Mathf.Clamp01(progress);
    }



    // Check if order is currently printing
    public bool IsOrderPrinting(int OrderID)
    {
        if (CurrentJob != null && CurrentJob.OrderID == OrderID)
            return true;


        foreach (PrintJob job in PrintQueue)
        {
            if (job.OrderID == OrderID)
                return true;
        }


        return false;
    }
}