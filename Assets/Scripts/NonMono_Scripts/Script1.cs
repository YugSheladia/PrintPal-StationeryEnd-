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


    public static void PrintStationeryInfo()
    {
        if (StationeryInfoObject == null)
        {
            Debug.Log("StationeryInfoObject is NULL");
            return;
        }

        Debug.Log("StationeryName   = " + StationeryInfoObject.StationeryName   +
                  "\nStationeryID     = " + StationeryInfoObject.StationeryID     +
                  "\nUPI Number       = " + StationeryInfoObject.UPINumber        +
                  "\nPhone Number     = " + StationeryInfoObject.PhoneNumber      +
                  "\nEmail            = " + StationeryInfoObject.Email);  
    }
}
