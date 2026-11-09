//namespaces =
using UnityEngine;
using TMPro;





public class QueuedOrder : MonoBehaviour
{
    PrintJob Job;






    //UI stuff =
    [SerializeField] public TMP_Text OrderID;

    [SerializeField] public TMP_Text TimeLeft;






    public void SetPrintJob(PrintJob Job)
    {
        this.Job = Job;
    }



    



    public void UpdateUI()
    {
        //Debug.Log("UpdateUI for QueuedOrder of JobID = " + Job.OrderID);
        //Debug.Log("TimeLeft = " + Job.TimeLeft);

        TimeLeft.text = Job.TimeLeft.ToString() + " secs left";
    }
}
