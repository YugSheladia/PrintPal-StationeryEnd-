using UnityEngine;
using TMPro;

public class PrinterPanel : MonoBehaviour
{
    public Printer Printer;

    //public bool IsSelected                  = false;
    public bool SupportsDoubleSidedPrinting = false;
    public bool SupportsColorPrinting       = false;

    [SerializeField] private TMP_Text PrinterNameText;


    [SerializeField] private GameObject ToggleButton;
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

    //A transparent panel, to allow users to click DoubleSided / Color Toggle only when Printer selection Toggle has been switched on -
    [SerializeField] private GameObject TransparentPanelForUnselectedPrinters;





    public void SetPrinter(Printer printer)
    {
        Printer = printer;

        SupportsDoubleSidedPrinting = Printer.SupportsDoubleSidedPrinting;
        SupportsColorPrinting = Printer.SupportsColorPrinting;

        UpdateUI();
    }
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
            Debug.Log("False");
        }
        else
        {
            TransparentPanelForUnselectedPrinters.SetActive(true);
            Debug.Log("True");
        }

        DoubleSidedPrintingToggleOnImage.SetActive(SupportsDoubleSidedPrinting);
        DoubleSidedPrintingToggleOffImage.SetActive(!SupportsDoubleSidedPrinting);

        ColorPrintingToggleOnImage.SetActive(SupportsColorPrinting);
        ColorPrintingToggleOffImage.SetActive(!SupportsColorPrinting);
    }



    public void ClickedToggleButton()
    {
        if (Printer == null)
            return;

        Printer.IsSelected = !Printer.IsSelected;

        UpdateUI();
    }



    public void DisableToggles()
    {
        ToggleButton.SetActive(false);

        DoubleSidedToggleButton.SetActive(false);
        ColorToggleButton.SetActive(false);

        //ToggleOffImage.SetActive(false);
        //ToggleOnImage.SetActive(false);
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
    }
    public void DisableViewPrinterPanel_Stuff()
    {
        DoubleSidedPrintingSupportTick.SetActive(false);
        DoubleSidedPrintingSupportCross.SetActive(false);

        ColorPrintingSupportTick.SetActive(false);
        ColorPrintingSupportCross.SetActive(false);
    }



    public void ClickedDoubleSidedToggle()
    {
        if (Printer == null)
            return;

        SupportsDoubleSidedPrinting = !SupportsDoubleSidedPrinting;
        Printer.SupportsDoubleSidedPrinting = SupportsDoubleSidedPrinting;

        UpdateUI();
    }
    public void ClickedColorPrintingToggle()
    {
        if (Printer == null)
            return;

        SupportsColorPrinting = !SupportsColorPrinting;
        Printer.SupportsColorPrinting = SupportsColorPrinting;

        UpdateUI();
    }
}