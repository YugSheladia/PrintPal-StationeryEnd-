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


    [DllImport
    (
        "winspool.drv",
        SetLastError = true,
        CharSet = CharSet.Auto
    )]


    static extern bool GetPrinter
    (
        IntPtr hPrinter,
        uint Level,
        IntPtr pPrinter,
        uint cbBuf,
        out uint pcbNeeded
    );

    public struct PrinterStatusInfo
    {
        public bool Success;
        public int Status;
        public string Message;
    }


    public static PrinterStatusInfo GetPrinterStatus
    (
        string PrinterName
    )
    {
        PrinterStatusInfo Result =
            new PrinterStatusInfo();

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
            Result.Success = false;
            Result.Status = 0;

            Result.Message =
                "OpenPrinter failed. Error = "
                + Marshal.GetLastWin32Error();

            return Result;
        }

        try
        {
            uint BytesNeeded = 0;

            GetPrinter
            (
                PrinterHandle,
                2,
                IntPtr.Zero,
                0,
                out BytesNeeded
            );

            if (BytesNeeded == 0)
            {
                Result.Success = false;
                Result.Status = 0;

                Result.Message =
                    "GetPrinter returned no data. Error = "
                    + Marshal.GetLastWin32Error();

                return Result;
            }

            IntPtr Buffer =
                Marshal.AllocHGlobal((int)BytesNeeded);

            try
            {
                bool Success =
                    GetPrinter
                    (
                        PrinterHandle,
                        2,
                        Buffer,
                        BytesNeeded,
                        out BytesNeeded
                    );

                if (!Success)
                {
                    Result.Success = false;
                    Result.Status = 0;

                    Result.Message =
                        "GetPrinter failed. Error = "
                        + Marshal.GetLastWin32Error();

                    return Result;
                }

                Win32Structures.PRINTER_INFO_2 PrinterInfo =
                    Marshal.PtrToStructure
                    <
                        Win32Structures.PRINTER_INFO_2
                    >
                    (
                        Buffer
                    );

                Result.Success = true;
                Result.Status = PrinterInfo.Status;

                Result.Message =
                    GetPrinterStatusMessage
                    (
                        PrinterInfo.Status
                    );

                return Result;
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
    }


    static string GetPrinterStatusMessage
    (
        int Status
    )
    {
        if (Status == 0)
        {
            return "No printer status flags reported.";
        }

        List<string> Messages =
            new List<string>();

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_PAUSED) != 0
        )
        {
            Messages.Add("Paused");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_ERROR) != 0
        )
        {
            Messages.Add("Error");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_PAPER_JAM) != 0
        )
        {
            Messages.Add("Paper jam");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_PAPER_OUT) != 0
        )
        {
            Messages.Add("Out of paper");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_OFFLINE) != 0
        )
        {
            Messages.Add("Offline");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_BUSY) != 0
        )
        {
            Messages.Add("Busy");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_PRINTING) != 0
        )
        {
            Messages.Add("Printing");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_PROCESSING) != 0
        )
        {
            Messages.Add("Processing");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_INITIALIZING) != 0
        )
        {
            Messages.Add("Initializing");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_WARMING_UP) != 0
        )
        {
            Messages.Add("Warming up");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_TONER_LOW) != 0
        )
        {
            Messages.Add("Toner low");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_NO_TONER) != 0
        )
        {
            Messages.Add("No toner");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_USER_INTERVENTION) != 0
        )
        {
            Messages.Add("User intervention required");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_DOOR_OPEN) != 0
        )
        {
            Messages.Add("Door open");
        }

        if
        (
            (Status &
            Win32Structures.PRINTER_STATUS_POWER_SAVE) != 0
        )
        {
            Messages.Add("Power save");
        }

        if (Messages.Count == 0)
        {
            return $"Unknown status flags: 0x{Status:X}";
        }

        return string.Join(", ", Messages);
    }
}