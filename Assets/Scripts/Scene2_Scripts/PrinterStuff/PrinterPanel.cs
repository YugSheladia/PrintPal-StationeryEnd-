//namespaces =
using UnityEngine;
using TMPro;





public class PrinterPanel : MonoBehaviour
{
    //Reference to the Printer object.
    public Printer Printer;


  
    public bool SupportsDoubleSidedPrinting = false;
    public bool SupportsColorPrinting       = false;



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

    [SerializeField] private GameObject PageCapacityWindow1;

    [SerializeField] private TMP_Text Window1_PageCapacityText;


    //PageCapacity panel for ViewPrinters page.
    //( This does not include the ability to increase / decrease PageCapacity )

    [SerializeField] private GameObject PageCapacityWindow2;

    [SerializeField] private TMP_Text Window2_PageCapacityText;










    //A transparent panel, to allow users to click DoubleSided / Color Toggle only when Printer selection Toggle has been switched on -
    [SerializeField] private GameObject TransparentPanelForUnselectedPrinters;










    //To provide PrinterPanel script the reference to Printer object.
    public void SetPrinter(Printer printer)
    {
        Printer = printer;

        //This is used for remembering the Printer's ability to support a type of print in
        //ViewPrinters page ( After the user has already mentioned the same in AddNewPrinters
        // OR SelectPrinters in Scene5 / Scene2 respectively ).
        
        SupportsDoubleSidedPrinting = Printer.SupportsDoubleSidedPrinting;
        SupportsColorPrinting = Printer.SupportsColorPrinting;

        UpdateUI();
    }

    //SetPrinter() -> UpdateUI().
    public void UpdateUI()
    {
        if (Printer == null)
            return;

        PrinterNameText.text = Printer.Name;

        ToggleOnImage.SetActive(Printer.IsSelected);
        ToggleOffImage.SetActive(!Printer.IsSelected);

        if(Printer.IsSelected)
        {
            TransparentPanelForUnselectedPrinters.SetActive(false);
        }
        else
        {
            TransparentPanelForUnselectedPrinters.SetActive(true);
        }

        DoubleSidedPrintingToggleOnImage.SetActive(SupportsDoubleSidedPrinting);
        DoubleSidedPrintingToggleOffImage.SetActive(!SupportsDoubleSidedPrinting);

        ColorPrintingToggleOnImage.SetActive(SupportsColorPrinting);
        ColorPrintingToggleOffImage.SetActive(!SupportsColorPrinting);

        Window1_PageCapacityText.text = Printer.PageCapacity.ToString();
        Window2_PageCapacityText.text = Printer.PageCapacity.ToString();
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

        SupportsDoubleSidedPrinting = !SupportsDoubleSidedPrinting;
        Printer.SupportsDoubleSidedPrinting = SupportsDoubleSidedPrinting;

        UpdateUI();
    }

    //This function is called when a user clicks on Color toggle button.
    public void ClickedColorPrintingToggle()
    {
        if (Printer == null)
            return;

        SupportsColorPrinting = !SupportsColorPrinting;
        Printer.SupportsColorPrinting = SupportsColorPrinting;

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
        Printer.PageCapacity -= 10;
        Printer.PagesLeft = Printer.PageCapacity;
        
        Debug.Log("Page Capacity for = " + Printer.Name + " = " + Printer.PageCapacity);

        UpdateUI();
    }








    //These functions is used to disable the toggles, & enable / disable the ticks & crosses.

    //This is because PrinterPanels in Scene2, and Scene5's SelectNewPrinters page use toggles,
    //&
    //PrinterPanels in Scene5's ViewPrinters page use ticks and crosses.

    //DisableToggles() disables all toggles / buttons from PrinterPanel.
    public void DisableToggles()
    {
        ToggleButton.SetActive(false);
        ToggleButtonBackground.SetActive(false);

        DoubleSidedToggleButton.SetActive(false);
        ColorToggleButton.SetActive(false);

        PageCapacityWindow1.SetActive(false);
    }
    public void EnableViewPrinterPanel_Stuff()
    {
        if(SupportsDoubleSidedPrinting)
        {
            DoubleSidedPrintingSupportTick.SetActive(true);
            DoubleSidedPrintingSupportCross.SetActive(false);
        }
        else
        {
            DoubleSidedPrintingSupportTick.SetActive(false);
            DoubleSidedPrintingSupportCross.SetActive(true);
        }

        if (SupportsColorPrinting)
        {
            ColorPrintingSupportTick.SetActive(true);
            ColorPrintingSupportCross.SetActive(false);
        }
        else
        {
            ColorPrintingSupportTick.SetActive(false);
            ColorPrintingSupportCross.SetActive(true);
        }

        PageCapacityWindow2.SetActive(true);

    }
    public void DisableViewPrinterPanel_Stuff()
    {
        DoubleSidedPrintingSupportTick.SetActive(false);
        DoubleSidedPrintingSupportCross.SetActive(false);

        ColorPrintingSupportTick.SetActive(false);
        ColorPrintingSupportCross.SetActive(false);
    }
}