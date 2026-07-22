using UnityEngine;

public static class Script1
{
    public static StationeryInfo StationeryInfoObject = new StationeryInfo();

    public static void SetStationeryInfo(StationeryInfo Info)
    {
        if(Info != null)
        {
            PrintStationeryInfo();
        }

        StationeryInfoObject = Info;
    }


    public static void SetStationeryInfo2(UpdatedStationeryInfo Info)
    {
        if (Info != null)
        {
            PrintStationeryInfo();
        }

        StationeryInfoObject.StationeryName     = Info.StationeryName;
        StationeryInfoObject.StationeryLocation = Info.StationeryLocation;
        StationeryInfoObject.Email              = Info.Email;
        StationeryInfoObject.UPINumber          = Info.UPINumber;
    }


    public static void PrintStationeryInfo()
    {
        if (StationeryInfoObject == null)
        {
            Debug.Log("StationeryInfoObject is NULL");
            return;
        }

        //Debug.Log("StationeryName       = " + StationeryInfoObject.StationeryName       +
        //          "\nStationerLocation  = " + StationeryInfoObject.StationeryLocation   +
        //          "\nStationeryID       = " + StationeryInfoObject.StationeryID         +
        //          "\nUPI Number         = " + StationeryInfoObject.UPINumber            +
        //          "\nPhone Number       = " + StationeryInfoObject.PhoneNumber          +
        //          "\nEmail              = " + StationeryInfoObject.Email);  
    }
}
