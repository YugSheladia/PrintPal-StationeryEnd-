using UnityEngine;
using TMPro;

public class PrinterStatusPanel : MonoBehaviour
{
    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterStatus;

    [SerializeField] public GameObject PrinterNotWorking;
    [SerializeField] public GameObject PrinterWorking;
}
