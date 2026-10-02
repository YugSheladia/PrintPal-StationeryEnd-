//namespaces =
using TMPro;
using UnityEngine;





public class Print_FinalBill : MonoBehaviour
{
    //UI stuff =
    [SerializeField] TMP_Text FinalPriceText;
    [SerializeField] TMP_Text OTPText;
    [SerializeField] TMP_Text OrderIDText;




    //Random variables =
    private float FinalBillAmount;

    public int OTP;
    public int OrderID;



    //Pretty sure this function is useless. It is useful only for Userend.
    public void SetFinalBillAmount(float FinalBillAmount)
    {
        this.FinalBillAmount = FinalBillAmount;
        FinalPriceText.text = "$" + FinalBillAmount.ToString("F2");

        OrderIDText.text = "Go to Recent Orders.";
        OTPText.text = "Go to Recent Orders.";
    }




    //Called by Orderstatus script -> CreateFinalBill()
    public void SetFinalBillAmount(float FinalBillAmount, int ExistingOrderID, int ExistingOTP)
    {
        this.FinalBillAmount = FinalBillAmount;

        OrderID = ExistingOrderID;
        OTP = ExistingOTP;

        SetDetailsInUI();
    }

    //This function is called by SetFinalBillAmount.
    //It is for updating UI of FinalBill.
    private void SetDetailsInUI()
    {
        FinalPriceText.text = "`" + FinalBillAmount.ToString("F2");

        OrderIDText.text = OrderID.ToString();
        OTPText.text = OTP.ToString();
    }
}