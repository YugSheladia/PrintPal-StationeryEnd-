using UnityEngine;
using TMPro;



public class PrinterPanel : MonoBehaviour
{
    [SerializeField] GameObject ToggleOffImage;
    [SerializeField] GameObject ToggleOnImage;

    enum ToggleEnum
    {
        off,
        on
    }

    ToggleEnum Toggle;


    [SerializeField] TMP_Text PrinterNameText;
    public string PrinterName;

    [SerializeField] TMP_Text PrinterStatusText;


    enum PrinterStatusEnum
    {
        Ready,
        Printing,
        Busy,
        Error,
        Offline,
        Unknown
    }

    PrinterStatusEnum printerStatusEnum;





    public void Start()
    {
        SwitchToToggleOff();
    }




    public void SetPrinterDetails(string PrinterName, string PrinterStatus)
    {
        this.PrinterName = PrinterName;

        switch (PrinterStatus)
        {
            case "Normal":
                // Green - Ready
                printerStatusEnum = PrinterStatusEnum.Ready;
                break;

            case "Printing":
                // Blue - Printing
                printerStatusEnum = PrinterStatusEnum.Printing;
                break;

            case "Busy":
                // Yellow - Busy
                printerStatusEnum = PrinterStatusEnum.Busy;
                break;

            case "Error":
                // Red - Error
                printerStatusEnum = PrinterStatusEnum.Error;
                break;

            case "Offline":
                // Gray - Offline
                printerStatusEnum = PrinterStatusEnum.Offline;
                break;

            default:
                // Gray - Unknown
                printerStatusEnum = PrinterStatusEnum.Unknown;
                break;
        }
    }
    public void UpdateUI()
    {
        PrinterNameText.text = PrinterName;

        switch (printerStatusEnum)
        {
            case PrinterStatusEnum.Ready:
                // Green - Ready
                PrinterStatusText.text = "Ready";
                break;

            case PrinterStatusEnum.Printing:
                // Blue - Printing
                PrinterStatusText.text = "Printing";
                break;

            case PrinterStatusEnum.Busy:
                // Yellow - Busy
                PrinterStatusText.text = "Busy";
                break;

            case PrinterStatusEnum.Error:
                // Red - Error
                PrinterStatusText.text = "Error";
                break;

            case PrinterStatusEnum.Offline:
                // Gray - Offline
                PrinterStatusText.text = "Offline";
                break;

            default:
                // Gray - Unknown
                PrinterStatusText.text = "Unknown";
                break;
        }
    }





    public void ClickedToggleButton()
    {
        Debug.Log("Clicked Toggle Button.");

        if (Toggle == ToggleEnum.off)
        {
            SwitchToToggleOn();
        }
        else
        {
            SwitchToToggleOff();
        }
    }

    public void SwitchToToggleOn()
    {
        Toggle = ToggleEnum.on;

        ToggleOnImage.SetActive(true);
        ToggleOffImage.SetActive(false);
    }

    public void SwitchToToggleOff()
    {
        Toggle = ToggleEnum.off;

        ToggleOffImage.SetActive(true);
        ToggleOnImage.SetActive(false);
    }
}
