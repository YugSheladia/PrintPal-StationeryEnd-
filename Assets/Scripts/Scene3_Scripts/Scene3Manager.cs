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
    [SerializeField] GameObject RevenuePage;
    [SerializeField] GameObject QueuePage;


    [SerializeField] GameObject InvisiblePanel;
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;





    private void Start()
    {
        Debug.Log("Scene3Manager.Start()");

        DisableAllPanels();
        InitializeHomePagePanel();
    }



    public void DisableAllPanels()
    {
        Debug.Log("Scene3Manager.DisabledAllPanels()");

        DisableInvisiblePanel();

        DisableHomePagePanel();
        DisableOrdersPage();
        DisableRevenuePage();
        DisableQueuePage();

        DisableMessagePanel();
    }



    public void EnableBlankPanel()
    {
        BlankPanel.SetActive(true);
    }
    public void DisableBlankPanel()
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




    public void InitializeHomePagePanel()
    {
        Debug.Log("InitializeHomePagePanel()");

        HomePagePanel.SetActive(true);
        HomePageScript.StartHomePage();
    }
    public void EnableHomePagePanel()
    {
        Debug.Log("EnableHomePagePanel()");

        HomePagePanel.SetActive(true);
    }
    public void DisableHomePagePanel()
    {
        Debug.Log("Scene3Manager.DisabledHomePagePanel()");
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



    public void EnableRevenuePage()
    {
        RevenuePage.SetActive(true);
    }
    public void DisableRevenuePage()
    {
        RevenuePage.SetActive(false);
    }



    public void EnableQueuePage()
    {
        QueuePage.SetActive(true);
    }
    public void DisableQueuePage()
    {
        QueuePage.SetActive(false);
    }
    public bool CheckQueuePage_ActiveStatus()
    {
        return QueuePage.activeSelf;
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
        //EnableHomePagePanel();

        InitializeHomePagePanel();
    }
}
