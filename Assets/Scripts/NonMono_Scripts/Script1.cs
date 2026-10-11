//namespaces = 
using UnityEngine;
using System.Collections.Generic;



public static class Script1
{
    //A bool which decides whether Auotmatic printing is turned on or off.
    //It is false by default every time a stationery logs in.
    public static bool AutomaticPrinting = false;

    //A bool which decides whether a Stationery is Closed / Open.
    public static bool Stationery_IsOperational_bool = false;









    //StationeryInfo = stores all details of a Stationery.
    public static StationeryInfo StationeryInfoObject = new StationeryInfo();

    //SetStationeryInfo() =
    //1. access stationery information from the server while logging in Scene2,
    //2. forget a user's information when the userr logs out in Scene4,
    //3. viewing stationery information in Scene4.
    public static void SetStationeryInfo(StationeryInfo Info)
    {
        if(Info != null)
        {
            PrintStationeryInfo();

            StationeryInfoObject = Info;

            Stationery_IsOperational_bool = StationeryInfoObject.IsOperational;

            return;
        }

        Debug.LogWarning("StationeryInfoObject is NULL.");
    }

    //4. SetStationeryInfo2() = Update stationery information in server in Scene4.
    //5. PrintStationerInfo() = Print stationery information whenever required ( Dev ).
    public static void SetStationeryInfo2(UpdatedStationeryInfo Info)
    {
        if (Info != null)
        {
            PrintStationeryInfo();

            StationeryInfoObject.StationeryName = Info.StationeryName;
            StationeryInfoObject.StationeryLocation = Info.StationeryLocation;
            StationeryInfoObject.Email = Info.Email;
            StationeryInfoObject.UPINumber = Info.UPINumber;

            return;
        }

        Debug.LogWarning("UpdatedStationeryInfoObject is NULL.");
    }
    public static void PrintStationeryInfo()
    {
        if (StationeryInfoObject == null)
        {
            Debug.LogWarning("StationeryInfoObject is NULL");
            return;
        }

        //Debug.Log("StationeryName         = " + StationeryInfoObject.StationeryName       +
       //           "\nStationerLocation    = " + StationeryInfoObject.StationeryLocation   +
       //         "\nStationeryID           = " + StationeryInfoObject.StationeryID         +
       //         "\nUPI Number             = " + StationeryInfoObject.UPINumber            +
       //         "\nPhone Number           = " + StationeryInfoObject.PhoneNumber          +
       //         "\nEmail                  = " + StationeryInfoObject.Email);  
    }












    //Printer list = stores important information about a printer.
    public static List<Printer> SelectedPrinters = new();

    //SetPrinters() = used to save printer preferences in Script1.
    public static void SetPrinters(List<Printer> printers)
    {
        if (printers != null)
        {
            SelectedPrinters = printers;

            return;
        }

        Debug.LogWarning("Printers list is NULL.");
    }

    //PrintAllPrinterDetails() = used to print details of all selected printers. 
    public static void PrintAllPrinterDetails()
    {
        foreach(Printer Printer in SelectedPrinters)
        {
            Printer.PrintPrinterDetails();
        }
    }












    //GetAutomaticPrintingValue() = used to obtain the AutomaticPrinting bool value from Script1.
    public static bool GetAutomaticPrintingValue()
    {
        return AutomaticPrinting;
    }

    //SetAutomaticPrintingValue() = used to give AutomaticPrinting bool a preffered value.
    public static void SetAutomaticPrintingValue(bool automaticPrinting)
    {
        AutomaticPrinting =  automaticPrinting;
    }
}
