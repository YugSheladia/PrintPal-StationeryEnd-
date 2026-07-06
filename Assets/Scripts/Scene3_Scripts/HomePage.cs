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

    [SerializeField] PendingOrderStatusManager      PendingOrderStatusManagerScript;
    [SerializeField] RTCOrderStatusManager          RTCOrderStatusManagerScript;
    [SerializeField] CollectedOrderStatusManager    CollectedOrderStatusManagerScript;
    [SerializeField] SearchedOrderStatusManager     SearchedOrderStatusManagerScript;

    [SerializeField] TMP_Text StationeryName;

    [SerializeField] TMP_Dropdown IDSwitchDropdown;
    [SerializeField] TMP_Text SearchByText;
    [SerializeField] TMP_InputField SearchInputfield;

    [SerializeField] public TMP_Text TotalPendingOrdersText;
    [SerializeField] public TMP_Text TotalFinishedOrdersText;
    [SerializeField] public TMP_Text TotalCollectedOrdersText;

    [SerializeField] public TMP_Text CurrentDate;
    [SerializeField] public TMP_Text RevenueToday;



    private void Start()
    {
        StationeryName.text = Script1.StationeryInfoObject.StationeryName;
    }


    public void ClickedRefreshButton()
    {
        CurrentDate.text = DateTime.Now.ToString("dd-MM-yyyy");

        StartCoroutine(Scene3BackendScript.GetTodayStationeryRevenue(Script1.StationeryInfoObject.StationeryID));

        Scene3BackendScript.GetOrdersCount();
    }



    public void ClickedProfileButton()
    {
        SceneManager.LoadScene("Scene4_UpdateProfileInfo");
    }
    public void ClickedSettingsButton()
    {
        Debug.Log("Settings");
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
        yield return StartCoroutine(Scene3BackendScript.GetPendingOrders(Script1.StationeryInfoObject.StationeryID));

        PendingOrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnablePendingOrdersPage();
    }
    public void ClickedCompletedOrdersButton()
    {
        StartCoroutine(ClickedCompletedOrdersButtonCoroutine());
    }
    IEnumerator ClickedCompletedOrdersButtonCoroutine()
    {
        yield return StartCoroutine(Scene3BackendScript.GetCompletedOrders(Script1.StationeryInfoObject.StationeryID));

        RTCOrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableCompletedOrdersPage();
    }
    public void ClickedCollectedOrdersButton()
    {
        StartCoroutine(ClickedCollectedOrdersButtonCoroutine());
    }
    IEnumerator ClickedCollectedOrdersButtonCoroutine()
    {
        yield return StartCoroutine(Scene3BackendScript.GetCollectedOrders(Script1.StationeryInfoObject.StationeryID));

        CollectedOrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableCollectedOrdersPage();
    }
    public void ClickedSearchedOrdersButton()
    {
        StartCoroutine(ClickedSearchedOrdersButtonCoroutine());
    }
    IEnumerator ClickedSearchedOrdersButtonCoroutine()
    {
        Debug.Log("Stationery ID = " + Script1.StationeryInfoObject.StationeryID);

        if (IDToSearchByEnum == IDToSearchBy.SearchByOrderID)
        {
            yield return StartCoroutine(Scene3BackendScript.SearchByOrderIDCoroutine(Script1.StationeryInfoObject.StationeryID, int.Parse(SearchInputfield.text)));
        }
        else
        {
            yield return StartCoroutine(Scene3BackendScript.SearchByUserIDCoroutine(Script1.StationeryInfoObject.StationeryID, int.Parse(SearchInputfield.text)));
        }

        SearchedOrderStatusManagerScript.CreateOrderStatusPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableSearchedOrdersPage();
    }
}
