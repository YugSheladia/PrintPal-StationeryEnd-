//Script8

using TMPro;
using UnityEngine;

public class Print_FinalBill : MonoBehaviour
{
    [SerializeField] TMP_Text FinalPriceText;
    [SerializeField] TMP_Text OTPText;
    [SerializeField] TMP_Text OrderIDText;

    private float FinalBillAmount;

    public int OTP;
    public int OrderID;


    public void SetFinalBillAmount(float FinalBillAmount)
    {
        this.FinalBillAmount = FinalBillAmount;
        FinalPriceText.text = "$" + FinalBillAmount.ToString("F2");

        OrderIDText.text = "Go to Recent Orders.";
        OTPText.text = "Go to Recent Orders.";
    }


    public void SetFinalBillAmount(float FinalBillAmount, int ExistingOrderID, int ExistingOTP)
    {
        this.FinalBillAmount = FinalBillAmount;

        OrderID = ExistingOrderID;
        OTP = ExistingOTP;

        SetDetailsInUI();
    }


    private void SetDetailsInUI()
    {
        FinalPriceText.text = "`" + FinalBillAmount.ToString("F2");

        OrderIDText.text = OrderID.ToString();
        OTPText.text = OTP.ToString();
    }
}