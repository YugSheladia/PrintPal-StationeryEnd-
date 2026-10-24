//namespaces =
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;






//=========================================================================
//To recieve EVERY INFO possible about a order issued.
//=========================================================================

// ORDERS TABLE
[System.Serializable]
public class OrderSummary
{
    public int OrderID;

    public int UserID;
    public string Username;

    public int StationeryID;

    public int OrderOTP;

    public int ApproximateTimeLeft;

    public float FinalAmount;

    public int Status;
    public int ArchivedByStationery;

    public string CreatedAt;
    public string CompletedAt;
    public string CollectedAt;
    public string FailedAt;
    public string DeletedAt;

    public string ArchivedByStationeryAt;

    // Runtime only.
    public List<PrintDetails> Prints = new();

    public bool failedToDownloadOrder = false;
}

[System.Serializable]
public class OrderSummaryWrapper
{
    public bool Success;
    public string Message;

    public OrderSummary[] Orders;
}


// PRINTS TABLE
[System.Serializable]
public class PrintSummary
{
    public int PrintID;

    public int PrintNumber;

    public int OrderID;

    public string PDFName;

    public int TotalPages;

    public int Status;

    public bool failedToDownloadPrint = false;
}

[System.Serializable]
public class PrintSummaryWrapper
{
    public bool Success;
    public string Message;

    public PrintSummary[] Prints;
}


// PRINT SETTINGS TABLE
[System.Serializable]
public class PrintSettingsSummary
{
    public int PrintSettingsID;

    public int PrintID;

    public int PageSelection;

    public int SideSelection;

    public int Collated;

    public int Orientation;

    public int Margin;

    public int PagesPerSheet;

    public int Color;

    public int Copies;

    public bool failedToDownloadPrintSettings = false;
}

[System.Serializable]
public class PrintSettingsSummaryWrapper
{
    public bool Success;
    public string Message;

    public PrintSettingsSummary[] PrintSettings;
}


// PRINT BILLS TABLE
[System.Serializable]
public class PrintBillsSummary
{
    public int PrintBillID;

    public int PrintID;

    public int TotalPages;

    public int EffectiveSheets;

    public int TotalCopies;

    public int Color;

    public int SideSelection;

    public int PagesPerSheet;

    public float BillAmount;

    public bool failedToDownloadPrintBills = false;
}

[System.Serializable]
public class PrintBillsSummaryWrapper
{
    public bool Success;
    public string Message;

    public PrintBillsSummary[] PrintBills;
}


// PrintDetails
public enum PageSelectionEnum
{
    AllPages,
    OddPages,
    EvenPages
}
public enum SideSelectionEnum
{
    SingleSided,
    DoubleSided
}
public enum CollatedOrNotEnum
{
    Collated,
    Uncollated
}
public enum OrientationEnum
{
    Portrait,
    Landscape
}
public enum MarginEnum
{
    Normal,
    Narrow,
    Moderate,
    Wide,
    Mirrored,
    DoubleSided
}
public enum PagesPerSheetEnum
{
    _1,
    _2,
    _4,
    _8,
    _16
}
public enum ColorEnum
{
    BlackAndWhite,
    Color
}
public class PrintDetails
{
    public int OrderID;
    public int PrintID;

    public int PrintNumber;

    public string PDFName;

    public string LocalPDFPath;

    public PageSelectionEnum PageSelection = PageSelectionEnum.AllPages;

    public SideSelectionEnum SideSelection = SideSelectionEnum.SingleSided;

    public CollatedOrNotEnum CollatedOrNot = CollatedOrNotEnum.Collated;

    public OrientationEnum Orientation = OrientationEnum.Portrait;

    public MarginEnum Margin = MarginEnum.Normal;

    public PagesPerSheetEnum PagesPerSheet = PagesPerSheetEnum._1;

    public ColorEnum Color = ColorEnum.BlackAndWhite;

    public int Copies = 1;

    public int Pages = 1;

    public int EffectiveSheets;

    public float BillAmount;

    public bool failedToDownloadPrint = false;
    public bool failedToDownloadPrintSettings = false;
    public bool failedToDownloadPrintBills = false;

    public PrintDetails()
    {

    }

    // Copy Constructor
    public PrintDetails(PrintDetails Source)
    {
        PrintNumber = Source.PrintNumber;

        PDFName = Source.PDFName;

        PageSelection = Source.PageSelection;

        SideSelection = Source.SideSelection;

        CollatedOrNot = Source.CollatedOrNot;

        Orientation = Source.Orientation;

        Margin = Source.Margin;

        PagesPerSheet = Source.PagesPerSheet;

        Color = Source.Color;

        Copies = Source.Copies;

        Pages = Source.Pages;
    }

    public void PrintOrderDetails()
    {
        Debug.Log
        (
            "#" + (PrintNumber + 1) +
            "\nPDF Name         = " + PDFName +
            "\nPage Selection   = " + PageSelection +
            "\nSide Selection   = " + SideSelection +
            "\nCollated         = " + CollatedOrNot +
            "\nOrientation      = " + Orientation +
            "\nMargin           = " + Margin +
            "\nPagesPerSheet    = " + PagesPerSheet +
            "\nCopies           = " + Copies +
            "\nPages            = " + Pages
        );
    }
}












//==================================================
// Main Class
//==================================================

public class Scene3Backend : MonoBehaviour
{
    //Script references =
    [SerializeField] HomePage HomePageScript;
    [SerializeField] Scene3Manager Scene3ManagerScript;

    [SerializeField] OrderStatusManager OrderStatusManagerScript;

    [SerializeField] PrintManager PrintManagerScript;

    [SerializeField] RevenuePageManager RevenuePageManagerScript;

    [SerializeField] QueueManager QueueManagerScript;




    //Wrapper objects creation.
    public OrderSummaryWrapper ConfirmedOrders = new();

    public PrintSummaryWrapper ConfirmedPrints = new();

    public PrintSettingsSummaryWrapper ConfirmedPrintSettings = new();

    public PrintBillsSummaryWrapper ConfirmedPrintBills = new();




    //This is an extremely important bool, Why?
    //When a user clicks on Pending, Printed, Collected, Archived orders, or when AutomaticPrinting() tries to get all Pending orders, it is important that these requests dont overlap.
    //This is because these all these different request types recieve and store order in one common object.
    //This is why all of these requests are handled with a single IsBusy bool. If IsBusy is false, then the request is accepted, otherwise the request waits until the IsBusy becomes false.
    public bool IsBusy = false;









    //To check if the app is connected to the server or not =
    public IEnumerator CheckServer(System.Action<bool> OnComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/health"))
        {
            yield return request.SendWebRequest();

            bool isOnline = request.result == UnityWebRequest.Result.Success;

            Debug.Log(isOnline ? "Server is ONLINE" : "Server is OFFLINE");

            OnComplete?.Invoke(isOnline);
        }
    }











    //To Open / Close a stationery =

    [System.Serializable]
    public class OperationalRequest
    {
        public bool IsOperational;
    }

    public IEnumerator SetStationeryOperational(int stationeryID, bool isOperational, System.Action<bool> OnComplete)
    {
        string json = JsonUtility.ToJson(new OperationalRequest
        {
            IsOperational = isOperational
        });

        using (UnityWebRequest request = new UnityWebRequest(
            "http://localhost:3000/stationery/operational/" + stationeryID,
            "PUT"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("Operational status updated.");

                OnComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError(request.error);

                OnComplete?.Invoke(false);
            }
        }
    }









    //To get orders, belonging to a stationery, of a specific status.
    public IEnumerator GetPendingOrders(int StationeryID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        //Debug.Log("Downloading Pending Orders...");



        //Enabling IsBusy to make sure requests don't overlap.
        IsBusy = true;



        //Cleaning up ConfirmedOrders, Prints, PrintSettings, & PrintBills =
        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];



        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/pending?StationeryID={StationeryID}"
        );



        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        //Debug.Log(request.downloadHandler.text);
#endif


        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError("Server Error: " + Response.Message);
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders = Response.Orders ?? new OrderSummary[0];

        //Debug.Log
        //(
        //    $"Downloaded {ConfirmedOrders.Orders.Length} Pending Orders."
        //);

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);
            IsBusy = false;

            yield break;
        }

        //Debug.Log("Finished Downloading Pending Orders.");
        IsBusy = false;

        OnComplete(true);
    }
    public IEnumerator GetCompletedOrders(int StationeryID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Downloading Completed Orders...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/completed?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Orders Download Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Downloaded {ConfirmedOrders.Orders.Length} Completed Orders."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);

            IsBusy = false;

            yield break;
        }

        Debug.Log("Finished Downloading Completed Orders.");
        IsBusy = false;

        OnComplete(true);
    }
    public IEnumerator GetCollectedOrders(int StationeryID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Downloading Collected Orders...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/collected?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Orders Download Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Downloaded {ConfirmedOrders.Orders.Length} Collected Orders."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);

            IsBusy = false;

            yield break;
        }

        Debug.Log("Finished Downloading Collected Orders.");
        IsBusy = false;

        OnComplete(true);
    }
    public IEnumerator GetFailedOrders(int StationeryID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Downloading Failed Orders...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/failed?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Orders Download Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Downloaded {ConfirmedOrders.Orders.Length} Failed Orders."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);

            IsBusy = false;

            yield break;
        }

        Debug.Log("Finished Downloading Failed Orders.");
        IsBusy = false;

        OnComplete(true);
    }
    public IEnumerator GetArchivedOrders(int StationeryID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Downloading Archived Orders...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/stationeryOrders/archived?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Orders Download Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Downloaded {ConfirmedOrders.Orders.Length} Archived Orders."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);

            IsBusy = false;

            yield break;
        }

        Debug.Log("Finished Downloading Archived Orders.");
        IsBusy = false;

        OnComplete(true);
    }








    //To get Prints, PrintBills, and PrintSettings of Orders of a specific status
    //( Called by functions above -> GetPendingOrders(), GetCompletedOrders(), GetCollectedOrders(), GetArchivedOrders() ).

    //Therefore, GetOrders() -> GetPrints() -> GetPrintSettings() -> GetPrintBills() -> BuildPrintDetails().
    IEnumerator GetPrints(System.Action<bool> OnComplete)
    {
        //Debug.Log("Downloading Prints...");

        List<PrintSummary> AllPrints = new();

        foreach (OrderSummary Order in ConfirmedOrders.Orders)
        {
            UnityWebRequest request = UnityWebRequest.Get
            (
                "http://localhost:3000/prints/order/"
                + Order.OrderID
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Prints For Order "
                    + Order.OrderID
                );

                OnComplete(false);
                yield break;
            }

#if UNITY_EDITOR
            Debug.Log(request.downloadHandler.text);
#endif

            PrintSummaryWrapper Response =
                JsonUtility.FromJson<PrintSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null || !Response.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Prints For Order "
                    + Order.OrderID
                );

                OnComplete(false);
                yield break;
            }

            if (Response.Prints != null)
            {
                AllPrints.AddRange(Response.Prints);
            }
        }

        ConfirmedPrints.Prints = AllPrints.ToArray();

        bool SettingsSuccess = false;

        yield return StartCoroutine
        (
            GetPrintSettings(Result => SettingsSuccess = Result)
        );

        if (!SettingsSuccess)
        {
            OnComplete(false);
            yield break;
        }

        //Debug.Log("Finished Downloading Prints.");

        OnComplete(true);
    }
    IEnumerator GetPrintSettings(System.Action<bool> OnComplete)
    {
        //Debug.Log("Downloading Print Settings...");

        List<PrintSettingsSummary> AllPrintSettings = new();

        foreach (PrintSummary Print in ConfirmedPrints.Prints)
        {
            UnityWebRequest request = UnityWebRequest.Get
            (
                "http://localhost:3000/settings/print/"
                + Print.PrintID
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Settings For Print "
                    + Print.PrintID
                );

                OnComplete(false);
                yield break;
            }

            PrintSettingsSummaryWrapper Response =
                JsonUtility.FromJson<PrintSettingsSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null || !Response.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Settings For Print "
                    + Print.PrintID
                );

                OnComplete(false);
                yield break;
            }

            if (Response.PrintSettings != null)
            {
                AllPrintSettings.AddRange(Response.PrintSettings);
            }
        }

        ConfirmedPrintSettings.PrintSettings =
            AllPrintSettings.ToArray();

        bool BillsSuccess = false;

        yield return StartCoroutine
        (
            GetPrintBills(Result => BillsSuccess = Result)
        );

        if (!BillsSuccess)
        {
            OnComplete(false);
            yield break;
        }

        //Debug.Log("Finished Downloading Print Settings.");

        OnComplete(true);
    }
    IEnumerator GetPrintBills(System.Action<bool> OnComplete)
    {
        //Debug.Log("Downloading Print Bills...");

        List<PrintBillsSummary> AllPrintBills = new();

        foreach (PrintSummary Print in ConfirmedPrints.Prints)
        {
            UnityWebRequest request = UnityWebRequest.Get
            (
                "http://localhost:3000/bill/print/"
                + Print.PrintID
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Bill For Print "
                    + Print.PrintID
                );

                OnComplete(false);
                yield break;
            }

            PrintBillsSummaryWrapper Response =
                JsonUtility.FromJson<PrintBillsSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null || !Response.Success)
            {
                Debug.LogError
                (
                    "Failed To Download Bill For Print "
                    + Print.PrintID
                );

                OnComplete(false);
                yield break;
            }

            if (Response.PrintBills != null)
            {
                AllPrintBills.AddRange(Response.PrintBills);
            }
        }

        ConfirmedPrintBills.PrintBills =
            AllPrintBills.ToArray();

        BuildPrintDetails();

        //Debug.Log("Finished Downloading Everything.");

        OnComplete(true);
    }







    //Making a PrintDetails object for each prints of each order ( of a specific order status ).

    //This basically gets all the info from Print, PrintSettings, & PrintBills table, and combines it into a single class called PrintDetails.

    public Dictionary<int, PrintSettingsSummary> SettingsDictionary = new();
    public Dictionary<int, PrintBillsSummary> BillsDictionary = new();
    public Dictionary<int, List<PrintSummary>> PrintsDictionary = new();
    void BuildPrintDetails()
    {
        //Debug.Log("Building Print Details...");

        SettingsDictionary.Clear();
        BillsDictionary.Clear();
        PrintsDictionary.Clear();

        foreach (PrintSettingsSummary Setting in ConfirmedPrintSettings.PrintSettings)
        {
            SettingsDictionary[Setting.PrintID] = Setting;
        }

        foreach (PrintBillsSummary Bill in ConfirmedPrintBills.PrintBills)
        {
            BillsDictionary[Bill.PrintID] = Bill;
        }

        foreach (PrintSummary Print in ConfirmedPrints.Prints)
        {
            if (!PrintsDictionary.ContainsKey(Print.OrderID))
            {
                PrintsDictionary[Print.OrderID] = new List<PrintSummary>();
            }

            PrintsDictionary[Print.OrderID].Add(Print);
        }

        //==================================================
        // Build Orders
        //==================================================

        foreach (OrderSummary Order in ConfirmedOrders.Orders)
        {
            Order.Prints ??= new List<PrintDetails>();
            Order.Prints.Clear();

            if (!PrintsDictionary.TryGetValue(Order.OrderID, out List<PrintSummary> OrderPrints))
            {
                continue;
            }

            foreach (PrintSummary Print in OrderPrints)
            {
                if (!SettingsDictionary.TryGetValue(Print.PrintID, out PrintSettingsSummary Settings))
                {
                    Debug.LogWarning
                    (
                        "Missing Print Settings for PrintID = "
                        + Print.PrintID
                    );

                    continue;
                }

                if (!BillsDictionary.TryGetValue(Print.PrintID, out PrintBillsSummary Bill))
                {
                    Debug.LogWarning
                    (
                        "Missing Print Bill for PrintID = "
                        + Print.PrintID
                    );

                    continue;
                }

                PrintDetails Details = new PrintDetails();

                Details.OrderID = Order.OrderID;

                Details.PrintID = Print.PrintID;
                Details.PrintNumber = Print.PrintNumber;

                Details.PDFName = Print.PDFName;

                Details.Pages = Print.TotalPages;

                Details.Copies = Settings.Copies;

                Details.PageSelection =
                    (PageSelectionEnum)Settings.PageSelection;

                Details.SideSelection =
                    (SideSelectionEnum)Settings.SideSelection;

                Details.CollatedOrNot =
                    (CollatedOrNotEnum)Settings.Collated;

                Details.Orientation =
                    (OrientationEnum)Settings.Orientation;

                Details.Margin =
                    (MarginEnum)Settings.Margin;

                Details.PagesPerSheet =
                    (PagesPerSheetEnum)Settings.PagesPerSheet;

                Details.Color =
                    (ColorEnum)Settings.Color;

                Details.EffectiveSheets = Bill.EffectiveSheets;

                Details.BillAmount = Bill.BillAmount;

                Order.Prints.Add(Details);

#if UNITY_EDITOR
                Details.PrintOrderDetails();
#endif
            }

            //Debug.Log("GetUsername()");
            //GetUsernameFromServer(Order);
        }

        //Debug.Log("Finished Building Print Details.");
    }







    //Get name of the customer ( for OrderStatus panels ).
    [System.Serializable]
    public class UserInfo
    {
        public int UserID;
        public string PhoneNumber;
        public string Name;
        public string UPINumber;
        public string Email;
        public string Username;
    }

    [System.Serializable]
    public class UsernameResponse
    {
        public bool Success;
        public string Message;
        public UserInfo User;
    }
    public void GetUsernameFromServer(OrderSummary Order)
    {
        StartCoroutine(GetUsername(Order));
    }
    public IEnumerator GetUsername(OrderSummary Order)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + Order.UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            //Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Order.Username = response.User.Name;

                Debug.Log("Name of user : " + Order.Username);
            }
            else
            {
                Debug.LogWarning("User not found.");

                Scene3ManagerScript.DisableAllPanels();
                Scene3ManagerScript.EnableHomePagePanel();

                Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
                Scene3ManagerScript.EnableMessagePanel();
            }
        }
        else
        {
            Debug.LogError(request.error);

            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();
        }
    }







    //=============================
    //To set Order's status.
    //=============================



    [System.Serializable]
    public class OrderIDRequest
    {
        public int OrderID;
    }



    //This bool is switched to true when such a coroutine is called which starts a transacion in the database,
    //and is switched to false only when the transaction has been completed.

    public bool CheckFor_OngoingTransaction = false;





    public void SetOrderAsPending(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsPending
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }
    public void SetOrderAsCompleted(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsCompleted
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }
    public void SetOrderAsCollected(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsCollected
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }
    public void SetOrderAsFailed(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsFailed
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }
    public void SetOrderAsUnarchived(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsUnarchived
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }





    void HandleStandaloneStatusChange(bool Success, GameObject Panel)
    {
        CheckFor_OngoingTransaction = false;

        //While OrderStatusManager is changing many orders, it controls the loading screen itself ( once, for the whole process ).
        if (!OrderStatusManagerScript.IsBusy)
        {
            Scene3ManagerScript.DisableInvisiblePanel();
        }

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text =
                "Something went wrong, try again later.";

            Scene3ManagerScript.EnableMessagePanel();
            return;
        }

        if (Panel)
        {
            OrderStatusManagerScript.Remove_OrderStatusPanel
            (
                Panel.GetComponent<OrderStatus>()
            );

            Destroy(Panel);
        }
    }






    public IEnumerator SetOrderAsPending(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/pending",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Mark Order As Pending: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Pending.");

        OnComplete(true);
    }
    public IEnumerator SetOrderAsCompleted(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/completed",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Mark Order As Completed: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Completed.");

        OnComplete(true);
    }
    public IEnumerator SetOrderAsCollected(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/collected",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Mark Order As Collected: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Collected.");

        OnComplete(true);
    }
    public IEnumerator SetOrderAsFailed(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/failed",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Mark Order As Failed: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Failed.");

        OnComplete(true);
    }
    public IEnumerator SetOrderAsUnarchived(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/stationery-unarchive",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Unarchive Order "
                + OrderID
                + ": "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Unarchived.");

        OnComplete(true);
    }





    //Same as the other SetOrderAs...(int, GameObject) functions: on success the panel is removed from OrderStatusManager's list and destroyed.
    public void SetOrderAsArchived(int OrderID, GameObject Panel)
    {
        CheckFor_OngoingTransaction = true;

        StartCoroutine
        (
            SetOrderAsArchived
            (
                OrderID,
                Success => HandleStandaloneStatusChange
                (
                    Success,
                    Panel
                )
            )
        );
    }

    //Coroutine version, used by OrderStatusManager to archive orders one after another.
    public IEnumerator SetOrderAsArchived(int OrderID, System.Action<bool> OnComplete)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/order/stationery-archive",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed To Archive Order "
                + OrderID
                + ": "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        Debug.Log("Order marked as Archived.");

        OnComplete(true);
    }










    //Search functionality -
    public IEnumerator SearchByOrderIDCoroutine(int StationeryID, int OrderID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Searching Order...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/SearchByOrderID?StationeryID={StationeryID}&OrderID={OrderID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Order Search Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Order JSON Parsing Failed.");

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Found {ConfirmedOrders.Orders.Length} Order(s)."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        Debug.Log("Finished Searching Order.");

        IsBusy = false;

        OnComplete(true);
    }

    public IEnumerator SearchByUserIDCoroutine(int StationeryID, int UserID, System.Action<bool> OnComplete)
    {
        // Wait for any other order fetch already in progress (manual or the
        // automatic-printing background poll) to finish first, rather than
        // corrupting its in-flight shared Confirmed*/ state.
        while (IsBusy)
        {
            yield return null;
        }

        Debug.Log("Searching User Orders...");

        IsBusy = true;

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/SearchByUserID?StationeryID={StationeryID}&UserID={UserID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "User Orders Search Failed: "
                + request.error
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper Response =
            JsonUtility.FromJson<OrderSummaryWrapper>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError
            (
                "Server Error: "
                + Response.Message
            );

            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Found {ConfirmedOrders.Orders.Length} Order(s)."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            IsBusy = false;

            OnComplete(false);
            yield break;
        }

        Debug.Log("Finished Searching User Orders.");

        IsBusy = false;

        OnComplete(true);
    }










    //Order count -
    [System.Serializable]
    public class OrderCountResponse
    {
        public bool Success;
        public string Message;

        public int TotalOrders;
    }
    public IEnumerator GetOrdersCount(System.Action<bool> OnComplete)
    {
        bool PendingSuccess = false;

        yield return StartCoroutine
        (
            GetOrderCount
            (
                "pending",
                HomePageScript.TotalPendingOrdersText,
                Result => PendingSuccess = Result
            )
        );

        if (!PendingSuccess)
        {
            OnComplete(false);
            yield break;
        }


        bool CompletedSuccess = false;

        yield return StartCoroutine
        (
            GetOrderCount
            (
                "completed",
                HomePageScript.TotalPrintedOrdersText,
                Result => CompletedSuccess = Result
            )
        );

        if (!CompletedSuccess)
        {
            OnComplete(false);
            yield break;
        }


        bool CollectedSuccess = false;

        yield return StartCoroutine
        (
            GetOrderCount
            (
                "collected",
                HomePageScript.TotalCollectedOrdersText,
                Result => CollectedSuccess = Result
            )
        );

        if (!CollectedSuccess)
        {
            OnComplete(false);
            yield break;
        }

        bool FailedSuccess = false;

        yield return StartCoroutine
        (
            GetOrderCount
            (
                "failed",
                HomePageScript.TotalFailedOrdersText,
                Result => FailedSuccess = Result
            )
        );

        if (!FailedSuccess)
        {
            OnComplete(false);
            yield break;
        }

        OnComplete(true);
    }
    public IEnumerator GetOrderCount(string status, TMP_Text targetText, System.Action<bool> OnComplete)
    {
        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/count/{status}?StationeryID={Script1.StationeryInfoObject.StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed to get "
                + status
                + " order count: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

        OrderCountResponse Response =
            JsonUtility.FromJson<OrderCountResponse>
            (
                request.downloadHandler.text
            );

        if (Response == null || !Response.Success)
        {
            Debug.LogError
            (
                "Failed to get "
                + status
                + " order count."
            );

            OnComplete(false);
            yield break;
        }

        targetText.text = Response.TotalOrders.ToString();

        OnComplete(true);
    }










    //Get stationery's revenue.
    [System.Serializable]
    public class EarningsResponse
    {
        public bool Success;
        public string Message;

        public float Amount;
    }
    public IEnumerator GetAllTimeStationeryEarnings(int StationeryID)
    {
        UnityWebRequest request = UnityWebRequest.Get(
            $"http://localhost:3000/analytics/stationeryEarnings?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get stationery earnings: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        EarningsResponse response =
            JsonUtility.FromJson<EarningsResponse>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError(response.Message);
            yield break;
        }

        Debug.Log("Stationery Earnings: " + response.Amount);

        //HomePageScript.StationeryEarningsText.text = "`" + response.Amount.ToString("F2");
    }
    public IEnumerator GetTodayStationeryRevenue(int StationeryID, System.Action<bool> OnComplete)
    {
        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/analytics/stationery/todayEarnings?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed to get today's revenue: "
                + request.error
            );

            OnComplete(false);
            yield break;
        }

#if UNITY_EDITOR
        //Debug.Log(request.downloadHandler.text);
#endif

        EarningsResponse Response =
            JsonUtility.FromJson<EarningsResponse>
            (
                request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("JSON Parsing Failed.");

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError(Response.Message);

            OnComplete(false);
            yield break;
        }

        //Debug.Log
        //(
        //   "Today's Revenue: "
        //    + Response.Amount
        //);

        HomePageScript.RevenueToday.text =
            Response.Amount.ToString("F2");

        OnComplete(true);
    }








    //Only to be used by PrintPal end.
    public IEnumerator GetPrintPalEarnings()
    {
        UnityWebRequest request = UnityWebRequest.Get(
            "http://localhost:3000/analytics/printpalEarnings"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get PrintPal earnings: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        EarningsResponse response =
            JsonUtility.FromJson<EarningsResponse>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError(response.Message);
            yield break;
        }

        Debug.Log("PrintPal Earnings: " + response.Amount);

        // Example:
        // HomePageScript.PrintPalEarningsText.text = "`" + response.Amount.ToString("F2");
    }
    public IEnumerator GetTotalRevenue()
    {
        UnityWebRequest request = UnityWebRequest.Get(
            "http://localhost:3000/analytics/totalRevenue"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get total revenue: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        EarningsResponse response =
            JsonUtility.FromJson<EarningsResponse>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError(response.Message);
            yield break;
        }

        Debug.Log("Total Revenue: " + response.Amount);

        // Example:
        // HomePageScript.TotalRevenueText.text = "`" + response.Amount.ToString("F2");
    }







    //==================================================
    // 30 days Revenue
    //==================================================

    public void LoadRevenue(int StationeryID)
    {
        GetRevenueFromServer
        (
            StationeryID,
            RevenueRows =>
            {
                if (RevenueRows == null)
                {
                    Scene3ManagerScript.DisableInvisiblePanel();

                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                if (RevenueRows.Length <= 0)
                {
                    Scene3ManagerScript.DisableInvisiblePanel();

                    Scene3ManagerScript.MessageText.text = "No revenue has been made in the last 31 days.";
                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                foreach (RevenueDetails Row in RevenueRows)
                {
                    Debug.Log
                    (
                        $"{Row.RevenueDate} | " +
                        $"Revenue = {Row.Revenue} | " +
                        $"Orders = {Row.TotalOrders}"
                    );

                    RevenuePageManagerScript.CreateRevenuePanel(Row);
                }

                Scene3ManagerScript.DisableAllPanels();
                Scene3ManagerScript.EnableRevenuePage();

                Scene3ManagerScript.DisableInvisiblePanel();
            }
        );
    }

    [System.Serializable]
    public class RevenueDetails
    {
        public int RevenueID;

        public int StationeryID;

        public string RevenueDate;

        public int TotalOrders;

        public int CompletedOrders;

        public float Revenue;

        public int TotalPagesPrinted;

        public string CreatedAt;
    }

    [System.Serializable]
    public class GetRevenueResponse
    {
        public bool Success;

        public RevenueDetails[] Revenue;
    }

    public void GetRevenueFromServer
    (
        int StationeryID,
        System.Action<RevenueDetails[]> OnComplete
    )
    {
        StartCoroutine
        (
            Coroutine_GetRevenueFromServer
            (
                StationeryID,
                OnComplete
            )
        );
    }
    public IEnumerator Coroutine_GetRevenueFromServer
    (
        int StationeryID,
        System.Action<RevenueDetails[]> OnComplete
    )
    {
        UnityWebRequest Request = UnityWebRequest.Get
        (
            "http://localhost:3000/stationery/30Days/revenue/" +
            StationeryID
        );

        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError
            (
                "Failed to retrieve revenue: " +
                Request.error
            );

            Scene3ManagerScript.MessageText.text = "Failed to retrieve revenue.";

            OnComplete?.Invoke(null);

            yield break;
        }

        Debug.Log
        (
            "Raw Revenue Response =\n" +
            Request.downloadHandler.text
        );

        GetRevenueResponse Response =
            JsonUtility.FromJson<GetRevenueResponse>
            (
                Request.downloadHandler.text
            );

        if (Response == null)
        {
            Debug.LogError("Revenue response is null.");

            Scene3ManagerScript.MessageText.text = "Revenue response is null.";

            OnComplete?.Invoke(null);

            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError("Failed to retrieve revenue.");

            Scene3ManagerScript.MessageText.text = "Failed to retrieve revenue.";

            OnComplete?.Invoke(null);

            yield break;
        }

        if (Response.Revenue == null)
        {
            Debug.Log("No revenue records found.");

            OnComplete?.Invoke(new RevenueDetails[0]);

            yield break;
        }

        Debug.Log
        (
            $"Retrieved {Response.Revenue.Length} revenue records."
        );

        OnComplete?.Invoke(Response.Revenue);
    }







    // Home page refreshing stuff

    private Coroutine HomePageRefreshCoroutine;

    private bool IsRefreshingHomePage = false;

    public void StartHomePageRefreshing()
    {
        if (HomePageRefreshCoroutine != null) return;

        HomePageRefreshCoroutine = StartCoroutine(Coroutine_HomePageRefreshing());
    }

    public void StopHomePageRefreshing()
    {
        if (HomePageRefreshCoroutine == null) return;

        StopCoroutine(HomePageRefreshCoroutine);

        HomePageRefreshCoroutine = null;
    }

    IEnumerator Coroutine_HomePageRefreshing()
    {
        while (true)
        {
            // Don't start another refresh while one is already running.
            if (!IsRefreshingHomePage)
            {
                IsRefreshingHomePage = true;

                RefreshUI();

                // Wait until RefreshUI() finishes.
                while (IsRefreshingHomePage)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(5f);
        }
    }







    //Automatic printing stuff

    private Coroutine AutomaticPrintingCoroutine;

    private bool IsPollingOrders = false;

    public void StartAutomaticPrinting()
    {
        if (AutomaticPrintingCoroutine != null)
        {
            Debug.Log("Automatic Printing Coroutine has already been started.");
            return;
        }

        Debug.Log("Automatic Printing Coroutine has been started.");
        AutomaticPrintingCoroutine = StartCoroutine(Coroutine_AutomaticPrinting());
    }

    public void StopAutomaticPrinting()
    {
        if (AutomaticPrintingCoroutine == null)
        {
            Debug.Log("Automatic Printing Coroutine has already been stopped.");
            return;
        }

        Debug.Log("Automatic Printing Coroutine has been stopped.");

        StopCoroutine(AutomaticPrintingCoroutine);
        AutomaticPrintingCoroutine = null;
    }

    IEnumerator Coroutine_AutomaticPrinting()
    {
        while (true)
        {
            // Don't start another poll while one is already running.
            if (!IsPollingOrders)
            {
                yield return StartCoroutine
                (
                    Coroutine_CheckForNewOrders()
                );
            }

            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator Coroutine_CheckForNewOrders()
    {
        //Debug.Log("Inside Coroutine_CheckForNewOrders()");

        IsPollingOrders = true;

        try
        {
            bool Success = false;

            yield return StartCoroutine
            (
                GetPendingOrders
                (
                    Script1.StationeryInfoObject.StationeryID,
                    Result => Success = Result
                )
            );

            if (!Success)
            {
                Scene3ManagerScript.DisableAllPanels();

                Scene3ManagerScript.MessageText.text = "Something went wrong, restart the app or try again later.";
                Scene3ManagerScript.EnableMessagePanel();

                yield break;
            }

            //Important !!!
            foreach (OrderSummary Order in ConfirmedOrders.Orders)
            {
                if (PrintManager.Instance.IsOrderAlreadyHandled(Order.OrderID))
                {
                    continue;
                }

                PrintManager.Instance.BeginProcessingOrder(Order.OrderID);

                //StartPrintJob(Order);
                StartCoroutine(ProcessOrder(Order));
            }
        }
        finally
        {
            IsPollingOrders = false;

            RefreshUI();
            Scene3ManagerScript.EnableHomePagePanel();
        }
    }






    //Refreshing HomePage UI
    public void RefreshUI()
    {
        Debug.Log("RefreshUI");

        StartCoroutine(Coroutine_RefreshUI());
    }
    public IEnumerator Coroutine_RefreshUI()
    {
        //This is to make sure that UI is not refreshed again while UI refreshing process is already going on.
        IsRefreshingHomePage = true;


        //Debug.Log("Coroutine_RefreshUI()");
        HomePageScript.CurrentDate.text = DateTime.Now.ToString("dd-MM-yyyy");


        //=====================================================
        //To count the total number of each type of orders.
        //=====================================================
        bool OrdersCountSuccess = false;

        yield return StartCoroutine
        (
            GetOrdersCount
            (
                Result => OrdersCountSuccess = Result
            )
        );

        if (!OrdersCountSuccess)
        {
            RefreshFailed();

            IsRefreshingHomePage = false;

            yield break;
        }



        //=====================================================
        //To calculate stationery's revenue for today.
        //=====================================================
        bool RevenueSuccess = false;

        yield return StartCoroutine
        (
            GetTodayStationeryRevenue
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => RevenueSuccess = Result
            )
        );

        if (!RevenueSuccess)
        {
            RefreshFailed();

            IsRefreshingHomePage = false;

            yield break;
        }



        //To update number of working printers, based on their past activity.
        HomePageScript.UpdateNumberOfWorkingPrinters();

        //To inform users if 0 printers are conntected.
        HomePageScript.UpdateNumberOfConnectedPrinters();

        //This function checks if there exists any printer with less than 10% of PagesLeft out of PageCapacity in the tray.
        //If yes, it SetsActive an exclamation mark over the Queue button on HomePage.
        HomePageScript.Check_PrintersFor_PagesLeft();


        IsRefreshingHomePage = false;
    }
    void RefreshFailed()
    {
        Scene3ManagerScript.DisableAllPanels();

        Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
        Scene3ManagerScript.EnableMessagePanel();
    }











    //=====================================
    //Printing process ( Updated )
    //=====================================


    //Required class definitions =

    private const string ServerURL = "http://localhost:3000";

    [System.Serializable]
    public class ValidatePrintRequest
    {
        public int OrderID;
        public int StationeryID;
    }

    [System.Serializable]
    public class ValidatePrintResponse
    {
        public bool Success;
        public string Message;
        public int TotalPrints;
    }

    [System.Serializable]
    public class StartPrintRequest
    {
        public int OrderID;
        public int StationeryID;
        public string PrinterName;
    }

    [System.Serializable]
    public class StartPrintResponse
    {
        public bool Success;
        public string Message;
    }













    //Called by Print button in OrderStatus panel.
    //PrintButton -> ProcessOrder -> ValidateOrder() + DownloadPDFS() + CreatePrintJob()
    public IEnumerator ProcessOrder(OrderSummary Order)
    {
        bool Success = false;


        //==================================================
        // 1. VALIDATE ORDER
        //==================================================

        yield return ValidateOrder
        (
            Order,
            Result => Success = Result
        );

        if (!Success) yield break;


        //==================================================
        // 2. DOWNLOAD PDFs
        //==================================================

        yield return DownloadPDFs
        (
            Order,
            Result => Success = Result
        );

        if (!Success) yield break;


        //==================================================
        // 3. CREATE + QUEUE PRINT JOB
        //==================================================

        yield return CreatePrintJob
        (
            Order,
            Result => Success = Result
        );

        if (!Success) yield break;


        //==================================================
        // PRINTING IS NOW HANDLED BY PrinterWorker
        //==================================================

        Debug.Log
        (
            "OrderID = " +
            Order.OrderID +
            " has successfully entered the printing queue."
        );
    }









    IEnumerator ValidateOrder
    (
        OrderSummary Order,
        System.Action<bool> OnComplete
    )
    {
        ValidatePrintRequest Request = new ValidatePrintRequest()
        {
            OrderID = Order.OrderID,
            StationeryID = Script1.StationeryInfoObject.StationeryID
        };

        string JsonData = JsonUtility.ToJson(Request);


        using
        (
            UnityWebRequest RequestObject =
                new UnityWebRequest
                (
                    ServerURL + "/printOrder/validate",
                    "POST"
                )
        )
        {
            byte[] Body = Encoding.UTF8.GetBytes(JsonData);

            RequestObject.uploadHandler =
                new UploadHandlerRaw(Body);

            RequestObject.downloadHandler =
                new DownloadHandlerBuffer();

            RequestObject.SetRequestHeader
            (
                "Content-Type",
                "application/json"
            );


            yield return RequestObject.SendWebRequest();


            //==================================================
            // REQUEST FAILED
            //==================================================

            if
            (
                RequestObject.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogError(RequestObject.responseCode);
                Debug.LogError(RequestObject.error);
                Debug.LogError(RequestObject.downloadHandler.text);


                //Scene3ManagerScript.DisableInvisiblePanel();

                PrintManager.Instance.EndProcessingOrder
                (
                    Order.OrderID
                );

                //The order now must go to the failed order list, as it's vaildation failed.
                SetOrderAsFailed(Order.OrderID, (GameObject)null);


                OnComplete(false);

                yield break;
            }


            //==================================================
            // READ RESPONSE
            //==================================================

            ValidatePrintResponse Response =
                JsonUtility.FromJson<ValidatePrintResponse>
                (
                    RequestObject.downloadHandler.text
                );


            //==================================================
            // VALIDATION FAILED
            //==================================================

            if (!Response.Success)
            {
                Scene3ManagerScript.MessageText.text = Response.Message;
                Scene3ManagerScript.EnableMessagePanel();

                //Scene3ManagerScript.DisableInvisiblePanel();

                PrintManager.Instance.EndProcessingOrder
                (
                    Order.OrderID
                );

                //The order now must go to the failed order list, as it's validation failed.
                SetOrderAsFailed(Order.OrderID, (GameObject)null);

                OnComplete(false);

                yield break;
            }


            //==================================================
            // SUCCESS
            //==================================================

            Debug.Log
            (
                "All Prints of OrderID = " +
                Order.OrderID +
                " have been validated."
            );

            OnComplete(true);
        }
    }






    IEnumerator DownloadPDFs
    (
        OrderSummary Order,
        System.Action<bool> OnComplete
    )
    {
        bool Success = false;


        yield return StartCoroutine
        (
            Coroutine_DownloadAllPDFs
            (
                Order,
                Result => Success = Result
            )
        );


        //==================================================
        // DOWNLOAD FAILED
        //==================================================

        if (!Success)
        {
            Scene3ManagerScript.MessageText.text = "PrintPal couldn't download all PDFs.";
            Scene3ManagerScript.EnableMessagePanel();

            //Scene3ManagerScript.DisableInvisiblePanel();

            PrintManager.Instance.EndProcessingOrder
            (
                Order.OrderID
            );

            //The order now must go to the failed order list, as it's downloading process failed.
            SetOrderAsFailed(Order.OrderID, (GameObject)null);

            OnComplete(false);

            yield break;
        }


        //==================================================
        // SUCCESS
        //==================================================

        Debug.Log
        (
            "Downloaded all PDFs of OrderID = " +
            Order.OrderID +
            " successfully."
        );

        OnComplete(true);
    }

    //This downloads PDFs of all prints of an order which is to be Printed / Opened.
    public IEnumerator Coroutine_DownloadAllPDFs(OrderSummary Order, System.Action<bool> OnComplete)
    {
        foreach (PrintDetails Print in Order.Prints)
        {
            bool Success = false;
            string PDFPath = null;

            yield return StartCoroutine
            (
                Coroutine_DownloadPDF
                (
                    Print.PrintID,
                    (Result, Path) =>
                    {
                        Success = Result;
                        PDFPath = Path;
                    }
                )
            );

            if (!Success)
            {
                OnComplete(false);
                yield break;
            }

            Print.LocalPDFPath = PDFPath;
        }

        OnComplete(true);
    }
    IEnumerator Coroutine_DownloadPDF(int PrintID, System.Action<bool, string> OnComplete)
    {
        //This is important.
        //Here, we are combining Unity's path + PrintID + .pdf, as currently, we are downloading and saving
        //all PDFs in the same folder as that of the Unity project.
        string SavePath = Path.Combine
        (
            Application.persistentDataPath,
            PrintID + ".pdf"
        );

        //If the PDF of the same name is already downloaded.
        if (File.Exists(SavePath))
        {
            Debug.Log("PDF already exists.");

            OnComplete(true, SavePath);
            yield break;
        }

        using (UnityWebRequest Request = UnityWebRequest.Get
        (
            "http://localhost:3000/download/" + PrintID
        ))
        {
            Request.downloadHandler = new DownloadHandlerFile(SavePath);

            yield return Request.SendWebRequest();

            if (Request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(Request.error);

                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                OnComplete(false, null);
                yield break;
            }

            Debug.Log("Saved to: " + SavePath);
            Debug.Log(new FileInfo(SavePath).Length);

            OnComplete(true, SavePath);
        }
    }







    IEnumerator CreatePrintJob
    (
        OrderSummary Order,
        System.Action<bool> OnComplete
    )
    {
        PrintJob Job = PrintManager.Instance.CreatePrintJob(Order);


        //==================================================
        // JOB CREATION FAILED
        //==================================================

        //This happens if a LocalPDFPath of any print of an order is empty / NULL.
        if (Job == null)
        {
            //Scene3ManagerScript.DisableInvisiblePanel();

            PrintManager.Instance.EndProcessingOrder
            (
                Order.OrderID
            );

            //The order now must go to the failed order list, as one of it's PDF could not be found.
            SetOrderAsFailed(Order.OrderID, (GameObject)null);


            OnComplete(false);

            yield break;
        }


        Debug.Log
        (
            "Job for OrderID = " +
            Order.OrderID +
            " has been created."
        );







        //==================================================
        // QUEUE JOB
        //==================================================

        bool Queued = PrintManager.Instance.QueuePrintJob(Job);



        //Two reasons for a Job not being queued =
        //1. The Job in QueuePrintJob ( Job ) is null.
        //2. No compatible printer was found.

        if (!Queued)
        {
            Debug.Log
            (
                "Order could not be Queued."
            );


            //==================================================
            // JOB MAY ALREADY HAVE BEEN QUEUED ELSEWHERE
            //==================================================

            if
            (
                !PrintManager.Instance.AnyWorkerHasOrder
                (
                    Order.OrderID
                )
            )
            {
                PrintManager.Instance.EndProcessingOrder
                (
                    Order.OrderID
                );


                //The order now must go to the failed order list, as it's Queueing process failed.
                SetOrderAsFailed(Order.OrderID, (GameObject)null);


                //Scene3ManagerScript.DisableInvisiblePanel();


                OnComplete(false);

                yield break;
            }


            // Existing worker already owns the order.
            // Do NOT end processing here.
        }


        //==================================================
        // SUCCESS
        //==================================================

        //Printing the job details =
        Job.PrintJobDetails();

        OnComplete(true);
    }










    //This function is called by PrintWorker script when an Order is succesfully printed.
    public IEnumerator CompletePrintedOrder(int OrderID)
    {
        yield return StartCoroutine
        (
            Coroutine_CompletePrintedOrder(OrderID)
        );
    }
    IEnumerator Coroutine_CompletePrintedOrder(int OrderID)
    {
        ValidatePrintRequest Request = new ValidatePrintRequest()
        {
            OrderID = OrderID,
            StationeryID = Script1.StationeryInfoObject.StationeryID
        };

        string JsonData = JsonUtility.ToJson(Request);

        UnityWebRequest RequestObject = new UnityWebRequest
        (
            ServerURL + "/printOrder/complete",
            "POST"
        );

        byte[] Body = Encoding.UTF8.GetBytes(JsonData);

        RequestObject.uploadHandler = new UploadHandlerRaw(Body);
        RequestObject.downloadHandler = new DownloadHandlerBuffer();
        RequestObject.SetRequestHeader("Content-Type", "application/json");

        yield return RequestObject.SendWebRequest();

        if (RequestObject.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(RequestObject.responseCode);
            Debug.LogError(RequestObject.error);
            Debug.LogError(RequestObject.downloadHandler.text);

            Scene3ManagerScript.DisableInvisiblePanel();

            PrintManager.Instance.EndProcessingOrder(OrderID);

            yield break;
        }

        StartPrintResponse Response =
            JsonUtility.FromJson<StartPrintResponse>
            (
                RequestObject.downloadHandler.text
            );

        if (!Response.Success)
        {
            Debug.LogError(Response.Message);

            Scene3ManagerScript.DisableInvisiblePanel();

            PrintManager.Instance.EndProcessingOrder(OrderID);

            yield break;
        }

        Debug.Log
        (
            $"Order {OrderID} marked as completed."
        );


        //--------------------------------------------------
        // Refresh Pending Orders
        //--------------------------------------------------

        bool PendingSuccess = false;

        yield return StartCoroutine
        (
            GetPendingOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => PendingSuccess = Result
            )
        );



        //Refreshing Completed Orders and Pending Orders ( I believe there is no need for this to happen silently... could be wrong. 
        //Update - was wrong lmao... do not touch this.

        //--------------------------------------------------
        // Refresh Completed Orders
        //--------------------------------------------------

        bool CompletedSuccess = false;

        yield return StartCoroutine
        (
            GetCompletedOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => CompletedSuccess = Result
            )
        );

        if (!PendingSuccess || !CompletedSuccess)
        {
            Debug.LogWarning
            (
                "Failed to refresh order lists."
            );
        }


        PrintManager.Instance.EndProcessingOrder(OrderID);

        QueueManagerScript.RefreshQueuedOrders();

        Scene3ManagerScript.DisableInvisiblePanel();
    }















    //===========================================================================================================================
    //flow =
    //PrintWorker.Coroutine_PrintDocument() -> PagesLeftUpdationProcess() -> PagesLeftUpdationProcess_Coroutine() ->
    //UpdatePagesUsed() + GetPagesLeft() + PrintWorker.UpdatePagesLeft_Locally()
    //===========================================================================================================================


    [System.Serializable]
    public class PagesUsedRequest
    {
        public int PagesUsed;
    }

    [System.Serializable]
    public class PagesLeftResponse
    {
        public bool Success;
        public int PagesLeft;
        public string Message;
    }


    //=======================================================================================
    //UpdatePageLeft() -> UpdatePagesLeftCoroutine() -> UpdatePagesUsed() + GetPagesLeft().
    //
    // NOTE: Worker is no longer stored on a shared field here. With multiple printers
    // running in parallel (see PrintManager.Workers), two printers finishing around the
    // same time would overwrite each other's Worker reference before either coroutine
    // got to Worker.UpdatePagesLeft_Locally(), silently applying the wrong printer's
    // pages-left update to the wrong worker. Threading Worker through as a parameter
    // instead means each in-flight call keeps its own reference, regardless of how many
    // printers complete concurrently.
    //=======================================================================================
    public void PagesUsedUpdationProcess(int StationeryPrinterID, int PagesUsed, PrinterWorker Worker)
    {
        StartCoroutine
        (
            PagesUsedUpdationProcess_Coroutine
            (
                StationeryPrinterID,
                PagesUsed,
                Worker
            )
        );
    }

    IEnumerator PagesUsedUpdationProcess_Coroutine
    (
        int StationeryPrinterID,
        int PagesUsed,
        PrinterWorker Worker
    )
    {
        bool Success = false;

        yield return StartCoroutine(UpdatePagesUsed(StationeryPrinterID, PagesUsed, Result => Success = Result));

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Pages could not be updated.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        int PagesLeft = -1;


        yield return StartCoroutine(GetPagesLeft(StationeryPrinterID, Result => PagesLeft = Result));

        if (PagesLeft == -1)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Pages were updated, but their value could not be retrieved.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }


        Worker.UpdatePagesLeft_Locally(PagesLeft);
    }


    //======================================================
    // Update Pages Used
    //======================================================

    public IEnumerator UpdatePagesUsed
    (
        int StationeryPrinterID,
        int PagesUsed,
        System.Action<bool> OnComplete
    )
    {
        string json = JsonUtility.ToJson(new PagesUsedRequest
        {
            PagesUsed = PagesUsed
        });


        using (UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/stationery/printer/pages-used/" + StationeryPrinterID,
            "PUT")
        )
        {
            //Debug.Log("StationeryPrinterID = " + StationeryPrinterID);
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);



            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();


            request.SetRequestHeader
            (
                "Content-Type",
                "application/json"
            );


            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Pages used updated successfully.");

                OnComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("Failed to update pages used.");
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);

                OnComplete?.Invoke(false);
            }
        }
    }


    //======================================================
    // Get Pages Left
    //======================================================

    public IEnumerator GetPagesLeft
    (
        int StationeryPrinterID,
        System.Action<int> OnComplete
    )
    {
        using (UnityWebRequest request = UnityWebRequest.Get(
            "http://localhost:3000/stationery/printer/pages-left/" + StationeryPrinterID))
        {

            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.Success)
            {

                PagesLeftResponse response =
                    JsonUtility.FromJson<PagesLeftResponse>(
                        request.downloadHandler.text
                    );

                if (response.Success)
                {
                    Debug.Log("Pages left: " + response.PagesLeft);

                    OnComplete?.Invoke(response.PagesLeft);
                }
                else
                {

                    Debug.LogError("Failed to get pages left.");
                    Debug.LogError(response.Message);

                    OnComplete?.Invoke(-1);
                }
            }
            else
            {
                Debug.LogError("Failed to get pages left.");
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);

                OnComplete?.Invoke(-1);
            }
        }
    }








    ///=======================================
    //Refil Pages of a specific Printer.
    //========================================


    [System.Serializable]
    public class RefillPrinterResponse
    {
        public bool Success;
        public string Message;
    }




    // Refill Printer Pages coroutine =
    public IEnumerator RefillPrinterPages
    (
        int StationeryPrinterID,
        System.Action<bool> OnComplete
    )
    {
        using (UnityWebRequest request = UnityWebRequest.Put(
            "http://localhost:3000/stationery/printer/refill/" + StationeryPrinterID,
            ""))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RefillPrinterResponse response =
                    JsonUtility.FromJson<RefillPrinterResponse>(
                        request.downloadHandler.text
                    );

                if (response.Success)
                {
                    Debug.Log("Printer pages refilled successfully.");

                    OnComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError(response.Message);

                    OnComplete?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError("Failed to refill printer pages.");
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);

                OnComplete?.Invoke(false);
            }
        }
    }








    //Temp =

    [System.Serializable]
    public class PrinterCapabilitiesResponse
    {
        public bool Success;
        public string Message;
        public PrinterCapabilityInfo[] Printers;
    }

    [System.Serializable]
    public class PrinterCapabilityInfo
    {
        public string Name;
        public string DriverName;
        public string PortName;
        public int AveragePagesPerMinute;

        public bool SupportsColorPrinting;
        public bool SupportsDoubleSidedPrinting;
    }

    public IEnumerator GetPrinterCapabilities
    (
        System.Action<PrinterCapabilitiesResponse> OnComplete
    )
    {
        using (UnityWebRequest request = UnityWebRequest.Get
        (
            "http://localhost:3000/printers/capabilities"
        ))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Request failed: " + request.error);
                Debug.LogError(request.downloadHandler.text);

                OnComplete?.Invoke(null);
                yield break;
            }

            PrinterCapabilitiesResponse response =
                JsonUtility.FromJson<PrinterCapabilitiesResponse>
                (
                    request.downloadHandler.text
                );

            if (response == null ||
                !response.Success ||
                response.Printers == null)
            {
                Debug.LogError("Invalid printer response.");
                OnComplete?.Invoke(null);
                yield break;
            }

            foreach (PrinterCapabilityInfo printer in response.Printers)
            {
                printer.AveragePagesPerMinute =
                    WindowsPrinterCapabilities.GetPPM(
                        printer.Name, printer.PortName
                    );

                printer.SupportsColorPrinting =
                    WindowsPrinterCapabilities.SupportsColor(
                        printer.Name, printer.PortName
                    );

                printer.SupportsDoubleSidedPrinting =
                    WindowsPrinterCapabilities.SupportsDuplex(
                        printer.Name, printer.PortName
                    );
            }

            OnComplete?.Invoke(response);
        }
    }












    public static class WindowsPrinterCapabilities
    {
        private const int DC_DUPLEX = 7;
        private const int DC_COLORDEVICE = 32;
        private const int DC_PRINTRATEPPM = 31;

        [DllImport("winspool.drv", CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern int DeviceCapabilities(
            string pDevice,
            string pPort,
            short fwCapability,
            IntPtr pOutput,
            IntPtr pDevMode
        );

        public static int GetPPM(string printerName, string portName)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                return DeviceCapabilities(
                    printerName, portName,
                    DC_PRINTRATEPPM, IntPtr.Zero, IntPtr.Zero
                );
            }
            catch (Exception)
            {
                return 0;
            }
#else
        return 0;
#endif
        }

        public static bool SupportsDuplex(string printerName, string portName)
        {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                return DeviceCapabilities(
                    printerName, portName,
                    DC_DUPLEX, IntPtr.Zero, IntPtr.Zero
                ) == 1;
            }
            catch (Exception)
            {
                return false;
            }
        #else
        return false;
        #endif
        }

        public static bool SupportsColor(string printerName, string portName)
        {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                return DeviceCapabilities(
                    printerName, portName,
                    DC_COLORDEVICE, IntPtr.Zero, IntPtr.Zero
                ) == 1;
            }
            catch (Exception)
            {
                return false;
            }
        #else
        return false;
        #endif
        }
    }
}