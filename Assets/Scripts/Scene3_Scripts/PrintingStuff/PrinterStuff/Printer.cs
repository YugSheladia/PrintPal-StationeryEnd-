using UnityEngine;
[System.Serializable]
public class Printer
{
    // Display name from Windows
    public string Name;

    // Used to identify the printer uniquely
    public string PrinterID;

    // User selection
    public bool IsSelected;

    // Runtime state
    public bool IsOnline;
    public bool IsBusy;

    // Stationery setting
    public bool AutomaticPrinting;
}