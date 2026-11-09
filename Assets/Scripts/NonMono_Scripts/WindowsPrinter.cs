using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



public static class WindowsPrinter
{

    private static readonly string SumatraPath =
    Path.Combine
    (
        Application.streamingAssetsPath,
        "Printing",
        "SumatraPDF.exe"
    );

    // How long we'll wait for SumatraPDF to hand the job off before we
    // give up on that attempt. This is what replaces the
    // indefinite Process.WaitForExit() that was freezing the app - a
    // dead/unreachable printer now fails the job after this long
    // instead of blocking forever.
    private const int ProcessTimeoutMilliseconds = 30000;

    // How many times Sumatra is started for one submission before the submission is reported as failed.
    // ( A retry only happens when the first attempt timed out AND the job is not in the Windows spooler,
    // so nothing can be printed twice because of it. )
    private const int MaxSubmitAttempts = 2;

    // After a timed out Sumatra process is killed, this long is waited before the spooler is checked,
    // so a job which was just being handed over can show up.
    private const int SpoolerSettleMilliseconds = 1500;



    // Entry point used by PrinterWorker. Runs the actual print call on a
    // background thread via Task.Run, so the blocking Process.Start() /
    // WaitForExit() never touches Unity's main thread. The caller
    // (a coroutine) can yield on this Task without freezing anything.

    //PDFPathOverride is optional: when it is given, that PDF is printed instead of Document.Print.LocalPDFPath
    //( used for the document's PDF with the header merged into it ).
    public static Task<PrintResult> PrintAsync
    (
        PrintDocument Document,
        Printer Printer,
        string PDFPathOverride = null
    )
    {
        return Task.Run
        (
            () => Print(Document, Printer, PDFPathOverride)
        );
    }



    private static PrintResult Print
    (
        PrintDocument Document,
        Printer Printer,
        string PDFPathOverride
    )
    {
        PrintResult Result = new PrintResult();

        //The PDF which is really going to be printed.
        string PDFPath =
            string.IsNullOrEmpty(PDFPathOverride)
                ? Document.Print.LocalPDFPath
                : PDFPathOverride;

        try
        {
            if (!File.Exists(SumatraPath))
            {
                Result.Success = false;
                Result.Message =
                    "SumatraPDF.exe not found.";

                return Result;
            }

            if
            (
                !File.Exists
                (
                    PDFPath
                )
            )
            {
                Result.Success = false;
                Result.Message =
                    "PDF file not found.";

                return Result;
            }


            string Arguments =
                SumatraArguments.Build
                (
                    Document,
                    Printer,
                    PDFPath,

                    //A PDFPathOverride is always the merged PDF, which already contains the copies.
                    !string.IsNullOrEmpty(PDFPathOverride)
                );


            return SubmitWithSumatra
            (
                Arguments,
                Path.GetFileName(PDFPath),
                Printer,
                "Submitted successfully.",
                "Print"
            );
        }
        catch (Exception Exception)
        {
            Result.Success = false;
            Result.Message = Exception.Message;

            Result.Exception = Exception;

            return Result;
        }
    }






    //New
    public static Task<PrintResult> PrintHeaderAsync
    (
        string HeaderPDFPath,
        Printer Printer
    )
    {
        return Task.Run(() =>
        {
            PrintResult Result = new PrintResult();

            try
            {
                if (!File.Exists(SumatraPath))
                {
                    Result.Success = false;
                    Result.Message = "SumatraPDF.exe not found.";
                    return Result;
                }

                if (!File.Exists(HeaderPDFPath))
                {
                    Result.Success = false;
                    Result.Message = "Header PDF file not found.";
                    return Result;
                }

                string Arguments =
                    "-silent " +
                    $"-print-to \"{Printer.Name}\" " +
                    $"-print-settings \"monochrome\" " +
                    $"\"{HeaderPDFPath}\"";

                return SubmitWithSumatra
                (
                    Arguments,
                    Path.GetFileName(HeaderPDFPath),
                    Printer,
                    "Header submitted successfully.",
                    "Header printing"
                );
            }
            catch (Exception Exception)
            {
                Result.Success = false;
                Result.Message = Exception.Message;
                Result.Exception = Exception;

                return Result;
            }
        });
    }






    //Starts SumatraPDF with the given arguments, and reports if the job was handed over to Windows.
    //Used for both the document ( or merged PDF ) and the separate header. Must run on a background thread.
    //
    //1. Sumatra exits with code 0                      -> submitted.
    //2. Sumatra exits with another code                -> submitted only if the job is in the Windows spooler.
    //3. Sumatra doesn't exit within the timeout        -> it is killed, and then
    //     a. the job IS in the Windows spooler         -> submitted ( Sumatra only hung after handing it over ).
    //     b. the job is NOT in the spooler             -> Sumatra is started once more ( nothing was printed, so
    //                                                      this can't print twice ). If that also fails, it is a failure.
    private static PrintResult SubmitWithSumatra
    (
        string Arguments,
        string DocumentName,
        Printer Printer,
        string SuccessMessage,
        string Label
    )
    {
        PrintResult Result = new PrintResult();

        for (int Attempt = 1; Attempt <= MaxSubmitAttempts; Attempt++)
        {
            using (Process Process = new Process())
            {
                Process.StartInfo.FileName =
                    SumatraPath;

                Process.StartInfo.Arguments =
                    Arguments;

                Process.StartInfo.UseShellExecute =
                    false;

                Process.StartInfo.CreateNoWindow =
                    true;

                Process.StartInfo.WindowStyle =
                    ProcessWindowStyle.Hidden;


                Process.Start();


                //--------------------------------------------------
                // Wait for Sumatra to finish handing the document
                // to Windows - but only up to the timeout.
                //
                // If the target printer is unreachable, Sumatra can
                // hang indefinitely waiting on the OS print call.
                //--------------------------------------------------

                bool Exited =
                    Process.WaitForExit
                    (
                        ProcessTimeoutMilliseconds
                    );


                if (!Exited)
                {
                    try
                    {
                        Process.Kill();
                    }
                    catch (Exception KillException)
                    {
                        UnityEngine.Debug.LogWarning
                        (
                            "Failed to kill stuck SumatraPDF process: "
                            + KillException.Message
                        );
                    }


                    //Maybe Sumatra only hung AFTER Windows had already accepted the job.
                    Thread.Sleep(SpoolerSettleMilliseconds);

                    Win32Structures.JOB_INFO_1? AcceptedJob =
                        Win32PrintSpooler.FindJob
                        (
                            Printer.Name,
                            DocumentName
                        );


                    if (AcceptedJob.HasValue)
                    {
                        UnityEngine.Debug.LogWarning
                        (
                            $"[{Printer.Name}] SumatraPDF did not exit within " +
                            $"{ProcessTimeoutMilliseconds / 1000}s, but Windows already accepted " +
                            $"'{DocumentName}' as JobId {AcceptedJob.Value.JobId}. Treating it as submitted."
                        );

                        Result.Success = true;

                        Result.Message =
                            "Submitted to Windows spooler " +
                            "( SumatraPDF did not exit in time ).";

                        return Result;
                    }


                    if (Attempt < MaxSubmitAttempts)
                    {
                        UnityEngine.Debug.LogWarning
                        (
                            $"[{Printer.Name}] SumatraPDF did not respond within " +
                            $"{ProcessTimeoutMilliseconds / 1000}s ( attempt {Attempt} of {MaxSubmitAttempts} ) " +
                            $"and '{DocumentName}' is not in the Windows spooler. Trying again."
                        );

                        continue;
                    }


                    Result.Success = false;

                    Result.Message =
                        $"SumatraPDF did not respond within " +
                        $"{ProcessTimeoutMilliseconds / 1000}s " +
                        $"({MaxSubmitAttempts} attempts). " +
                        "The printer may be unreachable.";

                    return Result;
                }


                int ExitCode =
                    Process.ExitCode;


                //--------------------------------------------------
                // A zero exit code is a clean submission.
                //--------------------------------------------------

                if (ExitCode == 0)
                {
                    Result.Success = true;
                    Result.Message =
                        SuccessMessage;

                    return Result;
                }


                //--------------------------------------------------
                // Some printer drivers cause Sumatra to return
                // non-zero even though Windows accepted the job.
                //
                // Therefore check the Windows spooler before
                // declaring failure. (This call is safe here - we're
                // already on a background thread.)
                //--------------------------------------------------

                Win32Structures.JOB_INFO_1? Job =
                    Win32PrintSpooler.FindJob
                    (
                        Printer.Name,
                        DocumentName
                    );


                if (Job.HasValue)
                {
                    UnityEngine.Debug.LogWarning
                    (
                        $"Sumatra exited with code {ExitCode}, " +
                        $"but Windows accepted the job as JobId " +
                        $"{Job.Value.JobId}."
                    );

                    Result.Success = true;

                    Result.Message =
                        $"Submitted to Windows spooler " +
                        $"(Sumatra exit code {ExitCode}).";

                    return Result;
                }


                //--------------------------------------------------
                // Neither Sumatra nor Windows gave us evidence
                // that the job was accepted.
                //--------------------------------------------------

                Result.Success = false;

                Result.Message =
                    $"{Label} failed: Sumatra exit code {ExitCode} " +
                    "and the print job was not found in the Windows spooler.";

                return Result;
            }
        }


        //Not reachable ( every attempt returns or continues ), but the compiler needs a return.
        Result.Success = false;
        Result.Message = "SumatraPDF could not be started.";

        return Result;
    }
}