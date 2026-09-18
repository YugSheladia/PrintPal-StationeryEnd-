//namespaces =
using UnityEngine;
using TMPro;
using System.Collections;





public class Scene3Manager : MonoBehaviour
{
    //Script references =    
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;



    //All panels in Scene3
    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject HomePagePanel;
    [SerializeField] GameObject OrdersPage;
    [SerializeField] GameObject RevenuePage;
    [SerializeField] GameObject QueuePage;

    [SerializeField] GameObject InvisiblePanel;




    //Notification panel
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;





    private void Start()
    {
        Debug.Log("Scene3Manager.Start()");

        DisableAllPanels();
        InitializeHomePagePanel();
    }









    //Enabling & disabling panels =
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




    //========================================================
    //InitializeHomePagePanel() v EnableHomePagePanel() ->
    //========================================================

    //InitializeHomePagePanel() = Enables HomePage panel + HomePageScript.StartHomePage().
    public void InitializeHomePagePanel()
    {
        Debug.Log("InitializeHomePagePanel()");

        HomePagePanel.SetActive(true);
        HomePageScript.StartHomePage();
    }

    //EnableHomePagePanel() = Enables HomePage panel.
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








    //Message Panel functions =
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











    //When the app can't connect to the server, it enables the "BlankPage" which has a refresh button to try to
    // connect to the server again... this function is connected to the same refresh button.
    public void ClickedBlankPageRefreshButton()
    {
        //Old
        //InitializeHomePagePanel();

        //New =
        StartCoroutine(Scene3BackendScript.CheckServer((isOnline) =>
        {
            if (isOnline)
            {
                Debug.Log("Server is active");
                InitializeHomePagePanel();
            }
            else
            {
                Debug.Log("Server is inactive");

                MessageText.text = "Something went wrong, try again later.";
                EnableMessagePanel();
            }
        }));
    }
}
