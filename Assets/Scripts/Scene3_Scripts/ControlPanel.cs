//namespaces =
using UnityEngine;
using UnityEngine.SceneManagement;







public class ControlPanel : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

    [SerializeField] PrintManager PrintManagerScript;

    [SerializeField] HomePage HomePageScript;








    //============================
    //Control panel buttons =
    //============================

    //To go from HomePage -> ProfilePage.
    public void ClickedProfileButton()
    {
        //This request helps to check if the server is online and connected to the app or not. If yes, only then we move forward.
        StartCoroutine(Scene3BackendScript.CheckServer((isOnline) =>
        {
            if (isOnline)
            {
                Debug.Log("Server is active");

                //Check here if Automatic Printing is enabled.
                if (Script1.AutomaticPrinting)
                {
                    Debug.Log("You cannot leave the Home Page while Automatic Printing is enabled.");

                    Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while Automatic Printing is enabled.";
                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                //Check here if any orders are being processed or are currently in a queue.
                if (!PrintManager.Instance.CheckIf_AllPrinterQueues_AreEmpty()
                    &&
                    !PrintManager.Instance.Is_ProcessingOrders_HashSet_Empty())
                {
                    Debug.Log("You cannot leave the Home Page while an order is being processed.");

                    Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while an order is being processed.";
                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                SceneManager.LoadScene("Scene4_UpdateProfileInfo");
            }
            else
            {
                Debug.Log("Server is inactive");

                Scene3ManagerScript.DisableAllPanels();

                Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
                Scene3ManagerScript.EnableMessagePanel();
            }
        }));
    }

    //To go from HomePage -> SettingsPage.
    public void ClickedSettingsButton()
    {

        //This request helps to check if the server is online and connected to the app or not. If yes, only then we move forward.
        StartCoroutine(Scene3BackendScript.CheckServer((isOnline) =>
        {
            if (isOnline)
            {
                Debug.Log("Server is active");

                //Check here if Automatic Printing is enabled.
                if (Script1.AutomaticPrinting)
                {
                    Debug.Log("You cannot leave the Home Page while Automatic Printing is enabled.");

                    Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while Automatic Printing is enabled.";
                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                //Check here if any orders are being processed or are currently in a queue.
                if (!PrintManager.Instance.CheckIf_AllPrinterQueues_AreEmpty()
                    &&
                    !PrintManager.Instance.Is_ProcessingOrders_HashSet_Empty())
                {
                    Debug.Log("You cannot leave the Home Page while an order is being processed.");

                    Scene3ManagerScript.MessageText.text = "You cannot leave the Home Page while an order is being processed.";
                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                SceneManager.LoadScene("Scene5_Settings");
            }
            else
            {
                Debug.Log("Server is inactive");

                Scene3ManagerScript.DisableAllPanels();

                Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
                Scene3ManagerScript.EnableMessagePanel();
            }
        }));
    }







    //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
    //Auatomatic Printing stuff =
    //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

    //These functions are used to manually start or stop automatic printing.
    public void ClickedStopAutomaticPrinting()
    {
        Debug.Log("Clicked StopAutomaticPrinting() Button.");

        //This request helps to check if the server is online and connected to the app or not. If yes, only then we move forward.
        StartCoroutine(Scene3BackendScript.CheckServer((isOnline) =>
        {
            if (isOnline)
            {
                Debug.Log("Server is active");

                //Insert the true case here =

                //Disabling AutomaticPrinting.
                Script1.SetAutomaticPrintingValue(false);

                //Restarting HomePage.
                HomePageScript.StartHomePage();

                //Visually indicating that AutomaticPrinting is disabled.
                HomePageScript.AutomaticPrinting_ToggleOn.SetActive(false);
                HomePageScript.AutomaticPrinting_ToggleOff.SetActive(true);

                Scene3ManagerScript.MessageText.text = "Automatic Printing Disabled.";
                Scene3ManagerScript.EnableMessagePanel();
            }
            else
            {
                Debug.Log("Server is inactive");

                Scene3ManagerScript.DisableAllPanels();

                Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
                Scene3ManagerScript.EnableMessagePanel();
            }
        }));
    }
    public void ClickedStartAutomaticPrinting()
    {
        //This request helps to check if the server is online and connected to the app or not. If yes, only then we move forward.
        StartCoroutine(Scene3BackendScript.CheckServer((isOnline) =>
        {
            if (isOnline)
            {
                //Debug.Log("Server is active");

                //Insert the true case here =

                //Debug.Log("Clicked StartAutomaticPrinting() Button.");

                //Enabling AutomaticPrinting.
                Script1.SetAutomaticPrintingValue(true);

                //Restarting HomePage.
                HomePageScript.StartHomePage();

                //Visually indicating that AutomaticPrinting is enabled.
                HomePageScript.AutomaticPrinting_ToggleOff.SetActive(false);
                HomePageScript.AutomaticPrinting_ToggleOn.SetActive(true);

                Scene3ManagerScript.MessageText.text = "Automatic Printing Enabled.";
                Scene3ManagerScript.EnableMessagePanel();
            }
            else
            {
                Debug.Log("Server is inactive");

                Scene3ManagerScript.DisableAllPanels();

                Scene3ManagerScript.MessageText.text = "Something went wrong, try again later.";
                Scene3ManagerScript.EnableMessagePanel();
            }
        }));
    }








    //================================================
    //Pausing / Unpausing Automatic Printing
    //================================================

    //This button is used by the user to indicate that the user has 'fixed some issue' and is trying to start the automatic printing process again.
    public void ClickedExclamationButton()
    {
        Debug.Log("Clicked Exclamation Button.");

        //Unpause the printing process, and kickstart it using AutomaticPrinting().
        HomePageScript.UnpausePrinting();
    }
}
