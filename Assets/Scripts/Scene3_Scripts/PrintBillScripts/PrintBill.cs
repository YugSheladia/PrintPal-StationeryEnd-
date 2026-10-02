//namespaces =
using TMPro;
using UnityEngine;




public class PrintBill : MonoBehaviour
{
    //Bill UI =
    [SerializeField] public TMP_Text PrintNumberText;
    [SerializeField] TMP_Text PDFNameText;

    [SerializeField] TMP_Text PagesValueText;
    [SerializeField] TMP_Text CopiesValueText;
    [SerializeField] TMP_Text ColorValueText;
    [SerializeField] TMP_Text SideSelectionValueText;
    [SerializeField] TMP_Text PagesPerSheetText;

    [SerializeField] TMP_Text Price;
    [SerializeField] TMP_Text PriceCalculation;

    [SerializeField] TMP_Text OrientationText;
    [SerializeField] TMP_Text PageSelectionText;
    [SerializeField] TMP_Text CollationText;
    [SerializeField] TMP_Text MarginText;




    //Reference to PrintDetails object
    PrintDetails PrintDetailsObject;



    //Random variables =
    float UnitPrice;
    public int EffectiveSheets;
    public float TotalPrice;





    //Function used for obtaining reference to PrintDetails object from OrderStatus script.
    public void SetBillDetailsInBillScript(PrintDetails PrintDetailsObject)
    {
        this.PrintDetailsObject = PrintDetailsObject;

        SetBillDetailsInUI();
    }

    //Updating UI according to PrintDetails object.
    private void SetBillDetailsInUI()
    {
        PrintNumberText.text = "#" + (PrintDetailsObject.PrintNumber + 1);

        PDFNameText.text = PrintDetailsObject.PDFName;

        PagesValueText.text = PrintDetailsObject.Pages.ToString();
        CopiesValueText.text = PrintDetailsObject.Copies.ToString();

        ColorValueText.text =
            PrintDetailsObject.Color == ColorEnum.BlackAndWhite
            ? "Black & White"
            : "Color";

        SideSelectionValueText.text = PrintDetailsObject.SideSelection.ToString();
        PagesPerSheetText.text = PrintDetailsObject.PagesPerSheet.ToString();

        Price.text = "`" + PrintDetailsObject.BillAmount.ToString("F2");

        if(PrintDetailsObject.SideSelection == SideSelectionEnum.SingleSided)
        {
            UnitPrice = PrintDetailsObject.Color == ColorEnum.BlackAndWhite
                ? 2.5f
                : 5.5f;
        }
        else
        {
            UnitPrice = PrintDetailsObject.Color == ColorEnum.BlackAndWhite
                ? 4.5f
                : 10.5f;
        }

        int BillableSheets =
            PrintDetailsObject.SideSelection == SideSelectionEnum.SingleSided
            ? PrintDetailsObject.EffectiveSheets
            : Mathf.CeilToInt(PrintDetailsObject.EffectiveSheets / 2f);

        PriceCalculation.text =
            BillableSheets + " Sheets x " +
            PrintDetailsObject.Copies + " Copies x ` " +
            UnitPrice.ToString("F2");

        MarginText.text = PrintDetailsObject.Margin.ToString();
        OrientationText.text = PrintDetailsObject.Orientation.ToString();
        PageSelectionText.text = PrintDetailsObject.PageSelection.ToString();
        CollationText.text = PrintDetailsObject.CollatedOrNot.ToString();
    }
}