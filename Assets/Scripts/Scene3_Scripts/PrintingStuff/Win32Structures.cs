using System;
using System.Runtime.InteropServices;

public static class Win32Structures
{
    //==================================================
    // Printer Access Rights
    //==================================================

    public const int PRINTER_ACCESS_USE = 0x00000008;

    //==================================================
    // EnumJobs Flags
    //==================================================

    public const int JOB_CONTROL_PAUSE = 1;
    public const int JOB_CONTROL_RESUME = 2;
    public const int JOB_CONTROL_CANCEL = 3;

    //==================================================
    // Job Status Flags
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
}