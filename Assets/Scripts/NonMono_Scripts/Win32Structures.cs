using System;
using System.Runtime.InteropServices;

public static class Win32Structures
{
    //==================================================
    // Printer Access Rights
    //==================================================

    public const int PRINTER_ACCESS_USE = 0x00000008;
    public const int PRINTER_ACCESS_ADMINISTER = 0x00000004;


    //==================================================
    // EnumJobs / SetJob
    //==================================================

    public const int JOB_CONTROL_PAUSE = 1;
    public const int JOB_CONTROL_RESUME = 2;
    public const int JOB_CONTROL_CANCEL = 3;


    //==================================================
    // JOB_INFO_1 Status Flags
    //==================================================

    public const int JOB_STATUS_PAUSED = 0x00000001;
    public const int JOB_STATUS_ERROR = 0x00000002;
    public const int JOB_STATUS_DELETING = 0x00000004;
    public const int JOB_STATUS_SPOOLING = 0x00000008;
    public const int JOB_STATUS_PRINTING = 0x00000010;
    public const int JOB_STATUS_OFFLINE = 0x00000020;
    public const int JOB_STATUS_PAPEROUT = 0x00000040;
    public const int JOB_STATUS_PRINTED = 0x00000080;
    public const int JOB_STATUS_DELETED = 0x00000100;
    public const int JOB_STATUS_BLOCKED_DEVQ = 0x00000200;
    public const int JOB_STATUS_USER_INTERVENTION = 0x00000400;
    public const int JOB_STATUS_RESTART = 0x00000800;
    public const int JOB_STATUS_COMPLETE = 0x00001000;
    public const int JOB_STATUS_RETAINED = 0x00002000;
    public const int JOB_STATUS_RENDERING_LOCALLY = 0x00004000;


    //==================================================
    // PRINTER_INFO_2 Status Flags
    //
    // These are printer-level flags, not job-level flags.
    //==================================================

    public const int PRINTER_STATUS_PAUSED = 0x00000001;
    public const int PRINTER_STATUS_ERROR = 0x00000002;
    public const int PRINTER_STATUS_PENDING_DELETION = 0x00000004;
    public const int PRINTER_STATUS_PAPER_JAM = 0x00000008;
    public const int PRINTER_STATUS_PAPER_OUT = 0x00000010;
    public const int PRINTER_STATUS_MANUAL_FEED = 0x00000020;
    public const int PRINTER_STATUS_PAPER_PROBLEM = 0x00000040;
    public const int PRINTER_STATUS_OFFLINE = 0x00000080;
    public const int PRINTER_STATUS_IO_ACTIVE = 0x00000100;
    public const int PRINTER_STATUS_BUSY = 0x00000200;
    public const int PRINTER_STATUS_PRINTING = 0x00000400;
    public const int PRINTER_STATUS_OUTPUT_BIN_FULL = 0x00000800;
    public const int PRINTER_STATUS_NOT_AVAILABLE = 0x00001000;
    public const int PRINTER_STATUS_WAITING = 0x00002000;
    public const int PRINTER_STATUS_PROCESSING = 0x00004000;
    public const int PRINTER_STATUS_INITIALIZING = 0x00008000;
    public const int PRINTER_STATUS_WARMING_UP = 0x00010000;
    public const int PRINTER_STATUS_TONER_LOW = 0x00020000;
    public const int PRINTER_STATUS_NO_TONER = 0x00040000;
    public const int PRINTER_STATUS_PAGE_PUNT = 0x00080000;
    public const int PRINTER_STATUS_USER_INTERVENTION = 0x00100000;
    public const int PRINTER_STATUS_OUT_OF_MEMORY = 0x00200000;
    public const int PRINTER_STATUS_DOOR_OPEN = 0x00400000;
    public const int PRINTER_STATUS_SERVER_UNKNOWN = 0x00800000;
    public const int PRINTER_STATUS_POWER_SAVE = 0x01000000;


    //==================================================
    // PRINTER_DEFAULTS
    //==================================================

    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_DEFAULTS
    {
        public IntPtr pDatatype;
        public IntPtr pDevMode;
        public int DesiredAccess;
    }


    //==================================================
    // SYSTEMTIME
    //==================================================

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }


    //==================================================
    // JOB_INFO_1
    //==================================================

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct JOB_INFO_1
    {
        public int JobId;
        public string PrinterName;
        public string MachineName;
        public string UserName;
        public string Document;
        public string Datatype;
        public string StatusString;
        public int Status;
        public int Priority;
        public int Position;
        public int TotalPages;
        public int PagesPrinted;
        public SYSTEMTIME Submitted;
    }


    //==================================================
    // PRINTER_INFO_2
    //
    // Pointer fields are represented as IntPtr because we only
    // need pStatus and do not want to marshal the nested strings.
    //==================================================

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PRINTER_INFO_2
    {
        public IntPtr pServerName;
        public IntPtr pPrinterName;
        public IntPtr pShareName;
        public IntPtr pPortName;
        public IntPtr pDriverName;
        public IntPtr pComment;
        public IntPtr pLocation;
        public IntPtr pDevMode;
        public IntPtr pSepFile;
        public IntPtr pPrintProcessor;
        public IntPtr pDatatype;
        public IntPtr pParameters;

        public IntPtr pSecurityDescriptor;

        public int Attributes;
        public int Priority;
        public int DefaultPriority;
        public int StartTime;
        public int UntilTime;
        public int Status;
        public int cJobs;
        public int AveragePPM;
    }
}

