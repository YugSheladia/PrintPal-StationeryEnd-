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


    [SerializeField] GameObject AutomaticPrinting_ToggleOn;
    [SerializeField] GameObject AutomaticPrinting_ToggleOff;





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



    public void ClickedRevenueButton()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3ManagerScript.EnableRevenuePage();
        RevenuePageManagerScript.LoadRevenue(Script1.StationeryInfoObject.StationeryID);
    }



    public void ClickedProfileButton()
    {
        if(Script1.AutomaticPrinting)
        {
            Debug.Log("You cannot leave the Home Page while Automatic Printing is enabled or an order is being printed.");

            Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while Automatic Printing is enabled or an order is being processed.";
            Scene3ManagerScript.EnableMessagePanel();

            return;
        }

        SceneManager.LoadScene("Scene4_UpdateProfileInfo");
    }
    public void ClickedSettingsButton()
    {
        if (Script1.AutomaticPrinting)
        {
            Debug.Log("You cannot leave the Home Page while Automatic Printing is enabled or an order is being processed.");

            Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while Automatic Printing is enabled or an order is being processed.";
            Scene3ManagerScript.EnableMessagePanel();

            return;
        }

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

        OrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();
    }



    public void ClickedRefreshButton()
    {
        Debug.Log("ClickedRefreshButton()");

        Scene3ManagerScript.MessageText.text = "Refreshed.";
        Scene3ManagerScript.EnableMessagePanel();

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
        if(PausePrintingBool)
        {
            Debug.Log("Printing is already Paused.");
            return;
        }

        Debug.Log("Printing Paused.");
        PausePrintingBool = true;

        StartHomePage();

        LoadingIcon.SetActive(false);
        ExclamationButton.SetActive(true);
    }
    public void UnpausePrinting()
    {
        if(!PausePrintingBool)
        {
            Debug.Log("Printing is already Unpaused.");
            return;
        }

        Debug.Log("Clicked Unpause Printing Button.");

        //To make sure that all Pending Orders are queued, as this resets their 'BeingProcessed' status to false.
        PrintManagerScript.EmptyProcessingOrdersHashSet();

        PausePrintingBool = false;

        StartHomePage();

        ExclamationButton.SetActive(false);
        LoadingIcon.SetActive(true);
    }
    public void ClickedExclamationButton()
    {
        Debug.Log("Clicked Exclamation Button.");

        //Unpause the printing process, and kickstart it using AutomaticPrinting().
        UnpausePrinting();
    }





    public void ClickedQueueButton()
    {
        QueueManagerScript.RefreshQueuedOrders();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableQueuePage();
    }




    public void ClickedStopAutomaticPrinting()
    {
        Debug.Log("Clicked StopAutomaticPrinting() Button.");

        Script1.SetAutomaticPrintingValue(false);

        StartHomePage();

        AutomaticPrinting_ToggleOn.SetActive(false);
        AutomaticPrinting_ToggleOff.SetActive(true);

        Scene3ManagerScript.MessageText.text = "Automatic Printing Disabled.";
        Scene3ManagerScript.EnableMessagePanel();
    }
    public void ClickedStartAutomaticPrinting()
    {
        Debug.Log("Clicked StartAutomaticPrinting() Button.");

        Script1.SetAutomaticPrintingValue(true);

        StartHomePage();

        AutomaticPrinting_ToggleOff.SetActive(false);
        AutomaticPrinting_ToggleOn.SetActive(true);

        Scene3ManagerScript.MessageText.text = "Automatic Printing Enabled.";
        Scene3ManagerScript.EnableMessagePanel(); 
    }
}
