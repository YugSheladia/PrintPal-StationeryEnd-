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
            printer.IsSelected = true;

            printer.IsOnline = true;
            printer.IsBusy = false;
            printer.AutomaticPrinting = false;

            printer.SupportsDoubleSidedPrinting = approvedPrinter.SupportsDoubleSidedPrinting;
            printer.SupportsColorPrinting = approvedPrinter.SupportsColorPrinting;

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





    //==============================================================
    //Feature of Enabling / Disabling Automatic Printing has been transferred from Settings -> HomePage
    //Regardless, the below lines of code made Automatic Printing a preference, and connected it to the database, which is a legcay feature now, so this is retired. 
    //==============================================================
    /*
    [System.Serializable]
    public class AutomaticPrintingRequest
    {
        public int StationeryID;

        public bool AutomaticPrinting;
    }

    [System.Serializable]
    public class AutomaticPrintingResponse
    {
        public bool Success;

        public string Message;
    }


    public IEnumerator SetAutomaticPrinting
    (
        int StationeryID,
        bool AutomaticPrinting,
        System.Action<bool> OnComplete
    )
    {
        AutomaticPrintingRequest RequestObject = new AutomaticPrintingRequest();

        RequestObject.StationeryID = StationeryID;
        RequestObject.AutomaticPrinting = AutomaticPrinting;

        string Json = JsonUtility.ToJson(RequestObject);

        UnityWebRequest Request = new UnityWebRequest
        (
            "http://localhost:3000/stationery/automaticPrinting",
            "POST"
        );

        byte[] Body = System.Text.Encoding.UTF8.GetBytes(Json);

        Request.uploadHandler = new UploadHandlerRaw(Body);

        Request.downloadHandler = new DownloadHandlerBuffer();

        Request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(Request.error);

            OnComplete?.Invoke(false);

            yield break;
        }

        AutomaticPrintingResponse Response = JsonUtility.FromJson<AutomaticPrintingResponse>
        (
            Request.downloadHandler.text
        );

        if (!Response.Success)
        {
            Debug.LogError(Response.Message);

            OnComplete?.Invoke(false);

            yield break;
        }

        Debug.Log("Automatic printing updated.");

        OnComplete?.Invoke(true);
    }
    */

    /*
    [System.Serializable]
    public class GetAutomaticPrintingResponse
    {
        public bool Success;

        public bool AutomaticPrinting;

        public string Message;
    }
    public IEnumerator GetAutomaticPrinting
    (
        int StationeryID,
        System.Action<bool?> OnComplete
    )
    {
        UnityWebRequest Request = UnityWebRequest.Get
        (
            "http://localhost:3000/stationery/automaticPrinting/" +
            StationeryID
        );

        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(Request.error);

            OnComplete?.Invoke(null);

            yield break;
        }

        GetAutomaticPrintingResponse Response =
            JsonUtility.FromJson<GetAutomaticPrintingResponse>
            (
                Request.downloadHandler.text
            );

        if (Response == null || !Response.Success)
        {
            Debug.LogError(Response?.Message);

            OnComplete?.Invoke(null);

            yield break;
        }

        OnComplete?.Invoke(Response.AutomaticPrinting);
    }
    */
}
