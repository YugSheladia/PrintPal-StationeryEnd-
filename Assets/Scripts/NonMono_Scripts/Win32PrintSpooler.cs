using System;
using System.Collections.Generic;
using System.IO;
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

    [DllImport("winspool.drv", SetLastError = true)]
    static extern bool SetJob
    (
        IntPtr PrinterHandle,
        uint JobId,
        uint Level,
        IntPtr pJob,
        uint Command
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
            if (string.IsNullOrEmpty(Job.Document))
            {
                continue;
            }

            // Windows/the driver sometimes registers the job's Document
            // field as a bare filename, sometimes as the full source
            // path (e.g. "C:\...\1.pdf") - compare on filename only so
            // this matches either way.
            string JobFileName = Path.GetFileName(Job.Document);

            if
            (
                JobFileName.Equals
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

    // Look up a specific job once you already know its JobId.
    // Prefer this over FindJob(name) once a job has been pinned,
    // since filenames aren't guaranteed unique across the queue.
    public static Win32Structures.JOB_INFO_1? GetJob
    (
        string PrinterName,
        int JobID
    )
    {
        foreach
        (
            Win32Structures.JOB_INFO_1 Job
            in GetJobs(PrinterName)
        )
        {
            if (Job.JobId == JobID)
            {
                return Job;
            }
        }

        return null;
    }

    // Cancels a specific job in the Windows spooler. Used when we give
    // up watching a stuck job (see PrintMonitor's StuckTimeoutSeconds
    // timeout) so it doesn't just sit there and get printed later if the
    // printer reconnects.
    public static bool CancelJob
    (
        string PrinterName,
        int JobID
    )
    {
        Win32Structures.PRINTER_DEFAULTS Defaults =
            new Win32Structures.PRINTER_DEFAULTS();

        // Cancelling a job needs admin-level access to the printer
        // object, not just PRINTER_ACCESS_USE (which is enough to submit
        // jobs and enumerate the queue, but not to control them).
        Defaults.DesiredAccess =
            Win32Structures.PRINTER_ACCESS_ADMINISTER
            | Win32Structures.PRINTER_ACCESS_USE;

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
            return false;
        }

        try
        {
            return SetJob
            (
                PrinterHandle,
                (uint)JobID,
                0,
                IntPtr.Zero,
                (uint)Win32Structures.JOB_CONTROL_CANCEL
            );
        }
        finally
        {
            ClosePrinter(PrinterHandle);
        }
    }

    public static bool IsPrinterResponding
    (
        string PrinterName
    )
    {
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
            return false;
        }

        ClosePrinter(PrinterHandle);

        return true;
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