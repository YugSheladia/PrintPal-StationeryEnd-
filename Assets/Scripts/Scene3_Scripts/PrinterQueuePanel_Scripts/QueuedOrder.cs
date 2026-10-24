//namespaces =
using UnityEngine;
using TMPro;





public class QueuedOrder : MonoBehaviour
{
    //UI stuff =
    [SerializeField] public TMP_Text OrderID;

    [SerializeField] public TMP_Text TimeLeft;



    private void Start()
    {
        TimeLeft.text = "Processing...";
    }



    public void Set_TimeLeft(int TimeLeft)
    {
        int TimeLeft_int = (int)(TimeLeft / 60);

        if(TimeLeft_int == 0)
        {
            this.TimeLeft.text = "Done";

            return;
        }

        this.TimeLeft.text = "TimeLeft = " + TimeLeft_int;
    }    
}
