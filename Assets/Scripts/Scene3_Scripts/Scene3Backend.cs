using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using static Scene3Backend;



//=========================================================================
//To recieve EVERY INFO possible about a order issued.
//=========================================================================

// ORDERS TABLE
[System.Serializable]
public class OrderSummary
{
    public int OrderID;
    public int UserID;
    public int StationeryID;

    public int OrderOTP;

    public int ApproximateTimeLeft;

    public float FinalAmount;

    public int Status;
    public int ArchivedByStationery;

    public string CreatedAt;
    public string CompletedAt;
    public string CollectedAt;
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

    public bool failedToDownloadPrint           = false;
    public bool failedToDownloadPrintSettings   = false;
    public bool failedToDownloadPrintBills      = false;

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
    [SerializeField] HomePage       HomePageScript;
    [SerializeField] Scene3Manager  Scene3ManagerScript;



    //Wrapper objects creation.
    public OrderSummaryWrapper ConfirmedOrders = new();

    public PrintSummaryWrapper ConfirmedPrints = new();

    public PrintSettingsSummaryWrapper ConfirmedPrintSettings = new();

    public PrintBillsSummaryWrapper ConfirmedPrintBills = new();







    //To get orders, beloing to a stationery, of a specific status.
    public IEnumerator GetPendingOrders ( int StationeryID, System.Action<bool> OnComplete )
    {
        //Debug.Log("Downloading Pending Orders...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get
        (
            $"http://localhost:3000/orders/pending?StationeryID={StationeryID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);

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

            OnComplete(false);
            yield break;
        }

        if (!Response.Success)
        {
            Debug.LogError("Server Error: " + Response.Message);

            OnComplete(false);
            yield break;
        }

        ConfirmedOrders.Orders =
            Response.Orders ?? new OrderSummary[0];

        Debug.Log
        (
            $"Downloaded {ConfirmedOrders.Orders.Length} Pending Orders."
        );

        bool PrintsSuccess = false;

        yield return StartCoroutine
        (
            GetPrints(Result => PrintsSuccess = Result)
        );

        if (!PrintsSuccess)
        {
            OnComplete(false);
            yield break;
        }

        Debug.Log("Finished Downloading Pending Orders.");

        OnComplete(true);
    }
    public IEnumerator GetCompletedOrders ( int StationeryID, System.Action<bool> OnComplete)
    {
        Debug.Log("Downloading Completed Orders...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];

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
            yield break;
        }

        Debug.Log("Finished Downloading Completed Orders.");

        OnComplete(true);
    }
    public IEnumerator GetCollectedOrders ( int StationeryID, System.Action<bool> OnComplete)
    {
        Debug.Log("Downloading Collected Orders...");

        ConfirmedOrders.Orders                          = new OrderSummary[0];
        ConfirmedPrints.Prints                          = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings            = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills                  = new PrintBillsSummary[0];

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
            yield break;
        }

        Debug.Log("Finished Downloading Collected Orders.");

        OnComplete(true);
    }
    public IEnumerator GetArchivedOrders ( int StationeryID, System.Action<bool> OnComplete )
    {
        Debug.Log("Downloading Archived Orders...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];

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
            yield break;
        }

        Debug.Log("Finished Downloading Archived Orders.");

        OnComplete(true);
    }








    //To get prints, printBills, and printSettings of Orders of a specific status
    //( Called by functions above, e.g. - GetPendingOrders, GetCompletedOrders ).
    IEnumerator GetPrints(System.Action<bool> OnComplete)
    {
        Debug.Log("Downloading Prints...");

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

        Debug.Log("Finished Downloading Prints.");

        OnComplete(true);
    }
    IEnumerator GetPrintSettings(System.Action<bool> OnComplete)
    {
        Debug.Log("Downloading Print Settings...");

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

        Debug.Log("Finished Downloading Print Settings.");

        OnComplete(true);
    }
    IEnumerator GetPrintBills(System.Action<bool> OnComplete)
    {
        Debug.Log("Downloading Print Bills...");

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

        Debug.Log("Finished Downloading Everything.");

        OnComplete(true);
    }







    //Making a PrintDetails object for each prints of each order ( of a specific order status ).

    public Dictionary  <int, PrintSettingsSummary> SettingsDictionary  = new();
    public Dictionary  <int, PrintBillsSummary>    BillsDictionary     = new();
    public Dictionary  <int, List<PrintSummary>>   PrintsDictionary    = new();
    void BuildPrintDetails()
    {
        Debug.Log("Building Print Details...");

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
        }

        Debug.Log("Finished Building Print Details.");
    }







    //Get name of the customer on bill.
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
    public void GetUsernameFromServer(int UserID, OrderStatus OrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, OrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, OrderStatus OrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            //Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                //Debug.Log("Username: " + response.User.Username);

                OrderStatusScript.UsernameString = response.User.Name;
                OrderStatusScript.UpdateUI();
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








    //To set Order's status.
    [System.Serializable]
    public class OrderIDRequest
    {
        public int OrderID;
    }
    public void SetOrderAsPending(int OrderID, GameObject Panel)
    {
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
    public void SetOrderAsUnarchived(int OrderID, GameObject Panel)
    {
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
    void HandleStandaloneStatusChange ( bool Success, GameObject Panel )
    {
        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            return;
        }

        Destroy(Panel);
    }



    public IEnumerator SetOrderAsPending ( int OrderID, System.Action<bool> OnComplete )
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
    public IEnumerator SetOrderAsCompleted ( int OrderID, System.Action<bool> OnComplete )
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
    public IEnumerator SetOrderAsCollected ( int OrderID, System.Action<bool> OnComplete )
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
    public IEnumerator SetOrderAsUnarchived ( int OrderID, System.Action<bool> OnComplete )
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




    public void SetOrderAsArchived(int OrderID, GameObject Panel)
    {
        StartCoroutine(Coroutine_SetOrderAsArchived(OrderID, Panel));
    }
    IEnumerator Coroutine_SetOrderAsArchived(int OrderID, GameObject Panel)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(
                                                            "http://localhost:3000/order/stationery-archive",
                                                            "POST"
                                                      );

        request.uploadHandler = new UploadHandlerRaw(body);

        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
                                    "Content-Type",
                                    "application/json"
                                );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Order marked as Archived.");

            Destroy(Panel);

            Scene3ManagerScript.DisableInvisiblePanel();
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










    //Search functionality -
    public IEnumerator SearchByOrderIDCoroutine ( int StationeryID, int OrderID, System.Action<bool> OnComplete )
    {
        Debug.Log("Searching Order...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];

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
            OnComplete(false);
            yield break;
        }

        Debug.Log("Finished Searching Order.");

        OnComplete(true);
    }
    public IEnumerator SearchByUserIDCoroutine ( int StationeryID, int UserID, System.Action<bool> OnComplete )
    {
        Debug.Log("Searching User Orders...");

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
            OnComplete(false);
            yield break;
        }

        Debug.Log("Finished Searching User Orders.");

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
    public IEnumerator GetOrdersCount ( System.Action<bool> OnComplete )
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
                HomePageScript.TotalFinishedOrdersText,
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

        OnComplete(true);
    }
    public IEnumerator GetOrderCount ( string status, TMP_Text targetText, System.Action<bool> OnComplete )
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

        targetText.text =
            Response.TotalOrders.ToString();

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
    public IEnumerator GetAllTimeStationeryEarnings (int StationeryID)
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
    public IEnumerator GetTodayStationeryRevenue ( int StationeryID, System.Action<bool> OnComplete)
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
        Debug.Log(request.downloadHandler.text);
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

        Debug.Log
        (
            "Today's Revenue: "
            + Response.Amount
        );

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









    public string ServerURL = "http://localhost:3000";

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


    public void StartPrintJob(OrderSummary Order)
    {
        StartCoroutine(Coroutine_CreatePrintJob(Order));
    }
    IEnumerator Coroutine_CreatePrintJob(OrderSummary Order)
    {
        //Validation is to check whether all the info relating to the order, i.e. Prints, PrintSettings, PrintBills, etc all exist.
        ValidatePrintRequest Request = new ValidatePrintRequest()
        {
            OrderID = Order.OrderID,
            StationeryID = Script1.StationeryInfoObject.StationeryID
        };

        string JsonData = JsonUtility.ToJson(Request);

        UnityWebRequest RequestObject = new UnityWebRequest
        (
            ServerURL + "/printOrder/validate",
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
            yield break;
        }

        ValidatePrintResponse Response =
            JsonUtility.FromJson<ValidatePrintResponse>
            (
                RequestObject.downloadHandler.text
            );

        if (!Response.Success)
        {
            Scene3ManagerScript.MessageText.text = Response.Message;

            Scene3ManagerScript.EnableMessagePanel();
            Scene3ManagerScript.DisableInvisiblePanel();

            yield break;
        }

        Debug.Log("All Prints validated.");


        //Downloading all PDFS in the app's file directory ( if they haven't been already ).
        yield return StartCoroutine
        (
            Coroutine_DownloadAllPDFs
            (
                Order,
                Success =>
                {
                    if (!Success)
                    {
                        Scene3ManagerScript.MessageText.text =
                            "PrintPal couldn't download all PDFs.";

                        Scene3ManagerScript.EnableMessagePanel();
                        Scene3ManagerScript.DisableInvisiblePanel();

                        return;
                    }

                    Debug.Log("Downloaded All PDFs.");
                    
                    //Create a PrintJob.
                    PrintJob Job = PrintManager.Instance.CreatePrintJob
                    (
                        Order,
                        PrintManager.Instance.SelectedPrinter
                    );

                    if (Job == null)
                    {
                        Scene3ManagerScript.MessageText.text =
                            "Failed to prepare print job.";

                        Scene3ManagerScript.EnableMessagePanel();
                        Scene3ManagerScript.DisableInvisiblePanel();

                        return;
                    }

                    Debug.Log("Job created.");

                    Job.PrintJobDetails();

                    //Queing the Job.
                    PrintManager.Instance.QueuePrintJob(Job);

                    Debug.Log("Current Queue = ");
                    PrintManager.Instance.Queue.DisplayQueueInfo();
                    
                }
            )
        );
        Scene3ManagerScript.DisableInvisiblePanel();
    }


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
        string SavePath = Path.Combine
        (
            Application.persistentDataPath,
            PrintID + ".pdf"
        );

        // Already downloaded.
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


    public IEnumerator CompletePrintedOrder ( int OrderID )
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

        Scene3ManagerScript.DisableInvisiblePanel();
    }
}

