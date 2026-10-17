using System;
using System.Diagnostics;
using System.IO;
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
    // give up and treat it as a failure. This is what replaces the
    // indefinite Process.WaitForExit() that was freezing the app - a
    // dead/unreachable printer now fails the job after this long
    // instead of blocking forever.
    private const int ProcessTimeoutMilliseconds = 30000;



    // Entry point used by PrinterWorker. Runs the actual print call on a
    // background thread via Task.Run, so the blocking Process.Start() /
    // WaitForExit() never touches Unity's main thread. The caller
    // (a coroutine) can yield on this Task without freezing anything.
    public static Task<PrintResult> PrintAsync
    (
        PrintDocument Document,
        Printer Printer
    )
    {
        return Task.Run
        (
            () => Print(Document, Printer)
        );
    }



    // Unchanged logic from before, EXCEPT:
    // 1. This now only ever runs on a background thread (via PrintAsync).
    // 2. Process.WaitForExit() now takes a timeout and kills the process
    //    instead of waiting indefinitely.
    private static PrintResult Print
    (
        PrintDocument Document,
        Printer Printer
    )
    {
        PrintResult Result = new PrintResult();

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
                    Document.Print.LocalPDFPath
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
                    Printer
                );


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
                // Without a timeout here, that used to block Unity's
                // main thread forever. Now it just fails this job.
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

                    Result.Success = false;

                    Result.Message =
                        $"SumatraPDF did not respond within " +
                        $"{ProcessTimeoutMilliseconds / 1000}s. " +
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
                        "Submitted successfully.";

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

                string DocumentName =
                    Path.GetFileName
                    (
                        Document.Print.LocalPDFPath
                    );

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
                    $"Sumatra failed with exit code {ExitCode} " +
                    "and the print job was not found in the Windows spooler.";

                return Result;
            }
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

                using (Process Process = new Process())
                {
                    Process.StartInfo.FileName = SumatraPath;
                    Process.StartInfo.Arguments = Arguments;
                    Process.StartInfo.UseShellExecute = false;
                    Process.StartInfo.CreateNoWindow = true;
                    Process.StartInfo.WindowStyle =
                        ProcessWindowStyle.Hidden;

                    Process.Start();

                    bool Exited = Process.WaitForExit(
                        ProcessTimeoutMilliseconds);

                    if (!Exited)
                    {
                        try { Process.Kill(); }
                        catch { }

                        Result.Success = false;
                        Result.Message =
                            "Header printing timed out.";

                        return Result;
                    }

                    if (Process.ExitCode == 0)
                    {
                        Result.Success = true;
                        Result.Message =
                            "Header submitted successfully.";

                        return Result;
                    }

                    string DocumentName =
                        Path.GetFileName(HeaderPDFPath);

                    Win32Structures.JOB_INFO_1? Job =
                        Win32PrintSpooler.FindJob(
                            Printer.Name,
                            DocumentName);

                    if (Job.HasValue)
                    {
                        Result.Success = true;
                        Result.Message =
                            "Header accepted by Windows spooler.";

                        return Result;
                    }

                    Result.Success = false;
                    Result.Message =
                        $"Header printing failed. Sumatra exit code: " +
                        $"{Process.ExitCode}";

                    return Result;
                }
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
}