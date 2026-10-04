//namespaces = 
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;





//For getting stationery details from database after login verification.
[System.Serializable]
public class StationeryInfoWrapper
{
    public bool             Success;
    public string           Message;
    public StationeryInfo   StationeryInfoObject;
}

//Used for login verification.
[System.Serializable]
public class LoginInfoVerificationResponse
{
    public bool Success;
    public string Message;
    public StationeryInfo StationeryInfoObject;
}

//For creating a Stationery account
[System.Serializable]
public class StationeryInfoVerificationResponse
{
    public bool Success;
    public string Message;

    public int StationeryID;
}









public class Scene2Backend : MonoBehaviour
{
    //ServerURL = 
    public string ServerURL = "http://localhost:3000";


    //Script references =
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] SelectPrinters SelectPrintersScript;


    
    public StationeryInfoWrapper StationeryInfoWrapperObject = new StationeryInfoWrapper();







    //Login verification.
    public IEnumerator SubmitLoginInfoToServer
    (
        LoginInfo LoginInfoObject,
        System.Action<bool, string> OnComplete
    )
    {
        string LoginInfoJSON = JsonUtility.ToJson(LoginInfoObject);

        byte[] LoginInfoRaw = System.Text.Encoding.UTF8.GetBytes(LoginInfoJSON);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/stationeryLogin",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(LoginInfoRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        string ServerResponse = request.downloadHandler.text;

        Debug.Log("Raw server response #1 = \n" + ServerResponse);

        LoginInfoVerificationResponse ResponseObject =
            JsonUtility.FromJson<LoginInfoVerificationResponse>(ServerResponse);

        if (request.responseCode == 200 && ResponseObject.Success)
        {
            bool Success = false;

            yield return StartCoroutine
            (
                GetStationeryInfoFromServer
                (
                    ResponseObject.StationeryInfoObject.PhoneNumber,
                    Result =>
                    {
                        Success = Result;
                    }
                )
            );

            if (!Success)
            {
                OnComplete?.Invoke
                (
                    false,
                    "Failed to retrieve stationery information."
                );

                yield break;
            }

            bool PrintersRetrieved = false;

            yield return StartCoroutine
            (
                GetApprovedPrintersFromServer
                (
                    Script1.StationeryInfoObject.StationeryID,
                    Result =>
                    {
                        PrintersRetrieved = Result;
                    }
                )
            );

            if (!PrintersRetrieved)
            {
                OnComplete?.Invoke
                (
                    false,
                    "Failed to retrieve approved printers."
                );

                yield break;
            }

            OnComplete?.Invoke(true, "");
        }
        else
        {
            string ErrorMessage;

            if (request.responseCode == 404)
            {
                ErrorMessage = "Phone number doesn't exist.";
            }
            else if (request.responseCode == 401)
            {
                ErrorMessage = "Incorrect password.";
            }
            else if (request.responseCode == 500)
            {
                ErrorMessage = "Database error. Please try again.";
            }
            else
            {
                ErrorMessage = "Something went wrong. Please try again.";
            }

            Debug.Log
            (
                "Login failed: "
                + request.responseCode
                + " | "
                + ResponseObject?.Message
            );

            OnComplete?.Invoke(false, ErrorMessage);
        }
    }



    //On succesful login, this is to retrive info about a stationery.
    IEnumerator GetStationeryInfoFromServer
    (
        string PhoneNumber,
        System.Action<bool> OnComplete
    )
    {
        UnityWebRequest request = UnityWebRequest.Get
        (
            "http://localhost:3000/stationeryInfo/" + PhoneNumber
        );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log("Raw Server response #2 = \n" + ServerResponseJSON); 

            StationeryInfoWrapper StationeryInfoWrapperObject =
                JsonUtility.FromJson<StationeryInfoWrapper>(ServerResponseJSON);

            if (StationeryInfoWrapperObject.Success)
            {
                Debug.Log("Stationery info received");

                Script1.SetStationeryInfo
                (
                    StationeryInfoWrapperObject.StationeryInfoObject
                );

                StationeryInfoWrapperObject.StationeryInfoObject.PrintStationeryDetails();

                OnComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("User not found");

                OnComplete?.Invoke(false);
            }
        }
        else
        {
            Debug.LogError(request.error);

            OnComplete?.Invoke(false);
        }
    }







    //On succesful login, this is to retrieve Stationery approved Printers from the Server.
    [System.Serializable]
    public class ApprovedPrinter
    {
        public int StationeryPrinterID;
        public int StationeryID;

        public string PrinterName;

        public bool SupportsDoubleSidedPrinting;
        public bool SupportsColorPrinting;

        public int PageCapacity;
        public int PagesLeft;
    }

    [System.Serializable]
    public class GetApprovedPrintersResponse
    {
        public bool Success;

        public ApprovedPrinter[] Printers;
    }

    IEnumerator GetApprovedPrintersFromServer
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

            Scene2ManagerScript.MessageText.text = "Failed to retrieve approved printers\nTry again later.";

            OnComplete?.Invoke(false);
            yield break;
        }

        Debug.Log
        (
            "Raw Printer Response =\n" +
            request.downloadHandler.text
        );

        GetApprovedPrintersResponse response =
            JsonUtility.FromJson<GetApprovedPrintersResponse>
            (
                request.downloadHandler.text
            );

        if (!response.Success)
        {
            Debug.LogError("Failed to retrieve approved printers.");

            Scene2ManagerScript.MessageText.text = "Failed to retrieve approved printers\nTry again later.";

            OnComplete?.Invoke(false);
            yield break;
        }

        if (response.Printers == null)
        {
            Debug.LogError("Approved printer list is null.");

            Scene2ManagerScript.MessageText.text = "Failed to retrieve approved printers\nTry again later.";

            OnComplete?.Invoke(false);
            yield break;
        }

        List<Printer> printers = new();

        foreach (ApprovedPrinter approvedPrinter in response.Printers)
        {
            Printer printer = new Printer();

            printer.Name = approvedPrinter.PrinterName;
            printer.StationeryPrinterID = approvedPrinter.StationeryPrinterID;

            //IsSelected = true as the printer as already been selected in the past, and hence it came in the list of Approved printers in the first place.
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

        PrintManager.Instance?.RefreshWorkersFromApprovedPrinters();

        Debug.Log($"Retrieved {printers.Count} approved printers.");

        Script1.PrintAllPrinterDetails();

        OnComplete?.Invoke(true);
    }







    //Used for checking phone availability verification.
    [System.Serializable]
    public class PhoneNumberAvailabilityResponse
    {
        public bool Success;
        public string Message;
    }
    public IEnumerator SubmitPhoneNumberToServer
    (
        StationeryPhoneNumber StationeryPhoneNumberObject,
        System.Action<bool, string> OnComplete
    )
    {
        string StationeryPhoneNumberJSON = JsonUtility.ToJson(StationeryPhoneNumberObject);

        byte[] StationeryPhoneNumberRaw = System.Text.Encoding.UTF8.GetBytes(StationeryPhoneNumberJSON);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/stationeryPhoneNumberAvailability",
            "POST"
        );

        request.uploadHandler =
            new UploadHandlerRaw(StationeryPhoneNumberRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log("Raw Server Response :\n" + ServerResponseJSON);

            PhoneNumberAvailabilityResponse ServerResponseObject =
                JsonUtility.FromJson<PhoneNumberAvailabilityResponse>
                (
                    ServerResponseJSON
                );

            if (ServerResponseObject.Success)
            {
                Debug.Log("Phone Number Available");

                StationeryInfoWrapperObject
                    .StationeryInfoObject
                    .PhoneNumber = StationeryPhoneNumberObject.PhoneNumber;

                OnComplete?.Invoke(true, "");
            }
            else
            {
                Debug.Log("Number already taken.");

                OnComplete?.Invoke
                (
                    false,
                    "Phone number already taken."
                );
            }
        }
        else
        {
            Debug.LogError(request.error);

            OnComplete?.Invoke
            (
                false,
                "An error occurred.\nRestart the app if you think this is a mistake."
            );
        }
    }









    //Creating a stationery account.
    public IEnumerator SubmitStationeryInfoToServer
    (
        System.Action<bool, string> OnComplete
    )
    {
        string StationeryInfoObjectJSON =
            JsonUtility.ToJson(StationeryInfoWrapperObject.StationeryInfoObject);

        byte[] UserInfoObjectRaw =
            System.Text.Encoding.UTF8.GetBytes(StationeryInfoObjectJSON);

        UnityWebRequest request = new UnityWebRequest
        (
            "http://localhost:3000/create-stationeryUser",
            "POST"
        );

        request.uploadHandler = new UploadHandlerRaw(UserInfoObjectRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader
        (
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log("Raw Server Response :\n" + ServerResponseJSON);

            StationeryInfoVerificationResponse ResponseObject =
                JsonUtility.FromJson<StationeryInfoVerificationResponse>
                (
                    ServerResponseJSON
                );

            StationeryInfoWrapperObject.StationeryInfoObject.StationeryID =
                ResponseObject.StationeryID;

            Script1.SetStationeryInfo
            (
                StationeryInfoWrapperObject.StationeryInfoObject
            );

            OnComplete?.Invoke(true, "");
        }
        else
        {
            Debug.LogError(request.error);

            OnComplete?.Invoke
            (
                false,
                "An error occurred.\nRestart the app if you think this is a mistake."
            );
        }
    }








    //To get all the printers connected to the device using a server request.

    [System.Serializable]
    public class PrinterInfo
    {
        public string name;
        
        //public string status;
        //public bool isDefault;
    }

    [System.Serializable]
    public class PrintersResponse
    {
        public bool success;
        public string message;
        public PrinterInfo[] printers;
    }

    public IEnumerator GetPrinters(System.Action<PrinterInfo[]> OnComplete)
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

            Debug.Log("Raw = " +  ResponseText);

            PrintersResponse Response =
                JsonUtility.FromJson<PrintersResponse>(ResponseText);

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







    // To save printer preference in Unity.
    [System.Serializable]
    public class SaveStationeryPrintersRequest
    {
        public int StationeryID;

        public List<Printer> Printers = new List<Printer>();
    }

    [System.Serializable]
    public class SaveStationeryPrintersResponse
    {
        public bool Success;

        public string Message;
    }

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
        SaveStationeryPrintersRequest Request = new SaveStationeryPrintersRequest();

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

        SaveStationeryPrintersResponse Response =
        JsonUtility.FromJson<SaveStationeryPrintersResponse>
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
