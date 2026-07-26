using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene5Manager : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject ViewPrintersPanel;
    [SerializeField] private GameObject UpdatePrintersPanel;

    [SerializeField] private GameObject InvisiblePanel;
    [SerializeField] private GameObject MessagePanel;

    [SerializeField] public TMP_Text MessageText;










    void Start()
    {
        DisableAllPanels();
        EnableSettingsPanel();
    }






    public void DisableAllPanels()
    {
        DisableInvisiblePanel();

        DisableSettingsPanel();
        DisableViewPrintersPanel();
        DisableUpdatePrintersPanel();

        DisableMessagePanel();
    }



    public void EnableSettingsPanel()
    {
        SettingsPanel.SetActive(true);
    }
    public void DisableSettingsPanel()
    {
        SettingsPanel.SetActive(false);
    }
    public void EnableViewPrintersPanel()
    {
        ViewPrintersPanel.SetActive(true);
    }
    public void DisableViewPrintersPanel()
    {
        ViewPrintersPanel.SetActive(false);
    }
    public void EnableUpdatePrintersPanel()
    {
        UpdatePrintersPanel.SetActive(true);
    }
    public void DisableUpdatePrintersPanel()
    {
        UpdatePrintersPanel.SetActive(false);
    }
    public void EnableInvisiblePanel()
    {
        InvisiblePanel.SetActive(true);
    }
    public void DisableInvisiblePanel()
    {
        InvisiblePanel.SetActive(false);
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






    //Control Panel
    public void ClickedHomePageButton()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
    public void ClickedAccountButton()
    {
        SceneManager.LoadScene("Scene4_UpdateProfileInfo");
    }

}
