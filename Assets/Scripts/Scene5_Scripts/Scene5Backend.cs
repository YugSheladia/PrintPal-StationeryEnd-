using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



public class Scene5Backend : MonoBehaviour
{
    [SerializeField] Scene5Manager Scene5ManagerScript;






    public string ServerURL = "http://localhost:3000";







    //To get all the connected printers using a server request.
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









    // To get saved stationery printers.
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
            printer.IsSelected = true;

            // These will be updated later after querying Windows.
            printer.IsOnline = false;
            printer.IsBusy = false;
            printer.AutomaticPrinting = false;

            printers.Add(printer);
        }

        Script1.SetPrinters(printers);

        Debug.Log($"Retrieved {printers.Count} approved printers.");

        Script1.PrintAllPrinterDetails();

        OnComplete?.Invoke(true);
    }



    






    // To save printer preference in server.
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
