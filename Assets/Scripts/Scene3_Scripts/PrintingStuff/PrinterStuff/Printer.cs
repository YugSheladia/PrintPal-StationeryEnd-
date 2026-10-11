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


    //Speed of printing ( PagesPerMinute ~ PPM )
    public int PPM;



    //This enum is used for storing the current status of the Printer.
    //Ths status could be - Caution, Low, Medium, High.
    public enum PagesLeft_Indicator
    {
        Caution,
        Low,
        Medium,
        High
    }

    public PagesLeft_Indicator PagesLeft_IndicatorEnum = PagesLeft_Indicator.High;






    //This function checks the PagesLeft / PageCapacity * 100, and sets the PagesLeft_Indicator accordingly.
    //This function is called -
    //1. After every RefreshUI().
    //2. After a PrintWorker.PrintDocument() -> PagesLeft_UpdationProcess() -> ... -> UpdatePagesLocally().
    //3. After everytime a user clicks on Queue button in HomePage.
    public void Update_PagesLeftIndicator()
    {
        float PagesLeft_Percentage = (float)PagesLeft / PageCapacity * 100;

        //Debug.Log("PagesLeft    = " + PagesLeft);
        //Debug.Log("PageCapacity = " + PageCapacity);
        //Debug.Log("PagesLeftPercentage = " + PagesLeft_Percentage);


        if (PagesLeft_Percentage < 10)
        {
            PagesLeft_IndicatorEnum = PagesLeft_Indicator.Caution;
            return;
        }
        if (PagesLeft_Percentage < 33)
        {
            PagesLeft_IndicatorEnum = PagesLeft_Indicator.Low;
            return;
        }
        if (PagesLeft_Percentage < 66)
        {
            PagesLeft_IndicatorEnum = PagesLeft_Indicator.Medium;
            return;
        }

        PagesLeft_IndicatorEnum = PagesLeft_Indicator.High;
    }





    //This function prints all the details of the printer.
    public void PrintPrinterDetails()
    {
        Debug.Log("Name = "                     + Name + 
            "\nStationeryPrinterID = "          + StationeryPrinterID + 
            "\nIsSelected = "                   + IsSelected + 
            "\nIsOnline = "                     + IsOnline + 
            "\nIsBusy = "                       + IsBusy + 
            "\nAutomaticPrinting = "            + AutomaticPrinting + 
            "\nSupportsDoubleSidedPrinting = "  + SupportsDoubleSidedPrinting + 
            "\nSupportsColorPrinting = "        + SupportsColorPrinting + 
            "\nPPM = "                          + PPM +
            "\nPageCapacity = "                 + PageCapacity +
            "\nPagesLeft = "                    + PagesLeft +
            "\nPagesLeft_Indicator = "          + PagesLeft_IndicatorEnum.ToString());
    }






    //To refil PagesLeft, i.e. PagesLeft = Page
    public void RefillPagesTo_PageCapacity()
    {
        PagesLeft = PageCapacity;

        Debug.Log("Pages in Printer : " + Name + " have been refilled.");
    }
}