//namespaces =
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

    // Stationery settings
    public bool AutomaticPrinting;

    //Printer supports =
    public bool SupportsDoubleSidedPrinting;
    public bool SupportsColorPrinting;

    //Pages =
    public int PageCapacity;
    public int PagesLeft;





    public void PrinterPrinterDetails()
    {
        Debug.Log("Name = " + Name);
        Debug.Log("PrinterID = " + PrinterID);

        Debug.Log("IsSelected = " + IsSelected);
        Debug.Log("IsOnline = " + IsOnline);
        Debug.Log("IsBusy = " + IsBusy);

        Debug.Log("AutomaticPrinting = " + AutomaticPrinting);

        Debug.Log("SupportsDoubleSidedPrinting = " + SupportsDoubleSidedPrinting);
        Debug.Log("SupportsColorPrinting = " + SupportsColorPrinting);

        Debug.Log("PageCapacity = " + PageCapacity);
        Debug.Log("PagesLeft = " + PagesLeft);
    }
}