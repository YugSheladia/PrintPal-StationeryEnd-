using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HomePage : MonoBehaviour
{
    enum IDToSearchBy
    {
        SearchByOrderID,
        SearchByUserID
    }

    IDToSearchBy IDToSearchByEnum = IDToSearchBy.SearchByOrderID;

    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] OrderStatusManager OrderStatusManagerScript;
    [SerializeField] RevenuePageManager RevenuePageManagerScript;

    [SerializeField] PrintManager PrintManagerScript;

    [SerializeField] PrinterStatusManager PrinterStatusManagerScript;

    [SerializeField] QueueManager QueueManagerScript;


    //UI Elements -
    [SerializeField] TMP_Text StationeryName;

    [SerializeField] public GameObject Disconnected;


    [SerializeField] TMP_Dropdown   IDSwitchDropdown;
    [SerializeField] TMP_Text       SearchByText;
    [SerializeField] TMP_InputField SearchInputfield;


    [SerializeField] public TMP_Text TotalPendingOrdersText;
    [SerializeField] public TMP_Text TotalFinishedOrdersText;
    [SerializeField] public TMP_Text TotalCollectedOrdersText;


    [SerializeField] public TMP_Text CurrentDate;
    [SerializeField] public TMP_Text RevenueToday;


    [SerializeField] public TMP_Text TotalConnectedPrinters;
    [SerializeField] public TMP_Text TotalWorkingPrinters;

   
    [SerializeField] public GameObject LoadingIcon;
    [SerializeField] public GameObject ExclamationButton;

    private bool PausePrintingBool = false;


    private bool PrinterStatusPanelActive = false;
    [SerializeField] private GameObject PrinterStatusPanelScroll;

    [SerializeField] public GameObject NoPrintersConnected;





    public void StartHomePage()
    {
        //Make sure that HomePAgePanel is active -
        Scene3ManagerScript.EnableHomePagePanel();

        Debug.Log("HomePage.StartHomePage()");

        StationeryName.text = Script1.StationeryInfoObject.StationeryName;

        TotalConnectedPrinters.text = "Connected printers : " + Script1.SelectedPrinters.Count;

        StartCoroutine(InitializeHomePage());
    }
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
    
    
   



    

    //=============================================================
    // HomePage Buttons 
    //=============================================================
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

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedCompletedOrdersButton()
    {
        //PausePrinting();

        Debug.Log("Clicked Completed Orders button.");

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

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }
    public void ClickedCollectedOrdersButton()
    {
        //PausePrinting();
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

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }



    public void ClickedRevenueButton()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3ManagerScript.EnableRevenuePage();
        RevenuePageManagerScript.LoadRevenue(Script1.StationeryInfoObject.StationeryID);
    }



    public void ClickedProfileButton()
    {
        SceneManager.LoadScene("Scene4_UpdateProfileInfo");
    }
    public void ClickedSettingsButton()
    {
        SceneManager.LoadScene("Scene5_Settings");
    }




    //Search by OrderID / UserID functionality.
    public void SwitchIDToSearchBy()
    {
        IDToSearchByEnum = (IDToSearchBy)IDSwitchDropdown.value;
        Debug.Log("Switched to = " + IDToSearchByEnum.ToString());

        if (IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
        {
            SearchByText.text = "Search Order by OrderID";
        }
        else
        {
            SearchByText.text = "Search Orders By UserID";
        }
    }
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

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }



    public void ClickedRefreshButton()
    {
        Debug.Log("ClickedRefreshButton()");
        StartCoroutine(Scene3BackendScript.Coroutine_RefreshUI());
    }
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



    public void PausePrinting()
    {
        Debug.Log("Printing Paused.");
        PausePrintingBool = true;

        StartHomePage();

        LoadingIcon.SetActive(false);
        ExclamationButton.SetActive(true);
    }
    public void UnpausePrinting()
    {
        Debug.Log("Clicked Unpause Printing Button.");
        PausePrintingBool = false;

        StartHomePage();

        ExclamationButton.SetActive(false);
        LoadingIcon.SetActive(true);
    }
    public void ClickedExclamationButton()
    {
        Debug.Log("Clicked Exclamation Button.");

        //The line below has been commented out because it only resets the list of failed attempts of one job, not all... exclamation mark is for resetting the same for all jobs.
        //PrintManagerScript.Reset_ListOfFailedAttemptsOf_LastFailedJob();


        PrintManagerScript.EmptyProcessingOrdersHashSet();


        //Unpause the printing process, and kickstart it using AutomaticPrinting().
        UnpausePrinting();
    }




    public void ClickedQueueButton()
    {
        QueueManagerScript.RefreshQueuedOrders();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableQueuePage();
    }
}
