using System; 
using UnityEngine;
using TMPro;
using System.Collections;





public class Scene3Manager : MonoBehaviour
{
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;

    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject HomePagePanel;
    [SerializeField] GameObject OrdersPage;


    [SerializeField] GameObject InvisiblePanel;
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;






    private void Start()
    {
        DisableAllPanels();
        EnableHomePagePanel();
    }



    public void DisableAllPanels()
    {
        DisableInvisiblePanel();

        DisableHomePagePanel();
        DisableOrdersPage();

        DisableMessagePanel();
    }



    public void EnableBlankPanel()
    {
        BlankPanel.SetActive(true);
    }
    public void DisableBlankPanel ()
    {
        BlankPanel.SetActive(false);
    }
    public void EnableInvisiblePanel()
    {
        InvisiblePanel.SetActive(true);
    }
    public void DisableInvisiblePanel()
    {
        InvisiblePanel.SetActive(false);
    }




    public void EnableHomePagePanel()
    {
        EnableBlankPanel();
        EnableInvisiblePanel();

        StartCoroutine(EnableHomePagePanelCoroutine());
    }
    IEnumerator EnableHomePagePanelCoroutine()
    {
        HomePageScript.CurrentDate.text =
            DateTime.Now.ToString("dd-MM-yyyy");

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
            Debug.LogError("Failed To Load Today's Revenue.");

            DisableAllPanels();

            MessageText.text = "Something went wrong, try again later.";
            EnableMessagePanel();

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
            Debug.LogError("Failed To Load Order Counts.");

            DisableAllPanels();

            MessageText.text = "Something went wrong, try again later.";
            EnableMessagePanel();

            yield break;
        }

        DisableAllPanels();
        HomePagePanel.SetActive(true);
    }
    public void DisableHomePagePanel()
    {
        HomePagePanel.SetActive(false);
    }




    public void EnableOrdersPage()
    {
        OrdersPage.SetActive(true);
    }
    public void DisableOrdersPage()
    {
        OrdersPage.SetActive(false);
    }






    private Coroutine MessagePanelCoroutine;
    public void EnableMessagePanel()
    {
        if (MessagePanelCoroutine != null)
        {
            StopCoroutine(MessagePanelCoroutine);
        }

        MessagePanel.SetActive(true);

        MessagePanelCoroutine = StartCoroutine(CoroutineMessagePanelHandler());
    }
    public void DisableMessagePanel()
    {
        if (MessagePanelCoroutine != null)
        {
            StopCoroutine(MessagePanelCoroutine);
            MessagePanelCoroutine = null;
        }

        MessagePanel.SetActive(false);
    }
    IEnumerator CoroutineMessagePanelHandler()
    {
        yield return new WaitForSeconds(5);

        MessagePanel.SetActive(false);
        MessagePanelCoroutine = null;
    }



    public void ClickedBlankPageRefreshButton()
    {
        EnableHomePagePanel();
    }
}
