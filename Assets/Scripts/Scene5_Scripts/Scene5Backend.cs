//namespaces =
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;






public class Scene5Backend : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene5Manager Scene5ManagerScript;



    
    //Server URL =
    public string ServerURL = "http://localhost:3000";







    //To get all printers connected to the PC using a server request.
    public IEnumerator GetPrinters(System.Action<Scene2Backend.PrinterInfo[]> OnComplete)
    {
        using (UnityWebRequest Request = UnityWebRequest.Get("http://localhost:3000/printers"))
        {
            yield return Request.SendWebRequest();

            if (Request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError
                (
                    "Failed to get printers: " +
                    Request.error
                );

                OnComplete?.Invoke(null);

                yield break;
            }

            string ResponseText = Request.downloadHandler.text;

            Debug.Log("Raw = " + ResponseText);

            Scene2Backend.PrintersResponse Response =
                JsonUtility.FromJson<Scene2Backend.PrintersResponse>(ResponseText);

            if (Response == null)
            {
                Debug.LogError("Printer response is null.");

                OnComplete?.Invoke(null);

                yield break;
            }

            if (!Response.success)
            {
                Debug.LogError(Response.message);

                OnComplete?.Invoke(null);

                yield break;
            }

            if (Response.printers == null)
            {
                Debug.LogError("Printers array is null.");

                OnComplete?.Invoke(null);

                yield break;
            }

            OnComplete?.Invoke(Response.printers);
        }
    }









    // To get printers saved in database - StationeryPrinters.
    public IEnumerator GetApprovedPrintersFromServer
    (
        int StationeryID,
        System.Action<bool> OnComplete
    )
    {
        UnityWebRequest request = UnityWebRequest.Get
        (
            "http://localhost:3000/stationery/printers/" + StationeryID
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);

            Scene5ManagerScript.MessageText.text = "Failed to retrieve approved printers\nTry again later.";

            OnComplete?.Invoke(false);
            yield break;
        }

        Debug.Log
        (
            "Raw Printer Response =\n" +
            request.downloadHandler.text
        );

        Scene2Backend.GetApprovedPrintersResponse response =
            JsonUtility.FromJson<Scene2Backend.GetApprovedPrintersResponse>
            (
                request.downloadHandler.text
            );

        if (!response.Success)
        {
            Debug.LogError("Failed to retrieve approved printers.");

            Scene5ManagerScript.MessageText.text = "Failed to retrieve approved printers\nTry again later.";

            OnComplete?.Invoke(false);
            yield break;
        }

        List<Printer> printers = new();

        foreach (Scene2Backend.ApprovedPrinter approvedPrinter in response.Printers)
        {
            Printer printer = new Printer();

            printer.Name = approvedPrinter.PrinterName;
            printer.StationeryPrinterID = approvedPrinter.StationeryPrinterID;

            printer.IsSelected = true;

            //IsOnline = true as this status is not decided by the server, but instead by the printer's actual activity.
            printer.IsOnline = true;

            //IsBusy = false as this status is not decided byt the server, but instead by the printer's actual activity.
            printer.IsBusy = false;

            //This status isn't used yet, and hence it kept false by default.
            printer.AutomaticPrinting = false;

            printer.SupportsDoubleSidedPrinting = approvedPrinter.SupportsDoubleSidedPrinting;
            printer.SupportsColorPrinting = approvedPrinter.SupportsColorPrinting;

            printer.PageCapacity = approvedPrinter.PageCapacity;
            printer.PagesLeft = approvedPrinter.PagesLeft;

            printers.Add(printer);
        }

        Script1.SetPrinters(printers);

        Debug.Log($"Retrieved {printers.Count} approved printers.");

        Script1.PrintAllPrinterDetails();

        OnComplete?.Invoke(true);
    }










    // To save the printers which were selecteed in 'Settings -> Update Printer' in database - StationeryPrinters.
    public void SaveApprovedPrinters
    (
        List<Printer> SelectedPrinters,
        System.Action<bool> OnComplete
    )
    {
        StartCoroutine
        (
            Coroutine_SaveApprovedPrinters
            (
                SelectedPrinters,
                OnComplete
            )
        );
    }

    IEnumerator Coroutine_SaveApprovedPrinters
    (
        List<Printer> SelectedPrinters,
        System.Action<bool> OnComplete
    )
    {
        Scene2Backend.SaveStationeryPrintersRequest Request = new Scene2Backend.SaveStationeryPrintersRequest();

        Request.StationeryID = Script1.StationeryInfoObject.StationeryID;

        Request.Printers = SelectedPrinters;

        string Json = JsonUtility.ToJson(Request);

        UnityWebRequest RequestObject = new UnityWebRequest
        (
            ServerURL + "/stationery/printers",
            "POST"
        );

        byte[] Body = Encoding.UTF8.GetBytes(Json);

        RequestObject.uploadHandler = new UploadHandlerRaw(Body);
        RequestObject.downloadHandler = new DownloadHandlerBuffer();

        RequestObject.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return RequestObject.SendWebRequest();

        if (RequestObject.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(RequestObject.error);

            OnComplete?.Invoke(false);

            yield break;
        }

        Scene2Backend.SaveStationeryPrintersResponse Response =
        JsonUtility.FromJson<Scene2Backend.SaveStationeryPrintersResponse>
        (
            RequestObject.downloadHandler.text
        );

        if (!Response.Success)
        {
            Debug.LogError(Response.Message);

            OnComplete?.Invoke(false);

            yield break;
        }

        Debug.Log("Approved printers saved.");

        OnComplete?.Invoke(true);
    }
}
