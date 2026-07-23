using UnityEngine;
using TMPro;

public class PrinterPanel : MonoBehaviour
{
    [SerializeField] private GameObject ToggleOffImage;
    [SerializeField] private GameObject ToggleOnImage;

    [SerializeField] private TMP_Text PrinterNameText;
    [SerializeField] private TMP_Text PrinterStatusText;

    [SerializeField] private GameObject ToggleButton;

    public Printer Printer;



    public void SetPrinter(Printer printer)
    {
        Printer = printer;

        UpdateUI();
    }



    public void UpdateUI()
    {
        if (Printer == null)
            return;

        PrinterNameText.text = Printer.Name;

        ToggleOnImage.SetActive(Printer.IsSelected);
        ToggleOffImage.SetActive(!Printer.IsSelected);

        if (!Printer.IsOnline)
        {
            PrinterStatusText.text = "Offline";
        }
        else if (Printer.IsBusy)
        {
            PrinterStatusText.text = "Busy";
        }
        else
        {
            PrinterStatusText.text = "Ready";
        }
    }



    public void ClickedToggleButton()
    {
        if (Printer == null)
            return;

        Printer.IsSelected = !Printer.IsSelected;

        UpdateUI();
    }



    public void DisableToggle()
    {
        ToggleButton.SetActive(false);

        ToggleOffImage.SetActive(false);
        ToggleOnImage.SetActive(false);
    }
}