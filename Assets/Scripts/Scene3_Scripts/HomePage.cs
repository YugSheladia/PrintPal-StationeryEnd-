//namespaces =
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;










public class HomePage : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] OrderStatusManager OrderStatusManagerScript;

    [SerializeField] RevenuePageManager RevenuePageManagerScript;

    [SerializeField] PrintManager PrintManagerScript;

    [SerializeField] PrinterStatusManager PrinterStatusManagerScript;

    [SerializeField] QueueManager QueueManagerScript;





    //*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
    //HomePage UI Elements -
    //*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

    //This displays the name of the stationery in the home screen.
    [SerializeField] TMP_Text StationeryName;





    //The app reads the database and sets the StationeryIsOperational to the correct value.
    bool StationeryIsOperational = false;

    //These are the panels which indicate weather the Stationery is Operational or not.
    [SerializeField] public GameObject Stationery_Operational;
    [SerializeField] public GameObject Stationery_NonOperational;





    //==================================
    //'Search by' UI elements =
    //==================================

    //Dropdown reference.
    [SerializeField] TMP_Dropdown   IDSwitchDropdown;
    
    [SerializeField] TMP_Text       SearchByText;
    [SerializeField] TMP_InputField SearchInputfield;

    //An enum to decide whether to search an order through OrderID or through UserID.
    enum IDToSearchBy
    {
        SearchByOrderID,
        SearchByUserID
    }

    IDToSearchBy IDToSearchByEnum = IDToSearchBy.SearchByOrderID;





    //================
    // Order sutff =
    //================

    //Total orders left in each category, i.e. Pending, Printed, Collected.
    [SerializeField] public TMP_Text TotalPendingOrdersText;
    [SerializeField] public TMP_Text TotalPrintedOrdersText;
    [SerializeField] public TMP_Text TotalCollectedOrdersText;
    [SerializeField] public TMP_Text TotalFailedOrdersText;






    //UI of Revenue panel =
    [SerializeField] public TMP_Text CurrentDate;
    [SerializeField] public TMP_Text RevenueToday;

    







    //==================================
    //UI of PrinterStatus panel =
    //==================================
    [SerializeField] public TMP_Text TotalConnectedPrinters;
    [SerializeField] public TMP_Text TotalWorkingPrinters;

    //This text is displayed if there are 0 printers selected.
    [SerializeField] public GameObject NoPrintersConnected;



    //This is a bool which is used to remember if the PrinterStatusPanelScroll is currently enabled or disabled.
    private bool PrinterStatusPanelActive = false;

    //This is a reference to PrinterStatusPanelScroll itself.
    [SerializeField] private GameObject PrinterStatusPanelScroll;







    //=============================
    //Control panel stuff =
    //=============================

    //A toggle image to indicate whether Automatic Printing is On / Off.
    [SerializeField] public GameObject AutomaticPrinting_ToggleOn;
    [SerializeField] public GameObject AutomaticPrinting_ToggleOff;




    //A simple animation to indicate that the app is working.
    [SerializeField] public GameObject LoadingIcon;


    //PausePrintingBool is different from AutomaticPrinting bool in Script1, because =
    //1. AutomaticPrinting = A preference, i.e. AutomaticPrinting is enabled / disabled by choice.

    //2. PausePrintingBool = AutomaticPrinting must be paused ( not disabled ) when some issue has arrived... say
    //a. No Printers connected
    //b. Invalid print doc 
    //c. etc
    private bool PausePrintingBool = false;


    //An indicator that something went wrong and the Automatic Printing was paused.
    [SerializeField] public GameObject ExclamationButton;


    

    


 









    //Flow = 
    //Scene3Manager.Start() -> Scene3Manager.IntializeHomePagePanel() ->
    //StartHomePage() -> InitializeHomePage() -> AutomaticPrinting()



    //StartHomePage() is called by InitializeHomePagePanel() inside Scene3Manager.
    public void StartHomePage()
    {
        //Make sure that HomePagePanel is active -
        Scene3ManagerScript.EnableHomePagePanel();

        Debug.Log("HomePage.StartHomePage()");

        StationeryName.text = Script1.StationeryInfoObject.StationeryName;
        TotalConnectedPrinters.text = "Connected printers : " + Script1.SelectedPrinters.Count;

        //Updating the Operationality of the Stationery.
        StartCoroutine(SetStationeryOperationalTo(Script1.Stationery_IsOperational_bool));

        StartCoroutine(InitializeHomePage());
    }

    //InitializeHomePage() is called by StartHomePage(), and is different than the InitializseHomePagePanel() in Scene3Manager.
    IEnumerator InitializeHomePage()
    {
        Debug.Log("InitializeHomePage()");

        // Initial refresh
        yield return StartCoroutine
        (
            Scene3BackendScript.Coroutine_RefreshUI()
        );

        AutomaticPrinting();
    }

    //AutomaticPrinting is called by InitialiseHomePage().
    public void AutomaticPrinting()
    {
        Debug.Log("AutomaticPrinting()");

        // Only start polling if Automatic Printing is enabled
        if (Script1.GetAutomaticPrintingValue() && !PausePrintingBool)
        {
            Debug.Log("Automatic Printing = true.");

            Scene3BackendScript.StopHomePageRefreshing();

            Scene3BackendScript.StartAutomaticPrinting();
        }
        else
        {
            Debug.Log("Automatic Printing = false.");

            Scene3BackendScript.StopAutomaticPrinting();

            Scene3BackendScript.StartHomePageRefreshing();
        }
    }













    //==============================================
    //Statioonery Operational button stuff =
    //==============================================

    //Called when a user clicks on Open Stationery button.
    public void Clicked_Stationery_Open()
    {
        if(StationeryIsOperational == true)
        {
            Debug.Log("Stationery is already Open");

            return;
        }

        StartCoroutine(SetStationeryOperationalTo(true));
    }

    //Called when a user clicks on Close Stationery button.
    public void Clicked_Stationery_Close()
    {
        if (StationeryIsOperational == false)
        {
            Debug.Log("Stationery is already Close");

            return;
        }

        StartCoroutine(SetStationeryOperationalTo(false));
    }

    //Calls the SetStationeryOperational() coroutine.
    IEnumerator SetStationeryOperationalTo(bool isOperational)
    {
        if(StationeryIsOperational == isOperational)
        {
            Debug.Log("Stationery is already " + isOperational);
            yield break;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;
        yield return StartCoroutine(Scene3BackendScript.SetStationeryOperational(Script1.StationeryInfoObject.StationeryID, isOperational, Result => Success = Result));

        if (Success)
        {
            Debug.Log("3");
            StationeryIsOperational = isOperational;
            Script1.Stationery_IsOperational_bool = isOperational;

            Update_Stationery_IsOperational_UI();

            Scene3ManagerScript.DisableInvisiblePanel();
        }
        else
        {
            Debug.Log("4");
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, please try again later.";
            Scene3ManagerScript.EnableMessagePanel();
        }
    }

    //Called to Update Stationery Open / Close UI =
    public void Update_Stationery_IsOperational_UI()
    {
        Stationery_Operational.SetActive(Script1.Stationery_IsOperational_bool);
        Stationery_NonOperational.SetActive(!Script1.Stationery_IsOperational_bool);
    }












    //=============================================================
    // HomePage Buttons 
    //=============================================================


    //Order buttons =
    public void ClickedPendingOrdersButton()
    {
        StartCoroutine(ClickedPendingOrdersButtonCoroutine());
    }
    IEnumerator ClickedPendingOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetPendingOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => Success = Result
            )
        );

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "No Pending orders currently exist.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedCompletedOrdersButton()
    {
        StartCoroutine(ClickedCompletedOrdersButtonCoroutine());
    }
    IEnumerator ClickedCompletedOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetCompletedOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => Success = Result
            )
        );

        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "No Completed orders currently exist.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedCollectedOrdersButton()
    {
        StartCoroutine(ClickedCollectedOrdersButtonCoroutine());
    }
    IEnumerator ClickedCollectedOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetCollectedOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => Success = Result
            )
        );

        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "No Collected orders currently exist.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedFailedOrdersButton()
    {
        StartCoroutine(ClickedFailedOrdersButtonCoroutine());
    }
    IEnumerator ClickedFailedOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetFailedOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => Success = Result
            )
        );

        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "No Failed orders currently exist.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedArchivedOrdersButton()
    {
        //PausePrinting();
        StartCoroutine(ClickedArchivedOrdersButtonCoroutine());
    }
    IEnumerator ClickedArchivedOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        bool Success = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetArchivedOrders
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => Success = Result
            )
        );

        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            Scene3ManagerScript.DisableAllPanels();
            Scene3ManagerScript.EnableHomePagePanel();

            Scene3ManagerScript.MessageText.text = "No Archived orders currently exist.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }





    





    //================================================
    //Search by OrderID / UserID functionality.
    //================================================

    //This is to switch between SearchByOrder <-> SearchByUser.
    public void SwitchIDToSearchBy()
    {
        //The IDToSearchByEnum takes on the value chosen by the user in IDSwitchDropdown in HomePage.
        IDToSearchByEnum = (IDToSearchBy)IDSwitchDropdown.value;

        Debug.Log("Switched to = " + IDToSearchByEnum.ToString());

        //Changing the UI text accordingly.
        if (IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
        {
            SearchByText.text = "Search Order by OrderID";
        }
        else
        {
            SearchByText.text = "Search Orders By UserID";
        }
    }

    //This function is called when a user clicks on the 'Search' / 'Magnifying glass' button.
    public void ClickedSearchedOrdersButton()
    {
        if (SearchInputfield.text.Length <= 0)
        {
            Scene3ManagerScript.MessageText.text = "Search field cannot be empty.";
            Scene3ManagerScript.EnableMessagePanel();

            return;
        }

        StartCoroutine(ClickedSearchedOrdersButtonCoroutine());
    }
    IEnumerator ClickedSearchedOrdersButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        Debug.Log
        (
            "Stationery ID = "
            + Script1.StationeryInfoObject.StationeryID
        );

        bool Success = false;

        if (IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
        {
            yield return StartCoroutine
            (
                Scene3BackendScript.SearchByOrderIDCoroutine
                (
                    Script1.StationeryInfoObject.StationeryID,
                    int.Parse(SearchInputfield.text),
                    Result => Success = Result
                )
            );
        }
        else
        {
            yield return StartCoroutine
            (
                Scene3BackendScript.SearchByUserIDCoroutine
                (
                    Script1.StationeryInfoObject.StationeryID,
                    int.Parse(SearchInputfield.text),
                    Result => Success = Result
                )
            );
        }

        Scene3ManagerScript.DisableInvisiblePanel();

        if (!Success)
        {
            Scene3ManagerScript.DisableAllPanels();

            Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene3ManagerScript.EnableMessagePanel();

            yield break;
        }

        //This is a case where no Order / User of chosen ID was found.
        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            if(IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
            {
                Scene3ManagerScript.MessageText.text = "No order of such OrderID found.";
                Scene3ManagerScript.EnableMessagePanel();
            }
            else
            {
                Scene3ManagerScript.MessageText.text = "No user of such UserID found.";
                Scene3ManagerScript.EnableMessagePanel();
            }

            yield break;
        }

        //This is a case where it was found.
        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }










    //Refresh button =
    public void ClickedRefreshButton()
    {
        Debug.Log("ClickedRefreshButton()");

        Scene3ManagerScript.MessageText.text = "Refreshed.";
        Scene3ManagerScript.EnableMessagePanel();

        StartCoroutine(Scene3BackendScript.Coroutine_RefreshUI());
    }

    //Called by Refresh Coroutine in Scene3Backend, displays the number of working printers based on their ability to print.
    public void UpdateNumberOfWorkingPrinters()
    {
        int WorkingPrinters = 0;

        foreach (PrinterWorker Worker in PrintManagerScript.Workers)
        {
            if
            (
                Worker != null &&
                Worker.WorkedLastTime
            )
            {
                WorkingPrinters++;
            }
        }


        TotalWorkingPrinters.text =
            "Working printers : " +
            WorkingPrinters;
    }

    //This displays 'No Printers connected' if 0 printers are selected.
    public void UpdateNumberOfConnectedPrinters()
    {
        if (Script1.SelectedPrinters.Count <= 0)
        {
            NoPrintersConnected.SetActive(true);
        }
        else
        {
            NoPrintersConnected.SetActive(false);
        }
    }






    //Dropdown button of PrinterStatus panel.
    public void ClickedPrintersStatusDropdown()
    {
        Debug.Log("Clicked Printers Status Dropdown Button");
        if(PrinterStatusPanelActive)
        {
            PrinterStatusPanelActive = false;

            PrinterStatusPanelScroll.SetActive(false);

            PrinterStatusManagerScript.DestroyAllPrinterStatusPanels();
        }
        else
        {
            PrinterStatusPanelActive = true;

            PrinterStatusPanelScroll.SetActive(true);

            PrinterStatusManagerScript.CreatePrinterStatusPanels();
        }
    }






    //Revenue page button.
    public void ClickedRevenueButton()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.LoadRevenue(Script1.StationeryInfoObject.StationeryID);
    }






    //Queue button.
    public void ClickedQueueButton()
    {
        //This is a case where 0 printers have been selected.
        if (Script1.SelectedPrinters.Count <= 0)
        {
            Debug.Log("No printer has been selected!");

            Scene3ManagerScript.MessageText.text = "No printers have been selected\n Click on Select new printers in Settings.";
            Scene3ManagerScript.EnableMessagePanel();

            return;
        }

        //To refresh the Queue.
        Scene3ManagerScript.EnableQueuePage();
        QueueManagerScript.RefreshQueuedOrders();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableQueuePage();
    }











    //================================================
    //Pausing / Unpausing Automatic Printing
    //================================================

    //PausePrinting =
    //1. A user may disable AutomaticPrinting, but cannot pause it.
    //2. Only the app itself has the ability to Pause AutomaticPrinting.
    //3. It does so if it faces some major issues, which need to be fixed before moving ahead.
    //4. A user may Unpause AutomaticPrinting, which is done by clicking the exclamation button.
    
    //5. The app will never speak in terms of Pause / Unpause AutomaticPrinting to the user in
    //order to keep things simple and straight forward.

    public void PausePrinting()
    {
        if (PausePrintingBool)
        {
            Debug.Log("Printing is already Paused.");
            return;
        }

        Debug.Log("Printing Paused.");
        PausePrintingBool = true;



        //Restarting HomePage, and because  PausePrintingBool = true, this time =>
        //StartHomePage -> InitializeHomePage ->
        //AutomaticPrinting() -> Scene3BackendScript.StopAutomaticPrinting() + Scene3BackendScript.StartHomePageRefreshing();
        StartHomePage();



        //Visually indicating that AutomaticPrinting has been paused.
        LoadingIcon.SetActive(false);
        ExclamationButton.SetActive(true);
    }
    
    //UnpausePrinting() is called when a user clicks on the exclamation mark in the Control panel.
    public void UnpausePrinting()
    {
        if (!PausePrintingBool)
        {
            Debug.Log("Printing is already Unpaused.");
            return;
        }

        Debug.Log("Clicked Unpause Printing Button.");



        //To make sure that all Pending Orders are queued, as this resets their 'BeingProcessed' status to false.
        PrintManagerScript.EmptyProcessingOrdersHashSet();

        PausePrintingBool = false;



        //Restarting HomePage, and because  PausePrintingBool = false, this time =>
        //StartHomePage -> InitializeHomePage ->
        //AutomaticPrinting() -> Scene3BackendScript.StopHomePageRefreshing() + Scene3BackendScript.StartAutomaticPrinting();
        StartHomePage();



        //Visually indicating that AutomaticPrinting has been unpaused.
        ExclamationButton.SetActive(false);
        LoadingIcon.SetActive(true);
    }
}
