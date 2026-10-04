//namespaces =
using UnityEngine;





[System.Serializable]
public class Printer
{
    // Display name from Windows
    public string Name;





    // The row ID of this printer in the stationery's database.
    // This is the numeric ID used for server calls like
    // /stationery/printer/pages-used/{id} and
    // /stationery/printer/pages-left/{id} - kept separate from
    // PrinterID above since that one is a Windows-side identifier
    // and may just be the printer's Name.
    public int StationeryPrinterID;





    // User selection
    public bool IsSelected;

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
        Debug.Log("StationeryPrinterID = " + StationeryPrinterID);

        Debug.Log("IsSelected = " + IsSelected);
        
        Debug.Log("IsOnline = " + IsOnline);
        Debug.Log("IsBusy = " + IsBusy);

        Debug.Log("AutomaticPrinting = " + AutomaticPrinting);

        Debug.Log("SupportsDoubleSidedPrinting = " + SupportsDoubleSidedPrinting);
        Debug.Log("SupportsColorPrinting = " + SupportsColorPrinting);

        Debug.Log("PageCapacity = " + PageCapacity);
        Debug.Log("PagesLeft = " + PagesLeft);
    }




    //To refil PagesLeft, i.e. PagesLeft = Page
    public void RefillPagesTo_PageCapacity()
    {
        PagesLeft = PageCapacity;

        Debug.Log("Pages in Printer : " + Name + " have been refilled.");
    }
}