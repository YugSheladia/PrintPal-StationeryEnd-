using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

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



//
[System.Serializable]
public class UserInfo
{
    public int      UserID;
    public string   PhoneNumber;
    public string   Name;
    public string   UPINumber;
    public string   Email;
    public string   Username;
}

[System.Serializable]
public class UsernameResponse
{
    public bool Success;
    public string Message;
    public UserInfo User;
}




//==================================================
// Main Class
//==================================================

public class Scene3Backend : MonoBehaviour
{
    [SerializeField] HomePage HomePageScript;


    //Wrapper objects creation.
    public OrderSummaryWrapper ConfirmedOrders = new();

    public PrintSummaryWrapper ConfirmedPrints = new();

    public PrintSettingsSummaryWrapper ConfirmedPrintSettings = new();

    public PrintBillsSummaryWrapper ConfirmedPrintBills = new();



    //To get orders, beloing to a stationery, of a specific status.
    public IEnumerator GetPendingOrders(int StationeryID)
    {
        Debug.Log("Downloading Pending Orders...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];
    

        UnityWebRequest request = UnityWebRequest.Get(
                                                         $"http://localhost:3000/orders/pending?StationeryID={StationeryID}"
                                                     );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Downloaded {ConfirmedOrders.Orders.Length} Pending Orders.");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Downloading Pending Orders.");
    }
    public IEnumerator GetCompletedOrders(int StationeryID)
    {
        Debug.Log("Downloading Completed Orders...");

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request =
            UnityWebRequest.Get(
                $"http://localhost:3000/orders/completed?StationeryID={StationeryID}"
            );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Downloaded {ConfirmedOrders.Orders.Length} Completed Orders.");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Downloading Completed Orders.");
    }
    public IEnumerator GetCollectedOrders(int StationeryID)
    {
        Debug.Log("Downloading Collected Orders...");

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request =
            UnityWebRequest.Get(
                $"http://localhost:3000/orders/collected?StationeryID={StationeryID}"
            );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Downloaded {ConfirmedOrders.Orders.Length} Collected Orders.");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Downloading Collected Orders.");
    }



    //Get all Archived Orders.
    public IEnumerator GetArchivedOrders(int StationeryID)
    {
        Debug.Log("Downloading Archived Orders...");

        ConfirmedOrders.Orders                  = new OrderSummary[0];
        ConfirmedPrints.Prints                  = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings    = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills          = new PrintBillsSummary[0];

        UnityWebRequest request =
            UnityWebRequest.Get(
                $"http://localhost:3000/stationeryOrders/archived?StationeryID={StationeryID}"
            );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Orders Download Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Downloaded {ConfirmedOrders.Orders.Length} Archived Orders.");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Downloading Deleted Orders.");
    }




   //To get prints, printBills, and printSettings of Orders of a specific status
   //( Called by functions above, e.g. - GetPendingOrders, GetCompletedOrders ).
    IEnumerator GetPrints()
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

                continue;
            }

#if UNITY_EDITOR
            Debug.Log(request.downloadHandler.text);
#endif

            PrintSummaryWrapper Response =
                JsonUtility.FromJson<PrintSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null)
            {
                Debug.LogError
                (
                    "Failed Parsing Prints JSON For Order "
                    + Order.OrderID
                );

                continue;
            }

            if (!Response.Success)
            {
                Debug.LogError
                (
                    "Server Error : "
                    + Response.Message
                );

                continue;
            }

            if (Response.Prints != null)
            {
                AllPrints.AddRange(Response.Prints);
            }
        }

        ConfirmedPrints.Prints = AllPrints.ToArray();

        Debug.Log
        (
            "Downloaded "
            + ConfirmedPrints.Prints.Length
            + " Prints."
        );

        yield return StartCoroutine(GetPrintSettings());

        Debug.Log("Finished Downloading Prints.");
    }
    IEnumerator GetPrintBills()
    {
        Debug.Log("Downloading Print Bills...");

        List<PrintBillsSummary> AllPrintBills = new();

        foreach (PrintSummary Print in ConfirmedPrints.Prints)
        {
            UnityWebRequest request =
                UnityWebRequest.Get
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

                continue;
            }

#if UNITY_EDITOR
            Debug.Log(request.downloadHandler.text);
#endif

            PrintBillsSummaryWrapper Response =
                JsonUtility.FromJson<PrintBillsSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null)
            {
                Debug.LogError
                (
                    "Failed Parsing Print Bill JSON For Print "
                    + Print.PrintID
                );

                continue;
            }

            if (!Response.Success)
            {
                Debug.LogError
                (
                    "Server Error : "
                    + Response.Message
                );

                continue;
            }

            if (Response.PrintBills != null)
            {
                AllPrintBills.AddRange(Response.PrintBills);
            }
        }

        ConfirmedPrintBills.PrintBills =
            AllPrintBills.ToArray();

        Debug.Log
        (
            "Downloaded "
            + ConfirmedPrintBills.PrintBills.Length
            + " Print Bills."
        );

        BuildPrintDetails();

        Debug.Log("Finished Downloading Everything.");
    }
    IEnumerator GetPrintSettings()
    {
        Debug.Log("Downloading Print Settings...");

        List<PrintSettingsSummary> AllPrintSettings = new();

        foreach (PrintSummary Print in ConfirmedPrints.Prints)
        {
            UnityWebRequest request =
                UnityWebRequest.Get
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

                continue;
            }

#if UNITY_EDITOR
            Debug.Log(request.downloadHandler.text);
#endif

            PrintSettingsSummaryWrapper Response =
                JsonUtility.FromJson<PrintSettingsSummaryWrapper>
                (
                    request.downloadHandler.text
                );

            if (Response == null)
            {
                Debug.LogError
                (
                    "Failed Parsing Print Settings JSON For Print "
                    + Print.PrintID
                );

                continue;
            }

            if (!Response.Success)
            {
                Debug.LogError
                (
                    "Server Error : "
                    + Response.Message
                );

                continue;
            }

            if (Response.PrintSettings != null)
            {
                AllPrintSettings.AddRange(Response.PrintSettings);
            }
        }

        ConfirmedPrintSettings.PrintSettings =
            AllPrintSettings.ToArray();

        Debug.Log
        (
            "Downloaded "
            + ConfirmedPrintSettings.PrintSettings.Length
            + " Print Settings."
        );

        yield return StartCoroutine(GetPrintBills());

        Debug.Log("Finished Downloading Print Settings.");
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



    //To get name of the User who issued the Order.
    public void GetUsernameFromServer(int UserID, PendingOrderStatus PendingOrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, PendingOrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, PendingOrderStatus PendingOrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Debug.Log("Username: " + response.User.Username);

                PendingOrderStatusScript.UsernameString = response.User.Name;
                PendingOrderStatusScript.UpdateUI();
            }
            else
            {
                Debug.LogWarning("User not found.");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void GetUsernameFromServer(int UserID, RTCOrderStatus RTCOrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, RTCOrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, RTCOrderStatus RTCOrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Debug.Log("Username: " + response.User.Username);

                RTCOrderStatusScript.UsernameString = response.User.Name;
                RTCOrderStatusScript.UpdateUI();
            }
            else
            {
                Debug.LogWarning("User not found.");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void GetUsernameFromServer(int UserID, CollectedOrderStatus CollectedOrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, CollectedOrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, CollectedOrderStatus CollectedOrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Debug.Log("Username: " + response.User.Username);

                CollectedOrderStatusScript.UsernameString = response.User.Name;
                CollectedOrderStatusScript.UpdateUI();
            }
            else
            {
                Debug.LogWarning("User not found.");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void GetUsernameFromServer(int UserID, SearchedOrderStatus SearchedOrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, SearchedOrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, SearchedOrderStatus SearchedOrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Debug.Log("Username: " + response.User.Username);

                SearchedOrderStatusScript.UsernameString = response.User.Name;
                SearchedOrderStatusScript.UpdateUI();
            }
            else
            {
                Debug.LogWarning("User not found.");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void GetUsernameFromServer(int UserID, ArchivedOrderStatus ArchivedOrderStatusScript)
    {
        StartCoroutine(GetUsername(UserID, ArchivedOrderStatusScript));
    }
    public IEnumerator GetUsername(int UserID, ArchivedOrderStatus ArchivedOrderStatusScript)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/user/" + UserID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Server Response:\n" + json);

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(json);

            if (response.Success)
            {
                Debug.Log("Username: " + response.User.Username);

                ArchivedOrderStatusScript.UsernameString = response.User.Name;
                ArchivedOrderStatusScript.UpdateUI();
            }
            else
            {
                Debug.LogWarning("User not found.");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }




    //To set Order's status.
    [System.Serializable]
    public class OrderIDRequest
    {
        public int OrderID;
    }
    public void SetOrderAsPending(int OrderID)
    {
        StartCoroutine(Coroutine_SetOrderAsPending(OrderID));
    }
    IEnumerator Coroutine_SetOrderAsPending(int OrderID)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/order/pending",
                                                        "POST"
                                                      );

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
                                    "Content-Type", 
                                    "application/json"
                                );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Order marked as Pending.");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void SetOrderAsCompleted(int OrderID)
    {
        StartCoroutine(Coroutine_SetOrderAsCompleted(OrderID));
    }
    IEnumerator Coroutine_SetOrderAsCompleted(int OrderID)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/order/completed",
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
            Debug.Log("Order marked as Completed.");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void SetOrderAsCollected(int OrderID)
    {
        StartCoroutine(Coroutine_SetOrderAsCollected(OrderID));
    }
    IEnumerator Coroutine_SetOrderAsCollected(int OrderID)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(
                                                            "http://localhost:3000/order/collected",
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
            Debug.Log("Order marked as Collected.");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }



    //Marking Orders as Archived / Unarchived.
    public void SetOrderAsArchived(int OrderID)
    {
        StartCoroutine(Coroutine_SetOrderAsArchived(OrderID));
    }
    IEnumerator Coroutine_SetOrderAsArchived(int OrderID)
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
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    public void SetOrderAsUnarchived(int OrderID)
    {
        StartCoroutine(Coroutine_SetOrderAsUnarchived(OrderID));
    }
    IEnumerator Coroutine_SetOrderAsUnarchived(int OrderID)
    {
        OrderIDRequest requestBody = new();
        requestBody.OrderID = OrderID;

        string json = JsonUtility.ToJson(requestBody);

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(
                                                            "http://localhost:3000/order/stationery-unarchive",
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
            Debug.Log("Order marked as Unarchived.");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }




    //Search functionality -
    public IEnumerator SearchByOrderIDCoroutine(int StationeryID, int OrderID)
    {
        Debug.Log("Searching Order...");

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get(
            $"http://localhost:3000/SearchByOrderID?StationeryID={StationeryID}&OrderID={OrderID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Order Search Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Order JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Found {ConfirmedOrders.Orders.Length} Order(s).");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Searching Order.");
    }
    public IEnumerator SearchByUserIDCoroutine(int StationeryID, int UserID)
    {
        Debug.Log("Searching User Orders...");

        ConfirmedOrders.Orders = new OrderSummary[0];
        ConfirmedPrints.Prints = new PrintSummary[0];
        ConfirmedPrintSettings.PrintSettings = new PrintSettingsSummary[0];
        ConfirmedPrintBills.PrintBills = new PrintBillsSummary[0];

        UnityWebRequest request = UnityWebRequest.Get(
            $"http://localhost:3000/SearchByUserID?StationeryID={StationeryID}&UserID={UserID}"
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("User Orders Search Failed: " + request.error);
            yield break;
        }

#if UNITY_EDITOR
        Debug.Log(request.downloadHandler.text);
#endif

        OrderSummaryWrapper response =
            JsonUtility.FromJson<OrderSummaryWrapper>(request.downloadHandler.text);

        if (response == null)
        {
            Debug.LogError("Orders JSON Parsing Failed.");
            yield break;
        }

        if (!response.Success)
        {
            Debug.LogError("Server Error: " + response.Message);
            yield break;
        }

        ConfirmedOrders.Orders =
            response.Orders ?? new OrderSummary[0];

        Debug.Log($"Found {ConfirmedOrders.Orders.Length} Order(s).");

        yield return StartCoroutine(GetPrints());

        Debug.Log("Finished Searching User Orders.");
    }



    //Order count -
    [System.Serializable]
    public class OrderCountResponse
    {
        public bool Success;
        public string Message;

        public int TotalOrders;
    }
    public void GetOrdersCount()
    {
        StartCoroutine(GetOrderCount("pending",     HomePageScript.TotalPendingOrdersText));
        StartCoroutine(GetOrderCount("completed",   HomePageScript.TotalFinishedOrdersText));
        StartCoroutine(GetOrderCount("collected",   HomePageScript.TotalCollectedOrdersText));
    }
    public IEnumerator GetOrderCount(string status, TMP_Text targetText)
    {
        UnityWebRequest request 
            = UnityWebRequest.Get(
                                   $"http://localhost:3000/orders/count/{status}?StationeryID={Script1.StationeryInfoObject.StationeryID}"
                                 );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        OrderCountResponse response =
            JsonUtility.FromJson<OrderCountResponse>(request.downloadHandler.text);

        if (response == null || !response.Success)
        {
            Debug.LogError("Failed to get order count.");
            yield break;
        }

        targetText.text = response.TotalOrders.ToString();
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
    public IEnumerator GetTodayStationeryRevenue(int StationeryID)
    {
        UnityWebRequest request = UnityWebRequest.Get(
                                                            $"http://localhost:3000/analytics/stationery/todayEarnings?StationeryID={StationeryID}"
                                                     );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get today's revenue: " + request.error);
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

        Debug.Log("Today's Revenue: " + response.Amount);

        HomePageScript.RevenueToday.text = response.Amount.ToString("F2");
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



    public void DownloadOrderPrintsFromServer(OrderSummary Order)
    {
        foreach (PrintDetails Print in Order.Prints)
        {
            StartCoroutine(Coroutine_DownloadPDF(Print.PrintID));
        }
    }
    IEnumerator Coroutine_DownloadPDF(int PrintID)
    {
        string savePath = Path.Combine(
            Application.persistentDataPath,
            PrintID + ".pdf"
        );

        // If already downloaded, just open it.
        if (File.Exists(savePath))
        {
            Debug.Log("PDF already exists. Opening...");
            Application.OpenURL("file://" + savePath);
            yield break;
        }

        using (UnityWebRequest request =
            UnityWebRequest.Get("http://localhost:3000/download/" + PrintID))
        {
            request.downloadHandler = new DownloadHandlerFile(savePath);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(new FileInfo(savePath).Length);

                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Saved to: " + savePath);

                Debug.Log(new FileInfo(savePath).Length);

                Application.OpenURL("file://" + savePath);
            }
        }
    }
}

