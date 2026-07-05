using System; 
using UnityEngine;

public class Scene3Manager : MonoBehaviour
{
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;

    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject HomePagePanel;
    [SerializeField] GameObject PendingOrdersPage;
    [SerializeField] GameObject CompletedOrdersPage;
    [SerializeField] GameObject CollectedOrdersPage;
    [SerializeField] GameObject SearchedOrdersPage;



    private void Start()
    {
        DisableAllPanels();
        EnableHomePagePanel();
    }



    public void DisableAllPanels()
    {
        DisableBlankPanel();
        DisableHomePagePanel();
        DisablePendingOrdersPage();
        DisableCompletedOrdersPage();
        DisableCollectedOrdersPage();
        DisableSearchedOrdersPage();
    }
    public void EnableBlankPanel()
    {
        BlankPanel.SetActive(true);
    }
    public void DisableBlankPanel ()
    {
        BlankPanel.SetActive(false);
    }
    public void EnableHomePagePanel()
    {
        HomePageScript.CurrentDate.text = DateTime.Now.ToString("dd-MM-yyyy");

        StartCoroutine(Scene3BackendScript.GetTodayStationeryRevenue(Script1.StationeryInfoObject.StationeryID));

        Scene3BackendScript.GetOrdersCount();

        HomePagePanel.SetActive(true);
    }
    public void DisableHomePagePanel()
    {
        HomePagePanel.SetActive(false);
    }
    public void EnablePendingOrdersPage()
    {
        PendingOrdersPage.SetActive(true);
    }
    public void DisablePendingOrdersPage()
    {
        PendingOrdersPage.SetActive(false);
    }
    public void EnableCompletedOrdersPage()
    {
        CompletedOrdersPage.SetActive(true);
    }
    public void DisableCompletedOrdersPage()
    {
        CompletedOrdersPage.SetActive(false);
    }
    public void EnableCollectedOrdersPage()
    {
        CollectedOrdersPage.SetActive(true);
    }
    public void DisableCollectedOrdersPage()
    {
        CollectedOrdersPage.SetActive(false);
    }
    public void EnableSearchedOrdersPage()
    {
        SearchedOrdersPage.SetActive(true);
    }
    public void DisableSearchedOrdersPage()
    {
        SearchedOrdersPage.SetActive(false);
    }
}
