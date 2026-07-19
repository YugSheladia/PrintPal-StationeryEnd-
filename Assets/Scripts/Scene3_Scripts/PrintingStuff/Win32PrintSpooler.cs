using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Win32PrintSpooler
{
    //==================================================
    // Win32 Imports
    //==================================================

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool OpenPrinter
    (
        string PrinterName,
        out IntPtr PrinterHandle,
        ref Win32Structures.PRINTER_DEFAULTS Defaults
    );

    [DllImport("winspool.drv", SetLastError = true)]
    static extern bool ClosePrinter
    (
        IntPtr PrinterHandle
    );

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool EnumJobs
    (
        IntPtr PrinterHandle,
        uint FirstJob,
        uint NumberOfJobs,
        uint Level,
        IntPtr Buffer,
        uint BufferSize,
        out uint BytesNeeded,
        out uint JobsReturned
    );

    //==================================================
    // Public API
    //==================================================

    public static List<Win32Structures.JOB_INFO_1> GetJobs
    (
        string PrinterName
    )
    {
        List<Win32Structures.JOB_INFO_1> Jobs =
            new List<Win32Structures.JOB_INFO_1>();

        Win32Structures.PRINTER_DEFAULTS Defaults =
            new Win32Structures.PRINTER_DEFAULTS();

        Defaults.DesiredAccess =
            Win32Structures.PRINTER_ACCESS_USE;

        if
        (
            !OpenPrinter
            (
                PrinterName,
                out IntPtr PrinterHandle,
                ref Defaults
            )
        )
        {
            return Jobs;
        }

        try
        {
            uint BytesNeeded;
            uint JobsReturned;

            EnumJobs
            (
                PrinterHandle,
                0,
                999,
                1,
                IntPtr.Zero,
                0,
                out BytesNeeded,
                out JobsReturned
            );

            if (BytesNeeded == 0)
            {
                return Jobs;
            }

            IntPtr Buffer =
                Marshal.AllocHGlobal((int)BytesNeeded);

            try
            {
                bool Success =
                    EnumJobs
                    (
                        PrinterHandle,
                        0,
                        999,
                        1,
                        Buffer,
                        BytesNeeded,
                        out BytesNeeded,
                        out JobsReturned
                    );

                if (!Success)
                {
                    return Jobs;
                }

                int Size =
                    Marshal.SizeOf
                    (
                        typeof(Win32Structures.JOB_INFO_1)
                    );

                IntPtr Current = Buffer;

                for
                (
                    int i = 0;
                    i < JobsReturned;
                    i++
                )
                {
                    Win32Structures.JOB_INFO_1 Job =
                        Marshal.PtrToStructure
                        <
                            Win32Structures.JOB_INFO_1
                        >
                        (
                            Current
                        );

                    Jobs.Add(Job);

                    Current =
                        IntPtr.Add
                        (
                            Current,
                            Size
                        );
                }
            }
            finally
            {
                Marshal.FreeHGlobal(Buffer);
            }
        }
        finally
        {
            ClosePrinter(PrinterHandle);
        }

        return Jobs;
    }

    public static Win32Structures.JOB_INFO_1? FindJob
    (
        string PrinterName,
        string DocumentName
    )
    {
        foreach
        (
            Win32Structures.JOB_INFO_1 Job
            in GetJobs(PrinterName)
        )
        {
            if
            (
                Job.Document.Equals
                (
                    DocumentName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return Job;
            }
        }

        return null;
    }

    public static bool IsDocumentInQueue
    (
        string PrinterName,
        string DocumentName
    )
    {
        return
            FindJob
            (
                PrinterName,
                DocumentName
            ).HasValue;
    }
}