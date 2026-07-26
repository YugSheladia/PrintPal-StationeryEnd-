using UnityEngine;
using TMPro;

public class RevenuePanel : MonoBehaviour
{
    [SerializeField] public TMP_Text DateValue;
    [SerializeField] public TMP_Text RevenueValue;
    [SerializeField] public TMP_Text TotalOrdersValue;
    [SerializeField] public TMP_Text TotalCompletedOrdersValue;
    [SerializeField] public TMP_Text TotalPagesPrintedValue;


    public void SetRevenuePanelInfo(string Date,float Revenue, int TotalOrders, int TotalCompletedOrders, int TotalPagesPrinted)
    {
        DateValue.text = Date;
        RevenueValue.text = Revenue.ToString();
        TotalOrdersValue.text = TotalOrders.ToString();
        TotalCompletedOrdersValue.text = TotalCompletedOrders.ToString();
        TotalPagesPrintedValue.text = TotalPagesPrinted.ToString();
    }
}
