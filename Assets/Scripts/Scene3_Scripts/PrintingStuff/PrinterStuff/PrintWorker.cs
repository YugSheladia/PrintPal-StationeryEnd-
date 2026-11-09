//namespaces =
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static Scene3Backend;







// Owns one physical printer's queue and print loop.
// PrintManager creates a PrinterWorker object per printer and dispatches jobs into PrintWorker.
// PrintWorkers run fully independently of each other, which is what lets N printers work in
// parallel instead of a single global queue serializing everything.

public class PrinterWorker
{
    //Reference to the main Printer object.
    public Printer Printer;

    //This is the worker's Queue =
    public PrintQueue Queue = new PrintQueue();




    //Reference to important Scripts =
    readonly PrintManager Manager;
    readonly Scene3Backend Scene3BackendScript;



    //Bool which is used to figure if the Printer is currenlty printing or not.
    bool IsPrinting;



    //Bool to figure out whether the printer is working or not.
    public bool WorkedLastTime = true;



    //Constructor =
    public PrinterWorker(PrintManager Manager, Scene3Backend Scene3BackendScript, Printer Printer)
    {
        this.Manager = Manager;
        this.Scene3BackendScript = Scene3BackendScript;
        this.Printer = Printer;
    }






    //=====================================================================================
    //Enqueue() -> StartNextJob() -> Couroutine_PrintJob() -> Coroutine_PrintDocuemnt()
    //=====================================================================================

    //This function is used to Enqueue a job in this PrintWorker's Queue.
    public void Enqueue(PrintJob Job)
    {
        //Providing Job a reference to the Printer, which is going to print it.
        Job.Printer = Printer;

        //The time estimates depend on this printer's PPM, so they are calculated again for this printer.
        Job.ResetTimeEstimates();

        //Enqueing the Job.
        Queue.Enqueue(Job);

        Debug.Log($"Queued Order {Job.OrderID} on {Printer.Name}");

        //If the printer isn't printing anything currently, we StartNextJob().
        if (!IsPrinting)
        {
            StartNextJob();
        }
    }

    //To Start the next job.
    void StartNextJob()
    {
        //==================================================
        // NEVER start another job while this printer is
        // already processing one.
        //==================================================

        if (IsPrinting)
        {
            return;
        }


        //==================================================
        // NEVER start a job if this printer is marked
        // as non-working.
        //
        // This covers both:
        //
        // 1. Automatic failure detection.
        // 2. Manual DisablePrinter().
        //==================================================

        if (!WorkedLastTime)
        {
            Debug.Log
            (
                $"[{Printer.Name}] cannot start next job " +
                $"because WorkedLastTime = false."
            );

            Printer.IsBusy = false;

            return;
        }



        //==================================================
        // Nothing queued.
        //==================================================

        if (!Queue.HasJobs())
        {
            IsPrinting = false;
            Printer.IsBusy = false;

            return;
        }



        //==================================================
        // Lock this worker before starting the coroutine.
        //==================================================

        IsPrinting = true;
        Printer.IsBusy = true;

        //This returns the Job which is in the front of the Queue.
        PrintJob Job = Queue.Peek();


        Debug.Log
        (
            $"Starting Order {Job.OrderID} on {Printer.Name}"
        );


        Manager.StartCoroutine
        (
            Coroutine_PrintJob(Job)
        );
    }









    //StartNextJob() -> Coroutine_PrintJob().
    IEnumerator Coroutine_PrintJob(PrintJob Job)
    {
        if (!WorkedLastTime)
        {
            Debug.LogWarning
            (
                $"[{Printer.Name}] Cannot start Order {Job.OrderID}: printer is not working."
            );

            IsPrinting = false;
            Printer.IsBusy = false;

            yield break;
        }




        Job.Status = PrintJobStatus.Printing;
        Job.StatusMessage = "Printing...";

        //The job's countdown starts from now.
        Job.LastTimeLeftUpdate = Time.realtimeSinceStartup;

        //Making sure the estimate for this job exists right now ( instead of whenever RefreshUI() happens to run next ).
        Manager.Calculate_TimeForCompletion_OfEachJob();





        foreach (PrintDocument Document in Job.Documents)
        {

            if (Document.Status == PrintDocumentStatus.Completed)
                continue;





            bool Success = false;
            string FailureMessage = "";
            PrintFailureType FailureType = PrintFailureType.None;





            yield return Manager.StartCoroutine
            (
                Coroutine_PrintDocument
                (
                    Job,
                    Document,
                    (ResultSuccess, ResultMessage, ResultFailureType) =>
                    {
                        Success = ResultSuccess;
                        FailureMessage = ResultMessage;
                        FailureType = ResultFailureType;
                    }
                )
            );






            if (!Success)
            {
                Debug.LogError
                (
                    $"Order {Job.OrderID}, PrintID {Document.Print.PrintID} failed: " +
                    $"{FailureMessage} | Type: {FailureType}"
                );



                Document.Status = PrintDocumentStatus.Failed;
                Document.StatusMessage = FailureMessage;



                //--------------------------------------------------
                // Remove the current job from this worker's queue.
                //--------------------------------------------------

                PrintJob RemovedJob = Queue.Dequeue();

                if (RemovedJob != Job)
                {
                    Debug.LogError
                    (
                        $"Queue mismatch on {Printer.Name}. " +
                        $"Expected Order {Job.OrderID}."
                    );
                }



                //--------------------------------------------------
                // Preparation failure: do NOT mark printer faulty
                // and do NOT reroute automatically.
                //--------------------------------------------------

                if (FailureType == PrintFailureType.Preparation)
                {
                    Job.Status = PrintJobStatus.Paused;
                    Job.StatusMessage =
                        "Printing stopped because document preparation failed: " +
                        FailureMessage;



                    Debug.LogError
                    (
                        $"Order {Job.OrderID} stopped during preparation. " +
                        $"{Printer.Name} remains available."
                    );





                    IsPrinting = false;
                    Printer.IsBusy = false;



                    StartNextJob();
                    yield break;
                }




                //--------------------------------------------------
                // Actual printer failure: preserve existing behavior.
                //--------------------------------------------------

                SetPrinterAsNotWorking();




                Manager.RerouteFailedJob
                (
                    Job,
                    this,
                    FailureMessage
                );




                IsPrinting = false;
                Printer.IsBusy = false;




                StartNextJob();
                yield break;
            }





            //--------------------------------------------------
            // Document completed successfully.
            //--------------------------------------------------

            Document.Status = PrintDocumentStatus.Completed;
            Document.StatusMessage = "Completed";
            Document.TimeLeft = 0;

            Debug.Log
            (
                $"[{Printer.Name}] PrintID {Document.Print.PrintID} completed."
            );

            Debug.Log
            (
                $"[{Printer.Name}] Order {Job.OrderID} Progress = {Job.Progress}"
            );
        }

        //--------------------------------------------------
        // All documents have been printed ( condition A ).
        //
        // The order is only marked as printed once the estimated
        // time has ALSO run out ( condition B = Job.TimeLeft_Exhausted ).
        //--------------------------------------------------

        yield return Manager.StartCoroutine
        (
            Coroutine_WaitForTimeLeftToExhaust(Job)
        );



        //--------------------------------------------------
        // Entire PrintJob completed.
        //--------------------------------------------------

        Job.Status = PrintJobStatus.Completed;
        Job.StatusMessage = "Completed";
        Job.TimeLeft = 0;







        PrintJob CompletedJob = Queue.Dequeue();

        if (CompletedJob != Job)
        {
            Debug.LogError
            (
                $"Queue mismatch on {Printer.Name}. " +
                $"Expected Order {Job.OrderID}."
            );
        }

        Debug.Log
        (
            $"Finished Order {Job.OrderID} on {Printer.Name}"
        );






        yield return Manager.StartCoroutine
        (
            Scene3BackendScript.CompletePrintedOrder(Job.OrderID)
        );


        SetPrinterAsWorking();






        IsPrinting = false;
        Printer.IsBusy = false;



        StartNextJob();
    }










    //Coroutine_PrintJob() -> Coroutine_PrintDocument() ->
    //Coroutine_PrintDocument() +
    //Coroutine_GenerateHeader() +
    //Soroutine_SendPrintRequest() +
    //UpdatePaperUsage()

    public enum PrintFailureType
    {
        None,
        Preparation,
        Printer
    }




    IEnumerator Coroutine_PrintDocument
    (
        PrintJob Job,
        PrintDocument Document,
        Action<bool, string, PrintFailureType> OnComplete
    )
    {
        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] Printing PrintID = {Document.Print.PrintID}"
        );


        //--------------------------------------------------
        // STEP 1: Generate the document header.
        //--------------------------------------------------

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 1 STARTED: Header Generation"
        );

        string HeaderPDFPath = "";
        string HeaderErrorMessage = "";
        PrintFailureType HeaderFailureType = PrintFailureType.Preparation;

        bool HeaderGenerated = false;


        yield return Manager.StartCoroutine
        (
            Coroutine_GenerateHeader
            (
                Job,
                Document,
                (Success, Message, FailureType, PDFPath) =>
                {
                    HeaderGenerated = Success;
                    HeaderErrorMessage = Message;
                    HeaderFailureType = FailureType;
                    HeaderPDFPath = PDFPath;
                }
            )
        );


        if (!HeaderGenerated)
        {
            Debug.Log
            (
                $"[{DateTime.Now:HH:mm:ss.fff}] " +
                $"[{Printer.Name}] STEP 1 FAILED: {HeaderErrorMessage}"
            );

            OnComplete
            (
                false,
                HeaderErrorMessage,
                HeaderFailureType
            );

            yield break;
        }

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 1 COMPLETED: Header Generated"
        );



        //--------------------------------------------------
        // STEP 1.5: Merge the header into the document's PDF.
        //
        // The header and the document then go to the printer as ONE
        // print job instead of two ( every job costs the printer
        // several seconds ). If merging isn't possible, or fails,
        // MergedPDFPath stays empty and the old way ( 2 jobs ) is used.
        //--------------------------------------------------

        string MergedPDFPath = "";

        //Empty when the header can be merged, else the reason why it can't.
        string MergeBlocker = PdfMerger.GetMergeBlocker(Document.Print);

        if (MergeBlocker == "")
        {
            Debug.Log
            (
                $"[{DateTime.Now:HH:mm:ss.fff}] " +
                $"[{Printer.Name}] STEP 1.5 STARTED: Merging Header Into Document"
            );

            Document.StatusMessage = "Preparing document...";

            string MergeOutputPath =
                System.IO.Path.Combine
                (
                    System.IO.Path.GetDirectoryName(HeaderPDFPath),
                    $"Header_Order{Job.OrderID}_Print{Document.Print.PrintID}_Merged_{Guid.NewGuid():N}.pdf"
                );

            var MergeTask = PdfMerger.MergeAsync
            (
                HeaderPDFPath,
                Document.Print.LocalPDFPath,
                Document.Print.SideSelection == SideSelectionEnum.DoubleSided,
                Document.Print.Copies,
                Document.Print.CollatedOrNot == CollatedOrNotEnum.Collated,
                MergeOutputPath
            );

            yield return new WaitUntil(() => MergeTask.IsCompleted);

            if (MergeTask.IsFaulted || MergeTask.IsCanceled)
            {
                string MergeError =
                    MergeTask.Exception != null
                        ? MergeTask.Exception.GetBaseException().Message
                        : "Merging was cancelled.";

                Debug.LogWarning
                (
                    $"[{Printer.Name}] Could not merge the header into PrintID " +
                    $"{Document.Print.PrintID}: {MergeError}. Printing the header and the document separately."
                );
            }
            else
            {
                MergedPDFPath = MergeTask.Result;

                Debug.Log
                (
                    $"[{DateTime.Now:HH:mm:ss.fff}] " +
                    $"[{Printer.Name}] STEP 1.5 COMPLETED: Header Merged"
                );
            }
        }
        //Only said when merging is possible in general ( PDFsharp is enabled ) but not for this document.
        else if (PdfMerger.IsAvailable)
        {
            Debug.Log
            (
                $"[{DateTime.Now:HH:mm:ss.fff}] " +
                $"[{Printer.Name}] STEP 1.5 SKIPPED: {MergeBlocker}. " +
                "The header will be printed as a separate job."
            );
        }



        //--------------------------------------------------
        // STEP 2: Submit and monitor the print request.
        //--------------------------------------------------

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 2 STARTED: Sending Print Request"
        );

        bool PrintSuccess = false;
        string PrintMessage = "";

        PrintFailureType PrintFailure = PrintFailureType.Printer;


        yield return Manager.StartCoroutine
        (
            Coroutine_SendPrintRequest
            (
                Job,
                Document,
                HeaderPDFPath,
                MergedPDFPath,
                (Success, Message, FailureType) =>
                {
                    PrintSuccess = Success;
                    PrintMessage = Message;
                    PrintFailure = FailureType;
                }
            )
        );


        if (!PrintSuccess)
        {
            Debug.Log
            (
                $"[{DateTime.Now:HH:mm:ss.fff}] " +
                $"[{Printer.Name}] STEP 2 FAILED: {PrintMessage}"
            );

            OnComplete
            (
                false,
                PrintMessage,
                PrintFailure
            );

            yield break;
        }

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 2 COMPLETED: Print Request Finished"
        );



        //--------------------------------------------------
        // STEP 3: Update paper usage.
        //--------------------------------------------------

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 3 STARTED: Updating Paper Usage"
        );

        UpdatePaperUsage(Document);

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 3.1 COMPLETED: Paper Usage Updated"
        );


        //--------------------------------------------------
        // The estimated time is NOT waited for here anymore.
        //
        // It is waited for ONCE per order, by Coroutine_WaitForTimeLeftToExhaust(),
        // after the last document has been submitted ( so the documents of an order
        // go to the printer one after the other without idle waiting in between ).
        //--------------------------------------------------

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 3 COMPLETED: Paper Usage Updated"
        );



        //--------------------------------------------------
        // STEP 4: Report final success.
        //--------------------------------------------------

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] STEP 4: Reporting Final Success"
        );

        OnComplete
        (
            true,
            PrintMessage,
            PrintFailureType.None
        );

        Debug.Log
        (
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[{Printer.Name}] PRINTING PIPELINE COMPLETED: PrintID = {Document.Print.PrintID}"
        );
    }




    //Generates a header for each print.
    IEnumerator Coroutine_GenerateHeader
    (
        PrintJob Job,
        PrintDocument Document,
        Action<bool, string, PrintFailureType, string> OnComplete
    )
    {
        //--------------------------------------------------
        // STEP 1: Fetch customer details if not cached.
        //
        // The customer's name is required for the header.
        // Store it in Job.Username so other documents in
        // this order can reuse it without another API call.
        //--------------------------------------------------

        if (string.IsNullOrWhiteSpace(Job.Username))
        {
            Document.StatusMessage = "Fetching customer details...";

            using (UnityWebRequest Request =
                UnityWebRequest.Get
                (
                    "http://localhost:3000/user/" + Job.UserID
                ))
            {
                yield return Request.SendWebRequest();

                if (Request.result != UnityWebRequest.Result.Success)
                {
                    string ErrorMessage =
                        "Could not fetch customer details: " +
                        Request.error;

                    Debug.LogError(ErrorMessage);

                    OnComplete
                    (
                        false,
                        ErrorMessage,
                        PrintFailureType.Preparation,
                        ""
                    );

                    yield break;
                }

                //--------------------------------------------------
                // Deserialize the backend's JSON response.
                //--------------------------------------------------

                UsernameResponse Response = null;

                try
                {
                    Response = JsonUtility.FromJson<UsernameResponse>
                    (
                        Request.downloadHandler.text
                    );
                }
                catch (Exception Exception)
                {
                    Debug.LogException(Exception);
                }

                //--------------------------------------------------
                // Verify that the response contains a valid name.
                //--------------------------------------------------

                if
                (
                    Response == null ||
                    !Response.Success ||
                    Response.User == null ||
                    string.IsNullOrWhiteSpace(Response.User.Name)
                )
                {
                    string ErrorMessage =
                        "Customer name is missing from the user response.";

                    Debug.LogError(ErrorMessage);

                    OnComplete
                    (
                        false,
                        ErrorMessage,
                        PrintFailureType.Preparation,
                        ""
                    );

                    yield break;
                }

                // Cache the customer's name for this PrintJob.
                Job.Username = Response.User.Name;
            }
        }

        //--------------------------------------------------
        // STEP 2: Validate the cached customer name.
        //--------------------------------------------------

        if (string.IsNullOrWhiteSpace(Job.Username))
        {
            string ErrorMessage = "Job.Username is empty.";

            Debug.LogError(ErrorMessage);

            OnComplete
            (
                false,
                ErrorMessage,
                PrintFailureType.Preparation,
                ""
            );

            yield break;
        }

        //--------------------------------------------------
        // STEP 3: Generate the header PDF.
        //
        // Pass the customer's details, order ID, print ID,
        // and original filename to the header generator.
        // Each document gets its own unique header.
        //--------------------------------------------------

        Document.StatusMessage = "Generating document header...";

        var HeaderTask = PrintHeaderGenerator.GenerateAsync
        (
            Job.UserID,
            Job.Username,
            Job.OrderID,
            Document.Print.PrintID,
            Document.Print.PDFName
        );

        // Wait until the asynchronous generation task finishes.
        yield return new WaitUntil(() => HeaderTask.IsCompleted);

        //--------------------------------------------------
        // STEP 4: Handle generation errors or cancellation.
        //--------------------------------------------------

        if (HeaderTask.IsFaulted || HeaderTask.IsCanceled)
        {
            string ErrorMessage =
                HeaderTask.Exception != null
                    ? HeaderTask.Exception.GetBaseException().Message
                    : "Header generation was cancelled.";

            Debug.LogError
            (
                $"[{Printer.Name}] {ErrorMessage}"
            );

            OnComplete
            (
                false,
                ErrorMessage,
                PrintFailureType.Preparation,
                ""
            );

            yield break;
        }

        //--------------------------------------------------
        // STEP 5: Retrieve and validate the generated PDF path.
        //--------------------------------------------------

        string HeaderPDFPath = HeaderTask.Result;

        if (string.IsNullOrWhiteSpace(HeaderPDFPath))
        {
            string ErrorMessage =
                "Header generation returned an empty PDF path.";

            Debug.LogError(ErrorMessage);

            OnComplete
            (
                false,
                ErrorMessage,
                PrintFailureType.Preparation,
                ""
            );

            yield break;
        }

        Debug.Log
        (
            $"[{Printer.Name}] Header generated for PrintID " +
            $"{Document.Print.PrintID}."
        );

        //--------------------------------------------------
        // STEP 6: Return the generated header path.
        //--------------------------------------------------

        OnComplete
        (
            true,
            "Header generated successfully.",
            PrintFailureType.None,
            HeaderPDFPath
        );
    }


    //Sends the actual PrintRequest.
    IEnumerator Coroutine_SendPrintRequest
    (
        PrintJob Job,
        PrintDocument Document,
        string HeaderPDFPath,
        string MergedPDFPath,
        Action<bool, string, PrintFailureType> OnComplete
    )
    {

        //--------------------------------------------------
        // If the header is already merged into the document's PDF,
        // it is printed together with the document ( in STEP 2 ),
        // so it must NOT be submitted again as a separate job.
        //--------------------------------------------------

        bool HeaderIsMerged = !string.IsNullOrEmpty(MergedPDFPath);

        if (!HeaderIsMerged)
        {
            //--------------------------------------------------
            // STEP 1: Submit the header before the original PDF.
            //--------------------------------------------------

            Document.StatusMessage = "Submitting document header...";

            var HeaderPrintTask = WindowsPrinter.PrintHeaderAsync
            (
                HeaderPDFPath,
                Printer
            );

            yield return new WaitUntil
            (
                () => HeaderPrintTask.IsCompleted
            );

            if
            (
                HeaderPrintTask.IsFaulted ||
                HeaderPrintTask.IsCanceled
            )
            {
                string ErrorMessage =
                    HeaderPrintTask.Exception != null
                        ? HeaderPrintTask.Exception.GetBaseException().Message
                        : "Header printing was cancelled.";

                Debug.LogError
                (
                    $"[{Printer.Name}] {ErrorMessage}"
                );

                OnComplete
                (
                    false,
                    ErrorMessage,
                    PrintFailureType.Printer
                );

                yield break;
            }

            PrintResult HeaderResult = HeaderPrintTask.Result;

            if (!HeaderResult.Success)
            {
                Debug.LogError
                (
                    $"[{Printer.Name}] Header failed: " +
                    HeaderResult.Message
                );

                OnComplete
                (
                    false,
                    HeaderResult.Message,
                    PrintFailureType.Printer
                );

                yield break;
            }

            Debug.Log
            (
                $"[{Printer.Name}] Header submitted for PrintID " +
                $"{Document.Print.PrintID}."
            );
        }



        //--------------------------------------------------
        // STEP 2: Submit exactly one request for the original PDF.
        //--------------------------------------------------

        Document.StatusMessage =
            HeaderIsMerged
                ? "Submitting header and document..."
                : "Submitting original document...";

        var PrintTask = WindowsPrinter.PrintAsync
        (
            Document,
            Printer,
            MergedPDFPath
        );

        yield return new WaitUntil
        (
            () => PrintTask.IsCompleted
        );

        if (PrintTask.IsFaulted || PrintTask.IsCanceled)
        {
            string ErrorMessage =
                PrintTask.Exception != null
                    ? PrintTask.Exception.GetBaseException().Message
                    : "Print submission was cancelled.";

            Debug.LogError
            (
                $"[{Printer.Name}] {ErrorMessage}"
            );

            OnComplete
            (
                false,
                ErrorMessage,
                PrintFailureType.Printer
            );

            yield break;
        }

        PrintResult Result = PrintTask.Result;

        if (!Result.Success)
        {
            Debug.LogError(Result.Message);

            if (Result.Exception != null)
            {
                Debug.LogException(Result.Exception);
            }

            OnComplete
            (
                false,
                Result.Message,
                PrintFailureType.Printer
            );

            yield break;
        }

        Debug.Log
        (
            $"[{Printer.Name}] Submitted PrintID " +
            $"{Document.Print.PrintID} to the Windows print queue. " +
            "Watching for completion..."
        );

        Document.Status = PrintDocumentStatus.Printing;
        Document.StatusMessage = "Printing...";

        //The document's countdown starts from now.
        Document.LastTimeLeftUpdate = Time.realtimeSinceStartup;

        //Making sure TimeForCompletion / TimeLeft of this document exist right now ( and not only when the UI happens to refresh ).
        Manager.Calculate_TimeForCompletion_OfEachJob();

        //--------------------------------------------------
        // STEP 3: Monitor the original PDF.
        //--------------------------------------------------

        bool Success = false;
        string Message = "";
        bool MonitorFinished = false;

        yield return Manager.StartCoroutine
        (
            PrintMonitor.MonitorPrintJob
            (
                Printer,
                Document,

                OnProgress: (PagesPrinted, TotalPages) =>
                {
                    Document.PagesPrinted = PagesPrinted;
                },

                OnPaused: (Reason, PauseMessage) =>
                {
                    Job.Status = PrintJobStatus.Paused;

                    Job.StatusMessage =
                        $"[{Printer.Name}] {PauseMessage}";

                    Document.StatusMessage = PauseMessage;

                    Debug.LogWarning
                    (
                        $"Order {Job.OrderID} paused on " +
                        $"{Printer.Name}: {PauseMessage}"
                    );
                },

                OnResumed: () =>
                {
                    Job.Status = PrintJobStatus.Printing;
                    Job.StatusMessage = "Printing...";

                    //The time spent paused must not be counted, so the countdowns continue from now.
                    Job.LastTimeLeftUpdate = Time.realtimeSinceStartup;
                    Document.LastTimeLeftUpdate = Time.realtimeSinceStartup;
                    Document.StatusMessage = "Printing...";

                    Debug.Log
                    (
                        $"Order {Job.OrderID} resumed on " +
                        $"{Printer.Name}."
                    );
                },

                OnComplete: (ResultSuccess, ResultMessage) =>
                {
                    Success = ResultSuccess;
                    Message = ResultMessage;
                    MonitorFinished = true;
                },

                //The spooler job is named after the file which was printed.
                PDFPathOverride: MergedPDFPath
            )
        );

        //--------------------------------------------------
        // STEP 4: Ensure the monitor reported completion.
        //--------------------------------------------------

        if (!MonitorFinished)
        {
            string ErrorMessage =
                "Print monitor ended without reporting completion.";

            Debug.LogError
            (
                $"[{Printer.Name}] {ErrorMessage} " +
                $"PrintID {Document.Print.PrintID}."
            );

            OnComplete
            (
                false,
                ErrorMessage,
                PrintFailureType.Printer
            );

            yield break;
        }

        //--------------------------------------------------
        // STEP 5: Return the final print result.
        //--------------------------------------------------

        OnComplete
        (
            Success,
            Message,
            Success
                ? PrintFailureType.None
                : PrintFailureType.Printer
        );
    }


    //Called by Coroutine_PrintJob() once ALL documents of the job have been printed ( A ).
    //It waits until Job.TimeLeft_Exhausted is true ( B ), so an order is never marked as printed earlier than its estimated time.
    //If A is true and B is already true ( the printing took longer than estimated ), it returns immediately.
    IEnumerator Coroutine_WaitForTimeLeftToExhaust(PrintJob Job)
    {
        //The job's countdown only runs while its status is Printing.
        Job.Status = PrintJobStatus.Printing;
        Job.StatusMessage = "Finishing print...";

        while (true)
        {
            //Updating the countdown here as well, so this doesn't depend on RefreshUI() running ( it is only called while the
            //home page is refreshing / automatic printing is on ). Calling it more often than needed is harmless, as the
            //countdown subtracts the time that has really passed.
            Manager.Calculate_TimeForCompletion_OfEachJob();

            if (Job.TimeLeft_Exhausted)
            {
                yield break;
            }

            //The job was removed from this queue while waiting ( e.g. the queues were cleared ), so there is nothing to wait for.
            if (!Queue.ContainsOrder(Job.OrderID))
            {
                Debug.LogWarning
                (
                    $"[{Printer.Name}] Order {Job.OrderID} is no longer in the queue. Not waiting for its TimeLeft."
                );

                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }




    //Tells the server how much paper a printed document used, and then updates this printer's PagesLeft ( locally ).
    //( This used to be done by Coroutine_WaitForPrintingDelay(), which was removed - PagesLeft was never updated after that. )
    void UpdatePaperUsage(PrintDocument Document)
    {
        //EffectiveSheets accounts for pages per sheet.
        //Copies accounts for the number of copies.
        //Add one sheet for the document header.
        int PagesUsed =
            (Document.Print.EffectiveSheets *
             Document.Print.Copies) + 1;

        Scene3BackendScript.PagesUsedUpdationProcess
        (
            Printer.StationeryPrinterID,
            PagesUsed,
            this
        );
    }










    //===================================================================================================================
    //Called by Scene3Backend's script -> PagesUsedUpdationProccess_Coroutine() to locally update the PagesLeft of a
    //stationery once the PagesUsedUpdationProcess() has been completed successfully.
    //===================================================================================================================

    //===========================================================================================================================
    //flow =
    //Coroutine_PrintDocument() -> Scene3BackendScript.PagesLeftUpdationProcess() -> PagesLeftUpdationProcess_Coroutine() ->
    //UpdatePagesUsed() + GetPagesLeft() + UpdatePagesLeft_Locally()
    //===========================================================================================================================
    public void UpdatePagesLeft_Locally(int PagesLeft)
    {
        Printer.PagesLeft = PagesLeft;

        Printer.Update_PagesLeftIndicator();

        Manager.QueueManagerScript.RefreshQueuedOrders();
    }









    //This function is called when -
    //a. A printer succesfully prints all documents of an order. ( i.e. by Coroutine_PrintJob() )
    //OR
    //b. When a stationery sets it as Operational through PrinterQueuePanel ( i.e. by PrintManager script -> JumpStartPrinter() ).
    public void SetPrinterAsWorking()
    {
        WorkedLastTime = true;

        Printer.IsOnline = true;


        Debug.Log
        (
            $"[{Printer.Name}] marked as WORKING."
        );


        //--------------------------------------------------
        // If this printer has jobs waiting in its queue,
        // allow it to resume processing them.
        //--------------------------------------------------

        if (!IsPrinting && Queue.HasJobs())
        {
            StartNextJob();
        }
    }

    //This function is called when -
    //a. A printer is not able to prints any documents of an order. ( i.e. by Coroutine_PrintJob() )
    //OR
    //b. When a stationery sets a printer as Non Operational through PrinterQueuePanel ( i.e. by PrintManager script -> DisablePrinter() ).
    public void SetPrinterAsNotWorking()
    {
        //--------------------------------------------------
        // Already marked as non-working.
        //--------------------------------------------------

        if (!WorkedLastTime)
        {
            Debug.Log
            (
                $"[{Printer.Name}] is already marked " +
                $"as NOT WORKING."
            );

            return;
        }


        //--------------------------------------------------
        // Mark printer unavailable immediately.
        //--------------------------------------------------

        WorkedLastTime = false;

        Printer.IsOnline = false;


        Debug.Log
        (
            $"[{Printer.Name}] marked as NOT WORKING."
        );


        //--------------------------------------------------
        // IMPORTANT:
        //
        // We DO NOT interrupt the currently printing order
        // here.
        //
        // If this method was triggered by PrintMonitor,
        // the current job has already failed and has already
        // been removed from the queue.
        //
        // If this method was triggered manually while the
        // printer is currently printing, the current physical
        // job is allowed to finish, but this printer will NOT
        // receive another order afterward.
        //
        // Waiting orders are redistributed immediately.
        //--------------------------------------------------

        Manager.RedistributeJobsFromWorker(this);
    }




    //This is called by PrintManager script -> JumpStartPrinter() incase the Printer had any Waiting orders.
    public void TryStartNextJob()
    {
        StartNextJob();
    }
}