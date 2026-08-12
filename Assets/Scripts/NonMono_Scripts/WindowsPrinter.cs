using System;
using System.Diagnostics;
using System.IO;
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



    public static PrintResult Print
    (
        PrintDocument Document,
        Printer Printer
    )
    {
        PrintResult Result = new PrintResult();

        try
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log
            (
                "Sumatra Path : " + SumatraPath
            );

            UnityEngine.Debug.Log
            (
                "Exists       : " + File.Exists(SumatraPath)
            );

            UnityEngine.Debug.Log
            (
                "PDF Path     : " + Document.Print.LocalPDFPath
            );

            UnityEngine.Debug.Log
            (
                "PDF Exists   : " +
                File.Exists(Document.Print.LocalPDFPath)
            );
#endif

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


#if UNITY_EDITOR
            UnityEngine.Debug.Log
            (
                "Arguments:"
            );

            UnityEngine.Debug.Log
            (
                Arguments
            );
#endif


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
                // to Windows.
                //--------------------------------------------------

                Process.WaitForExit();


                int ExitCode =
                    Process.ExitCode;


#if UNITY_EDITOR
                UnityEngine.Debug.Log
                (
                    "Sumatra Exit Code: "
                    + ExitCode
                );
#endif


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
                // declaring failure.
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
}