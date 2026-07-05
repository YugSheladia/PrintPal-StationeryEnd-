using UnityEngine;

public class Scene4Manager : MonoBehaviour
{
    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject ProfileMenuPanel;
    [SerializeField] GameObject ViewProfileInfoPanel;
    [SerializeField] GameObject UpdateProfilePanel;


    public StationeryInfo StationeryInfoObject = new StationeryInfo();


    private void Start()
    {
        DisableAllPanels();
        EnableProfileMenuPanel();
    }

    

    public void DisableAllPanels()
    {
        DisableBlankPanel();
        DisableProfileMenuPanel();
        DisableViewProfileInfoPanel();
        DisableUpdateProfilePanel();
    }

    public void EnableBlankPanel()
    {
        BlankPanel.SetActive(true);
    }
    public void DisableBlankPanel()
    {
        BlankPanel.SetActive(false);
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
}

