using UnityEngine;
using TMPro;
using System.Collections;






[System.Serializable]
public class StationeryInfo
{
    public int    StationeryID;

    public string PhoneNumber;
    public string Password;

    public string StationeryName;
    public string StationeryLocation;
    public string UPINumber;
    public string Email;

    //public bool AutomaticPrinting = false;

    public StationeryInfo() { }

    public void PrintStationeryDetails()
    {
        Debug.Log("StationeryID             = " + StationeryID          +
                  "\nPhone Number           = " + PhoneNumber           +
                  "\nPassword               = " + Password              +
                  "\nStationery Name        = " + StationeryName        +
                  "\nStationery Location    = " + StationeryLocation    +
                  "\nUPI Number             = " + UPINumber             + 
                  "\nEmail                  = " + Email);
    }
}







public class Scene2Manager : MonoBehaviour
{
    [SerializeField] GameObject BlankPanel;
    [SerializeField] GameObject LoginPanel;
    [SerializeField] GameObject SetPhoneNumberPanel;
    [SerializeField] GameObject SetPasswordPanel;
    [SerializeField] GameObject SetProfileInfoPanel;
    [SerializeField] GameObject SelectPrintersPanel;

    [SerializeField] GameObject InvisiblePanel;
    [SerializeField] GameObject MessagePanel;
    [SerializeField] public TMP_Text MessageText;






    public void Start()
    {
        DisableAllPanels();
        EnableLoginPanel();
    }




    public void DisableAllPanels()
    {
        DisableBlankPanel();
        DisableInvisiblePanel();

        DisableLoginPanel();
        DisableSetPhoneNumberPanel();
        DisableSetPasswordPanel();
        DisableSetProfileInfoPanel();
        DisableSelectPrintersPanel();

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




    public void EnableLoginPanel()
    {
        LoginPanel.SetActive(true);
    }
    public void DisableLoginPanel()
    {
        LoginPanel.SetActive(false);
    }
    public void EnableSetPhoneNumberPanel()
    {
        SetPhoneNumberPanel.SetActive(true);
    }
    public void DisableSetPhoneNumberPanel()
    {
        SetPhoneNumberPanel.SetActive(false);
    }
    public void EnableSetPasswordPanel()
    {
        SetPasswordPanel.SetActive(true);
    }
    public void DisableSetPasswordPanel()
    {
        SetPasswordPanel.SetActive(false);
    }
    public void EnableSetProfileInfoPanel()
    {
        SetProfileInfoPanel.SetActive(true);
    }
    public void DisableSetProfileInfoPanel()
    {
        SetProfileInfoPanel.SetActive(false);
    }
    public void EnableSelectPrintersPanel()
    {
        SelectPrintersPanel.SetActive(true);
    }
    public void DisableSelectPrintersPanel()
    {
        SelectPrintersPanel.SetActive(false);
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
