//namespaces =
using UnityEngine;
using TMPro;







public class PrinterStatusPanel : MonoBehaviour
{
    //UI stuff =
    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterStatus;




    //Here =
    //PrinterNotWorking is a reference to a red cross, &
    //PrinterWorking is a reference to a green tick.
    [SerializeField] public GameObject PrinterNotWorking;
    [SerializeField] public GameObject PrinterWorking;
}
