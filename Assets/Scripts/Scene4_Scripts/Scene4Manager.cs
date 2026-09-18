//namespaces =
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;





public class Scene4Manager : MonoBehaviour
{
    //All pages of Scene4 =
    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject ProfileMenuPanel;
    [SerializeField] GameObject ViewProfileInfoPanel;
    [SerializeField] GameObject UpdateProfilePanel;

    [SerializeField] GameObject InvisiblePanel;



    //Message panel.
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;

   





    //Instantiation of StationeryInfo object.
    public StationeryInfo StationeryInfoObject = new StationeryInfo();





    
  



    private void Start()
    {
        DisableAllPanels();
        EnableProfileMenuPanel();
    }





    //=========================================================
    //Enabling and Disabling of panels =
    //=========================================================
    public void DisableAllPanels()
    {
        DisableProfileMenuPanel();
        DisableViewProfileInfoPanel();
        DisableUpdateProfilePanel();

        DisableInvisiblePanel();
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





    public void EnableProfileMenuPanel()
    {
        ProfileMenuPanel.SetActive(true);
    }
    public void DisableProfileMenuPanel()
    {
        ProfileMenuPanel.SetActive(false);
    }
    public void EnableViewProfileInfoPanel()
    {
        ViewProfileInfoPanel.SetActive(true);
    }
    public void DisableViewProfileInfoPanel()
    {
        ViewProfileInfoPanel.SetActive(false);
    }
    public void EnableUpdateProfilePanel()
    {
        UpdateProfilePanel.SetActive(true);
    }
    public void DisableUpdateProfilePanel()
    {
        UpdateProfilePanel.SetActive(false);
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







    //Control Panel button functions =
    public void ClickedHomePageButton()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
    public void ClickedSettingsButton()
    {
        SceneManager.LoadScene("Scene5_Settings");
    }
}

