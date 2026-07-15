using TMPro;
using UnityEngine;
using System.Collections;





public class Scene4Manager : MonoBehaviour
{
    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject ProfileMenuPanel;
    [SerializeField] GameObject ViewProfileInfoPanel;
    [SerializeField] GameObject UpdateProfilePanel;


    public StationeryInfo StationeryInfoObject = new StationeryInfo();

    [SerializeField] GameObject InvisiblePanel;
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;







    private void Start()
    {
        DisableAllPanels();
        EnableProfileMenuPanel();
    }





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
}

