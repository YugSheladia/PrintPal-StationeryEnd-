using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;





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

        Debug.Log("Raw server response = \n" + ServerResponse);

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
                    (Result) =>
                    {
                        Success = Result;
                    }
                )
            );

            if (Success)
            {
                OnComplete?.Invoke(true, "");
            }
            else
            {
                OnComplete?.Invoke
                (
                    false,
                    "Failed to retrieve stationery information."
                );
            }
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

            Debug.LogError
            (
                "Login failed: "
                + request.responseCode
                + " | "
                + ResponseObject?.Message
            );

            OnComplete?.Invoke(false, ErrorMessage);
        }
    }
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

            Debug.Log("Raw Server response = \n" + ServerResponseJSON);

            StationeryInfoWrapper StationeryInfoWrapperObject =
                JsonUtility.FromJson<StationeryInfoWrapper>(ServerResponseJSON);

            if (StationeryInfoWrapperObject.Success)
            {
                Debug.Log("Stationery info received");

                Script1.SetStationeryInfo
                (
                    StationeryInfoWrapperObject.StationeryInfoObject
                );

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









    [System.Serializable]
    public class PrinterInfo
    {
        public string name;
        public string status;
        public bool isDefault;
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
        using (UnityWebRequest Request =
            UnityWebRequest.Get("http://localhost:3000/printers"))
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

}
