using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class PrintHeaderGenerator
{
    //Only ONE header is generated at a time. All headers now share one Edge profile folder, and 2 Edge
    //processes using the same profile would hand over to each other ( instead of each printing its own PDF ).
    static readonly SemaphoreSlim GenerationLock = new SemaphoreSlim(1, 1);

    //How many times Edge is tried before the header is reported as failed.
    const int AttemptCount = 2;

    //How long ( in milliseconds ) one attempt may take before Edge is killed and the attempt counts as failed.
    const int AttemptTimeoutMs = 40000;

    //How often ( in milliseconds ) the header PDF is checked for.
    const int PollIntervalMs = 250;

    //Old files are cleaned up only once each time the app runs.
    static int CleanUpStarted = 0;

    public static Task<string> GenerateAsync
    (
        int UserID,
        string UserName,
        int OrderID,
        int PrintID,
        string PDFName
    )
    {
        // Access Unity API on the main thread.
        string PersistentDataPath = Application.persistentDataPath;

        return Task.Run(() =>
        {
            GenerationLock.Wait();

            try
            {
                string FolderPath = Path.Combine(
                    PersistentDataPath,
                    "PrintHeaders"
                );

                Directory.CreateDirectory(FolderPath);

                StartCleanUpOnce(FolderPath);

                string BaseName =
                    $"Header_Order{OrderID}_Print{PrintID}_{Guid.NewGuid():N}";

                string HTMLPath = Path.Combine(
                    FolderPath, BaseName + ".html"
                );

                string PDFPath = Path.Combine(
                    FolderPath, BaseName + ".pdf"
                );

                //One profile folder which is reused by every header ( it used to be a brand new folder each time,
                //which made Edge set up a new profile - sync etc. - on every single header ).
                string ProfilePath = Path.Combine(
                    FolderPath, "EdgeProfile"
                );

                string SafeUserName =
                    System.Net.WebUtility.HtmlEncode(UserName ?? "Unknown");

                string SafePDFName =
                    System.Net.WebUtility.HtmlEncode(PDFName ?? "Unknown document");

                string HTML = @"<!DOCTYPE html>
                                <html>
                                <head>
                                <meta charset=""utf-8"">
                                <style>
                                    @page { size: A4; margin: 0; }
                                    body {
                                        font-family: Arial, sans-serif;
                                        font-size: 12px;
                                        margin: 0;
                                        padding: 0;
                                    }
                                </style>
                                </head>
                                <body>
                                    h1
                                </body>
                                </html>";

                File.WriteAllText(HTMLPath, HTML, new UTF8Encoding(false));

                string EdgePath = FindEdgePath();

                if (string.IsNullOrEmpty(EdgePath))
                {
                    throw new FileNotFoundException(
                        "Microsoft Edge was not found."
                    );
                }

                try
                {
                    Exception LastError = null;

                    for (int Attempt = 1; Attempt <= AttemptCount; Attempt++)
                    {
                        try
                        {
                            RunEdge(EdgePath, ProfilePath, HTMLPath, PDFPath);

                            return PDFPath;
                        }
                        catch (Exception Error)
                        {
                            LastError = Error;

                            //A half written PDF from a failed attempt must never be used.
                            TryDeleteFile(PDFPath);
                        }
                    }

                    throw new Exception(
                        "Header generation failed after " + AttemptCount +
                        " attempts. Last error: " + LastError.Message,
                        LastError
                    );
                }
                finally
                {
                    //The HTML is only needed while Edge is making the PDF.
                    TryDeleteFile(HTMLPath);
                }
            }
            finally
            {
                GenerationLock.Release();
            }
        });
    }




    //Runs Edge once. Throws if the header PDF could not be made.
    //
    //Edge does not always exit by itself after writing the PDF ( it was seen writing the file and then staying open
    //until the old 60 second timeout ). So this does not wait for Edge to exit - as soon as the PDF exists and has
    //stopped growing, Edge is killed and the PDF is used.
    private static void RunEdge
    (
        string EdgePath,
        string ProfilePath,
        string HTMLPath,
        string PDFPath
    )
    {
        TryDeleteFile(PDFPath);

        Directory.CreateDirectory(ProfilePath);

        string Arguments =
            "--headless=new " +
            "--disable-gpu " +
            "--disable-extensions " +
            "--disable-sync " +
            "--disable-background-networking " +
            "--no-first-run " +
            "--no-default-browser-check " +
            "--no-pdf-header-footer " +
            $"--user-data-dir=\"{ProfilePath}\" " +
            $"--print-to-pdf=\"{PDFPath}\" " +
            $"\"{new Uri(HTMLPath).AbsoluteUri}\"";

        StringBuilder ErrorOutput = new StringBuilder();

        using (Process EdgeProcess = new Process())
        {
            EdgeProcess.StartInfo.FileName = EdgePath;
            EdgeProcess.StartInfo.Arguments = Arguments;
            EdgeProcess.StartInfo.UseShellExecute = false;
            EdgeProcess.StartInfo.CreateNoWindow = true;
            EdgeProcess.StartInfo.WindowStyle =
                ProcessWindowStyle.Hidden;

            // Capture diagnostic output.
            EdgeProcess.StartInfo.RedirectStandardOutput = true;
            EdgeProcess.StartInfo.RedirectStandardError = true;

            EdgeProcess.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (ErrorOutput)
                    {
                        //Edge prints a lot of unrelated errors, so only the start is kept.
                        if (ErrorOutput.Length < 3000)
                        {
                            ErrorOutput.AppendLine(e.Data);
                        }
                    }
                }
            };

            EdgeProcess.Start();

            EdgeProcess.BeginOutputReadLine();
            EdgeProcess.BeginErrorReadLine();

            Stopwatch Watch = Stopwatch.StartNew();

            long LastLength = -1;
            bool PDFReady = false;
            bool EdgeExitedByItself = false;

            while (Watch.ElapsedMilliseconds < AttemptTimeoutMs)
            {
                bool Exited = EdgeProcess.HasExited;

                long Length = GetFinishedFileLength(PDFPath);

                //The same size twice in a row ( and the file can be opened, i.e. Edge isn't writing it ) = finished.
                if (Length > 0 && (Length == LastLength || Exited))
                {
                    PDFReady = true;

                    break;
                }

                LastLength = Length;

                if (Exited)
                {
                    EdgeExitedByItself = true;

                    break;
                }

                Thread.Sleep(PollIntervalMs);
            }

            int ExitCode = -1;

            if (EdgeExitedByItself)
            {
                try
                {
                    ExitCode = EdgeProcess.ExitCode;
                }
                catch { }
            }

            //Edge ( and everything it started ) is not needed anymore.
            if (!EdgeProcess.HasExited)
            {
                KillProcessTree(EdgeProcess);
            }

            if (PDFReady)
            {
                return;
            }

            string Output;

            lock (ErrorOutput)
            {
                Output = ErrorOutput.ToString();
            }

            if (EdgeExitedByItself)
            {
                throw new Exception(
                    "Edge exited with code " + ExitCode +
                    " without creating a valid header PDF. Output: " + Output
                );
            }

            throw new TimeoutException(
                "Edge did not create the header PDF within " +
                (AttemptTimeoutMs / 1000) + " seconds. " +
                "Executable: " + EdgePath +
                ". Output: " + Output
            );
        }
    }




    //Returns the size of a file only if it exists and nobody is writing to it anymore ( else -1 ).
    private static long GetFinishedFileLength(string FilePath)
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return -1;
            }

            using (FileStream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Stream.Length;
            }
        }
        catch (IOException)
        {
            return -1;
        }
        catch (UnauthorizedAccessException)
        {
            return -1;
        }
    }




    //Kills a process together with all the processes it started ( Edge starts many ).
    private static void KillProcessTree(Process Target)
    {
        try
        {
            using (Process Killer = new Process())
            {
                Killer.StartInfo.FileName = "taskkill";
                Killer.StartInfo.Arguments = "/PID " + Target.Id + " /T /F";
                Killer.StartInfo.UseShellExecute = false;
                Killer.StartInfo.CreateNoWindow = true;

                Killer.Start();
                Killer.WaitForExit(5000);
            }
        }
        catch { }

        //Fallback, in case taskkill couldn't run.
        try
        {
            if (!Target.HasExited)
            {
                Target.Kill();
                Target.WaitForExit(5000);
            }
        }
        catch { }
    }




    private static void TryDeleteFile(string FilePath)
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch { }
    }




    //Once per run of the app, in the background:
    //a. Deletes header files ( html / pdf ) which are older than a day - they used to pile up forever.
    //b. Deletes the old "EdgeProfile_<guid>" folders which the previous version left behind.
    private static void StartCleanUpOnce(string FolderPath)
    {
        if (Interlocked.Exchange(ref CleanUpStarted, 1) != 0)
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                foreach (string OldFile in Directory.GetFiles(FolderPath, "Header_*"))
                {
                    try
                    {
                        if (File.GetLastWriteTime(OldFile) < DateTime.Now.AddDays(-1))
                        {
                            File.Delete(OldFile);
                        }
                    }
                    catch { }
                }

                foreach (string OldProfile in Directory.GetDirectories(FolderPath, "EdgeProfile_*"))
                {
                    try
                    {
                        Directory.Delete(OldProfile, true);
                    }
                    catch { }
                }
            }
            catch { }
        });
    }




    private static string FindEdgePath()
    {
        string[] Paths =
        {
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft", "Edge", "Application", "msedge.exe"
            ),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "Microsoft", "Edge", "Application", "msedge.exe"
            ),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "Edge", "Application", "msedge.exe"
            )
        };

        foreach (string PathToCheck in Paths)
        {
            if (File.Exists(PathToCheck))
                return PathToCheck;
        }

        return null;
    }




    //Actual Header =
    /*
        string HTML = $@"<!DOCTYPE html>
                            <html>
                            <head>
                            <meta charset=""utf-8"">
                            <style>
                                @page {{ size: A4; margin: 18mm; }}
                                body {{ font-family: Arial, sans-serif; color: #111; font-size: 16px; }}
                                .header {{ border: 2px solid #222; padding: 24px; }}
                                h1 {{ font-size: 26px; margin-top: 0; border-bottom: 1px solid #999; padding-bottom: 12px; }}
                                .field {{ margin: 16px 0; }}
                                .label {{ font-weight: bold; }}
                                .footer {{ margin-top: 28px; font-size: 12px; color: #555; }}
                            </style>
                            </head>
                            <body>
                            <div class=""header"">
                                <h1>PrintPal — Document Header</h1>
                                <div class=""field""><span class=""label"">Customer:</span> {SafeUserName}</div>
                                <div class=""field""><span class=""label"">User ID:</span> {UserID}</div>
                                <div class=""field""><span class=""label"">Order ID:</span> {OrderID}</div>
                                <div class=""field""><span class=""label"">Print ID:</span> {PrintID}</div>
                                <div class=""field""><span class=""label"">Document:</span> {SafePDFName}</div>
                                <div class=""footer"">Generated by PrintPal</div>
                            </div>
                            </body>
                            </html>";
    */
}