//namespaces =
using UnityEngine;
using TMPro;





public class PrinterPanel : MonoBehaviour
{
    //Script references =
    Scene2Manager Scene2ManagerScript;




    //Reference to the Printer object.
    public Printer Printer;




    //UI stuff =
    [SerializeField] private TMP_Text PrinterNameText;


    [SerializeField] private GameObject ToggleButton;
    [SerializeField] private GameObject ToggleButtonBackground;

    [SerializeField] private GameObject DoubleSidedToggleButton;
    [SerializeField] private GameObject ColorToggleButton;

  




    //Images 

    //Printer Selection toggle images -
    [SerializeField] private GameObject ToggleOffImage;
    [SerializeField] private GameObject ToggleOnImage;

    //Printer's Double Sided printing support toggle images - 
    [SerializeField] private GameObject DoubleSidedPrintingToggleOffImage;
    [SerializeField] private GameObject DoubleSidedPrintingToggleOnImage;

    //Printer's Color printing support toggle images - 
    [SerializeField] private GameObject ColorPrintingToggleOffImage;
    [SerializeField] private GameObject ColorPrintingToggleOnImage;

    //Printer's Double Sided printing support tick / cross in View Printers Page.
    [SerializeField] private GameObject DoubleSidedPrintingSupportTick;
    [SerializeField] private GameObject DoubleSidedPrintingSupportCross;

    //Printer's Color printing support tick / cross in View Printers Page.
    [SerializeField] private GameObject ColorPrintingSupportTick;
    [SerializeField] private GameObject ColorPrintingSupportCross;









    //=============================
    //PageCapacity stuff =
    //=============================

    //PageCapacity panel for SelectPrinters / UpdatePrinters page.
    //( This includes the ability to increase / decrease PageCapacity )

    [SerializeField] private GameObject PageCapacityWindow_ForUpdatePrinters;

    [SerializeField] private TMP_Text PageCapacityText_ForUpdatePrinters;


    //PageCapacity panel for ViewPrinters page.
    //( This does not include the ability to increase / decrease PageCapacity )

    [SerializeField] private GameObject PageCapacityWindow_ForViewPrinters;

    [SerializeField] private TMP_Text PageCapacityText_ForViewPrinters;









    //==============================
    //Printer Speed stuff =
    //==============================

    //Printer speed is decided by PPM.

    //Window1 = PPM Panel in UpdatePrinters.
    //( This includes the ability to increase / decrease PPM )

    [SerializeField] private GameObject PPMWindow_ForUpdatePrinters;

    [SerializeField] private TMP_Text PPMText_ForUpdatePrinters;


    //Window2 = PPM Panel in ViewPrinters. 
    //( This does not include the ability to increase / decrease PPM )

    [SerializeField] private GameObject PPMWindow_ForViewPrinters;

    [SerializeField] private TMP_Text PPMText_ForViewPrinters;










    //A transparent panel, to allow users to click DoubleSided / Color Toggle only when Printer selection Toggle has been switched on -
    [SerializeField] private GameObject TransparentPanelForUnselectedPrinters;








    //To provide necessary scripts.
    public void SetRequiredScripts(Scene2Manager Scene2ManagerScript)
    {
        this.Scene2ManagerScript = Scene2ManagerScript;
    }

    //To provide PrinterPanel script the reference to Printer object.
    public void SetPrinter(Printer printer)
    {
        Printer = printer;

        UpdateUI();
    }

    //SetPrinter() -> UpdateUI().
    public void UpdateUI()
    {
        //Only move ahead if Printer is not null.
        if (Printer == null)
            return;


        //Printer name.
        PrinterNameText.text = Printer.Name;


        //IsSelect toggle.
        ToggleOnImage.SetActive(Printer.IsSelected);
        ToggleOffImage.SetActive(!Printer.IsSelected);


        //Transparent panel.
        //This is enabled by default, and is disabled when the printer IsSelected.
        TransparentPanelForUnselectedPrinters.SetActive(Printer.IsSelected);
        TransparentPanelForUnselectedPrinters.SetActive(!Printer.IsSelected);


        //DoubleSided support toggle.
        DoubleSidedPrintingToggleOnImage.SetActive(Printer.SupportsDoubleSidedPrinting);
        DoubleSidedPrintingToggleOffImage.SetActive(!Printer.SupportsDoubleSidedPrinting);


        //Color support toggle.
        ColorPrintingToggleOnImage.SetActive(Printer.SupportsColorPrinting);
        ColorPrintingToggleOffImage.SetActive(!Printer.SupportsColorPrinting);




        //==================================
        //Window1 = UpdatePrinters panel.
        //==================================

        PageCapacityText_ForUpdatePrinters.text = Printer.PageCapacity.ToString();
        PPMText_ForUpdatePrinters.text = Printer.PPM.ToString();

        Debug.Log("PPM = " + Printer.PPM);

        //==================================
        //Window2 = ViewPrinters panel.
        //==================================

        PageCapacityText_ForViewPrinters.text = Printer.PageCapacity.ToString();
        PPMText_ForViewPrinters.text = Printer.PPM.ToString();
    }







    //This function is called when a user clicks on the main toggle button of PrinterPanel.
    public void ClickedToggleButton()
    {
        if (Printer == null)
            return;

        Printer.IsSelected = !Printer.IsSelected;

        UpdateUI();
    }

    //This function is called when a user clicks on DoubleSided toggle button.
    public void ClickedDoubleSidedToggle()
    {
        if (Printer == null)
            return;

        Printer.SupportsDoubleSidedPrinting = !Printer.SupportsDoubleSidedPrinting;

        UpdateUI();
    }

    //This function is called when a user clicks on Color toggle button.
    public void ClickedColorPrintingToggle()
    {
        if (Printer == null)
            return;

        Printer.SupportsColorPrinting = !Printer.SupportsColorPrinting;

        UpdateUI();
    }








    //===================================================
    //Page Capacity Increase / Decrease buttons =
    //===================================================

    //Page Capacity Increase button =
    public void ClickedPageCapacityIncreaseButton()
    {
        Printer.PageCapacity += 10;
        Printer.PagesLeft = Printer.PageCapacity;

        Debug.Log("Page Capacity for = " + Printer.Name + " = " + Printer.PageCapacity);

        UpdateUI();
    }

    //Page Capacity Increase button =
    public void ClickedPageCapacityDecreaseButton()
    {
        if (Printer.PageCapacity < 10)
        {
            Printer.PageCapacity = 10;
            UpdateUI();


            Debug.Log
            (
                "Page capacity of " + Printer.Name + " is less than 10 pages, which is not accepted. " +
                "Therefore automatically updating it to 10"
            );

            Scene2ManagerScript.MessageText.text = "The PageCapacity of " + Printer.Name + " is less than 10 pages." +
                                                   " Therefore automatically updating it to 10";

            Scene2ManagerScript.EnableMessagePanel();

            return;
        }

        if (Printer.PageCapacity == 10)
        {
            Debug.Log("Cannot decrease PageCapacity further.");

            Scene2ManagerScript.MessageText.text = "Cannot decrease PageCapacity further.";
            Scene2ManagerScript.EnableMessagePanel();

            return;
        }

        Printer.PageCapacity -= 10;
        Printer.PagesLeft = Printer.PageCapacity;
        
        Debug.Log("Page Capacity for = " + Printer.Name + " = " + Printer.PageCapacity);

        UpdateUI();
    }











    //===================================================
    //PPM Increase / Decrease buttons =
    //===================================================

    //Page Capacity Increase button =
    public void ClickedPPMIncreaseButton()
    {
        Printer.PPM += 1;

        Debug.Log("PPM for = " + Printer.Name + " = " + Printer.PPM);

        UpdateUI();
    }

    //Page Capacity Increase button =
    public void ClickedPPMDecreaseButton()
    {
        if (Printer.PPM < 1)
        {
            Printer.PPM = 1;
            UpdateUI();


            Debug.Log
            (
                "The speed of " + Printer.Name + " is less than 1 page per minute, which is not accepted. " +
                "Therefore automatically updating it to 1"
            );

            Scene2ManagerScript.MessageText.text = "The speed of " + Printer.Name + " is less than 1 page per minute." +
                                                   " Therefore automatically updating it to 1";


            Scene2ManagerScript.EnableMessagePanel();

            return;
        }

        if (Printer.PageCapacity == 1)
        {
            Debug.Log("Cannot decrease the speed of the printer further.");

            return;
        }

        Printer.PPM -= 1;

        Debug.Log("PPM for = " + Printer.Name + " = " + Printer.PPM);

        UpdateUI();
    }










    //====================================================================================================
    //These functions is used to disable the toggles, & enable / disable the ticks & crosses.

    //This is because PrinterPanels in Scene2, and Scene5's SelectNewPrinters page use toggles,
    //&
    //PrinterPanels in Scene5's ViewPrinters page use ticks and crosses.
    //====================================================================================================


    //DisableToggles() disables all toggles / buttons from PrinterPanel.
    public void DisableToggles()
    {
        //This is to disable the IsSelected toggle.
        ToggleButton.SetActive(false);


        //This is to disable the IsSelected toggle's background.
        ToggleButtonBackground.SetActive(false);


        //This disables the DoubleSided & Color support toggles.
        DoubleSidedToggleButton.SetActive(false);
        ColorToggleButton.SetActive(false);


        //This disables the Increae and Decrease buttons for both -
        //a. Printer Capacity
        //b. Printer speed ( PPM ).
        PageCapacityWindow_ForUpdatePrinters.SetActive(false);
        PPMWindow_ForUpdatePrinters.SetActive(false);
    }



    //This function enables all ViewPrinters stuff, like =
    //a. DoublSided Tick / Cross.
    //b. Color Tick / Cross.
    //c. Enables Window2 ( For PageCapacity & PPM ).
    public void EnableViewPrinterPanel_Stuff()
    {
        if(Printer.SupportsDoubleSidedPrinting)
        {
            DoubleSidedPrintingSupportTick.SetActive(true);
            DoubleSidedPrintingSupportCross.SetActive(false);
        }
        else
        {
            DoubleSidedPrintingSupportTick.SetActive(false);
            DoubleSidedPrintingSupportCross.SetActive(true);
        }

        if (Printer.SupportsColorPrinting)
        {
            ColorPrintingSupportTick.SetActive(true);
            ColorPrintingSupportCross.SetActive(false);
        }
        else
        {
            ColorPrintingSupportTick.SetActive(false);
            ColorPrintingSupportCross.SetActive(true);
        }

        PageCapacityWindow_ForViewPrinters.SetActive(true);

        PPMWindow_ForViewPrinters.SetActive(true);
    }




    //This function disables all ViewPrinters stuff.
    public void DisableViewPrinterPanel_Stuff()
    {
        DoubleSidedPrintingSupportTick.SetActive(false);
        DoubleSidedPrintingSupportCross.SetActive(false);

        ColorPrintingSupportTick.SetActive(false);
        ColorPrintingSupportCross.SetActive(false);

        PageCapacityWindow_ForViewPrinters.SetActive(false);

        PPMWindow_ForViewPrinters.SetActive(false);
    }
}