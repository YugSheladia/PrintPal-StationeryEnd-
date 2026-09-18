//namespaces =
using System.Collections.Generic;
using UnityEngine;




//This is a reference to Scene2Backend, probably used for retrieving all printers saved in a computer's history of printers.
using static Scene2Backend;






public class UpdatePrinters : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene5Manager Scene5ManagerScript;
    [SerializeField] Scene5Backend Scene5BackendScript;




    //This is the prefab used to instantiate printer panels.
    [SerializeField] GameObject PrinterPanelPrefab;

    //This objects acts as a parent to all Printer panels.
    [SerializeField] GameObject PrinterPanelsParent;




    //A list to save all retrieved printer's Panel.
    public List<PrinterPanel> PrinterList = new();







    //This function is called by ClickedUpdatePrinters -> GetPrinters().
    public void GetConnectedPrinters(System.Action<bool> OnComplete)
    {
        //Deleting aldready exisitng Printer panels inside PrinterPanelsParnet to cleanup before producing them again.
        foreach (Transform child in PrinterPanelsParent.transform)
        {
            Destroy(child.gameObject);
        }

        PrinterList.Clear();



        StartCoroutine
        (
            //This is to retrieve all printers which have been saved in computers's history of printers.
            Scene5BackendScript.GetPrinters
            (
                (printers) =>
                {
                    if (printers == null)
                    {
                        Debug.LogError("Failed to retrieve printers.");

                        Scene5ManagerScript.MessageText.text =
                            "No connected printers were found\nMake sure all printers are connected ( via USB or Bluetooth )";

                        OnComplete?.Invoke(false);
                        return;
                    }

                    foreach (PrinterInfo printerInfo in printers)
                    {
                        Printer printer = new Printer()
                        {
                            Name = printerInfo.name,


                            //In the beginning, all printers are assumed to be 'not selected'.
                            IsSelected = false,

                            //In the beginning, all printers are assumed to be "Online', and their further status will be decided based on their abiloty to print / not print.
                            IsOnline = true,

                            //This is assumed to be false ( until a better solution is found ).
                            IsBusy = false,

                            //This is unused.
                            AutomaticPrinting = false,

                            //In the beginning, it is assumed that the printer supports neither double sided printing, not color printing.
                            SupportsDoubleSidedPrinting = false,
                            SupportsColorPrinting = false
                        };

                        CreatePrinterPanel(printer);
                    }

                    OnComplete?.Invoke(true);
                }
            )
        );
    }

    //This function is called by GetConnectedPrinters(), and is used to create Printer panels in UpdatePrinters page.
    void CreatePrinterPanel(Printer printer)
    {
        GameObject panelObject = Instantiate
        (
            PrinterPanelPrefab,
            PrinterPanelsParent.transform
        );

        PrinterPanel panel = panelObject.GetComponent<PrinterPanel>();

        panel.SetPrinter(printer);

        //Disabling the Cross mark / Tick mark in PrinterPanel.
        panel.DisableViewPrinterPanel_Stuff();

        PrinterList.Add(panel);
    }






    //This is used to save printer preference & settings in backend's database.
    public void ClickedProceed()
    {
        //This is to make sure no other input can be pressed while the below lines of code are being executed.
        Scene5ManagerScript.EnableInvisiblePanel();


        List<Printer> SelectedPrinters = new();

        foreach (PrinterPanel Panel in PrinterList)
        {
            if (Panel.Printer != null && Panel.Printer.IsSelected)
            {
                SelectedPrinters.Add(Panel.Printer);
            }
        }


        //Saving printer preference & settings inside database.
        Scene5BackendScript.SaveApprovedPrinters
        (
            SelectedPrinters,
            Success =>
            {
                Scene5ManagerScript.DisableInvisiblePanel();

                if (!Success)
                {
                    Scene5ManagerScript.DisableInvisiblePanel();

                    Scene5ManagerScript.MessageText.text = "Failed to save approved printers.";
                    Scene5ManagerScript.EnableMessagePanel();

                    return;
                }

                //Saving printer preference inside Script1.
                Script1.SetPrinters(SelectedPrinters);

                Script1.PrintAllPrinterDetails();

                Scene5ManagerScript.DisableInvisiblePanel();

                Scene5ManagerScript.MessageText.text = "Printer selection has been updated.";
                Scene5ManagerScript.EnableMessagePanel();
            }
        );
    }







    //This functions is to go from UpdatePrinters page to Settings page.
    public void ClickedPreviousPage()
    {
        Scene5ManagerScript.DisableAllPanels();
        Scene5ManagerScript.EnableSettingsPanel();
    }
}