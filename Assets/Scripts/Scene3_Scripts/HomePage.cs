using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    [SerializeField] TMP_Text StationeryName;

    [SerializeField] TMP_Dropdown IDSwitchDropdown;
    [SerializeField] TMP_Text SearchByText;
    [SerializeField] TMP_InputField SearchInputfield;

    [SerializeField] public TMP_Text TotalPendingOrdersText;
    [SerializeField] public TMP_Text TotalFinishedOrdersText;
    [SerializeField] public TMP_Text TotalCollectedOrdersText;

    [SerializeField] public TMP_Text CurrentDate;
    [SerializeField] public TMP_Text RevenueToday;

    [SerializeField] public TMP_Text TotalConnectedPrinters;



    private void Start()
    {
        StationeryName.text = Script1.StationeryInfoObject.StationeryName;
        TotalConnectedPrinters.text = "Connected printers : " + Script1.SelectedPrinters.Count; 
    }





    public void ClickedRefreshButton()
    {
        StartCoroutine(ClickedRefreshButtonCoroutine());
    }
    IEnumerator ClickedRefreshButtonCoroutine()
    {
        Scene3ManagerScript.EnableInvisiblePanel();

        CurrentDate.text = DateTime.Now.ToString("dd-MM-yyyy");

        bool RevenueSuccess = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetTodayStationeryRevenue
            (
                Script1.StationeryInfoObject.StationeryID,
                Result => RevenueSuccess = Result
            )
        );

        if (!RevenueSuccess)
        {
            RefreshFailed();
            yield break;
        }

        bool OrdersCountSuccess = false;

        yield return StartCoroutine
        (
            Scene3BackendScript.GetOrdersCount
            (
                Result => OrdersCountSuccess = Result
            )
        );

        if (!OrdersCountSuccess)
        {
            RefreshFailed();
            yield break;
        }

        Scene3ManagerScript.DisableInvisiblePanel();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    void RefreshFailed()
    {
        Scene3ManagerScript.DisableAllPanels();

        Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";

        Scene3ManagerScript.EnableMessagePanel();
    }






    public void ClickedProfileButton()
    {
        SceneManager.LoadScene("Scene4_UpdateProfileInfo");
    }
    public void ClickedSettingsButton()
    {
        SceneManager.LoadScene("Scene5_Settings");
    }





    public void SwitchIDToSearchBy()
    {
        IDToSearchByEnum = (IDToSearchBy)IDSwitchDropdown.value;
        Debug.Log("Switched to = " + IDToSearchByEnum.ToString());

        if(IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
        {
            SearchByText.text = "Search Order by OrderID";
        }
        else
        {
            SearchByText.text = "Search Orders By UserID";
        }
    }








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









    //Search by OrderID / UserID functionality.
    public void ClickedSearchedOrdersButton()
    {
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








    public void ClickedGetAllPrinters()
    {
        Debug.Log("Pressed ViewAllPrinters");
    }
}
