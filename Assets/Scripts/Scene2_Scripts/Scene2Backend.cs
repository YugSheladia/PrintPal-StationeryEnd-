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



    public StationeryInfoWrapper StationeryInfoWrapperObject = new StationeryInfoWrapper();










    //Login verification.
    public void VerifyLoginInfo(LoginInfo LoginInfoObject)
    {
        StartCoroutine(SubmitLoginInfoToServer(LoginInfoObject));
    }
    IEnumerator SubmitLoginInfoToServer(LoginInfo LoginInfoObject)
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
            yield return StartCoroutine
            (
                GetStationeryInfoFromServer
                (
                    ResponseObject.StationeryInfoObject.PhoneNumber
                )
            );

            Debug.Log("Unity - Successfully logged in.");
        }
        else
        {
            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableLoginPanel();

            if (request.responseCode == 404)
            {
                Scene2ManagerScript.MessageText.text =
                    "Phone number doesn't exist.";
            }
            else if (request.responseCode == 401)
            {
                Scene2ManagerScript.MessageText.text =
                    "Incorrect password.";
            }
            else if (request.responseCode == 500)
            {
                Scene2ManagerScript.MessageText.text =
                    "Database error. Please try again.";
            }
            else
            {
                Scene2ManagerScript.MessageText.text =
                    "Something went wrong. Please try again.";
            }

            Scene2ManagerScript.EnableMessagePanel();

            Debug.LogError
            (
                "Login failed: "
                + request.responseCode
                + " | "
                + ResponseObject?.Message
            );
        }
    }
    IEnumerator GetStationeryInfoFromServer(string PhoneNumber)
    {
        UnityWebRequest request = UnityWebRequest.Get(  
                                                        "http://localhost:3000/stationeryInfo/" 
                                                        + PhoneNumber
                                                     );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;
            Debug.Log("Raw Server response = \n" + ServerResponseJSON);

            StationeryInfoWrapper StationeryInfoWrapperObject = JsonUtility.FromJson<StationeryInfoWrapper>(ServerResponseJSON);

            if (StationeryInfoWrapperObject.Success)
            {
                Debug.Log("Stationery info received");

                Script1.SetStationeryInfo(StationeryInfoWrapperObject.StationeryInfoObject);

                SceneManager.LoadScene("Scene3_HomePage");
            }
            else
            {
                Debug.LogError("User not found");

                Scene2ManagerScript.MessageText.text = "User not found.\nRestart the app if you think this is a mistake";
                Scene2ManagerScript.EnableMessagePanel();

                Scene2ManagerScript.DisableAllPanels();
                Scene2ManagerScript.EnableLoginPanel();
            }
        }
        else
        {
            Debug.LogError(request.error);

            Scene2ManagerScript.MessageText.text = "An error occured.\nRestart the app if you think this is a mistake";
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableLoginPanel();
        }
    }






    //Used for checking phone availability verification.
    [System.Serializable]
    public class PhoneNumberAvailabilityResponse
    {
        public bool Success;
        public string Message;
    }
    public void CheckPhoneNumberAvailability(StationeryPhoneNumber StationeryPhoneNumberObject)
    {
        StartCoroutine(SubmitPhoneNumberToServer(StationeryPhoneNumberObject));
    }
    IEnumerator SubmitPhoneNumberToServer(StationeryPhoneNumber StationeryPhoneNumberObject)
    {
        string StationeryPhoneNumberJSON = JsonUtility.ToJson(StationeryPhoneNumberObject);

        byte[] StationeryPhoneNumberRaw = System.Text.Encoding.UTF8.GetBytes(StationeryPhoneNumberJSON);

        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/stationeryPhoneNumberAvailability",
                                                        "POST"
                                                     );

        request.uploadHandler = new UploadHandlerRaw(StationeryPhoneNumberRaw);

        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
                                    "Content-Type",
                                    "application/json"
                                );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;
            Debug.Log("Raw Server Response : \n" + ServerResponseJSON);

            PhoneNumberAvailabilityResponse ServerResponseObject = JsonUtility.FromJson<PhoneNumberAvailabilityResponse>(ServerResponseJSON);

            if (ServerResponseObject.Success == true)
            {
                Debug.Log("Phone Number Available");

                StationeryInfoWrapperObject.StationeryInfoObject.PhoneNumber = StationeryPhoneNumberObject.PhoneNumber;

                Scene2ManagerScript.DisableAllPanels();
                Scene2ManagerScript.EnableSetPasswordPanel();
            }
            else
            {
                Debug.Log("Number already taken.");

                Scene2ManagerScript.MessageText.text = "Phone number already taken.";
                Scene2ManagerScript.EnableMessagePanel();

                Scene2ManagerScript.DisableAllPanels();
                Scene2ManagerScript.EnableSetPhoneNumberPanel();
            }
        }
        else
        {
            Debug.LogError(request.error);

            Scene2ManagerScript.MessageText.text = "An error occured.\nRestart the app if you think this is a mistake";
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetPhoneNumberPanel();
        }
    }






    //Creating a stationery account.
    public void VerifyStationeryInfo()
    {
        StartCoroutine(SubmitStationeryInfoToServer());
    }
    IEnumerator SubmitStationeryInfoToServer()
    {
        string StationeryInfoObjectJSON = JsonUtility.ToJson(StationeryInfoWrapperObject.StationeryInfoObject);

        byte[] UserInfoObjectRaw = System.Text.Encoding.UTF8.GetBytes(StationeryInfoObjectJSON);

        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/create-stationeryUser",
                                                        "POST"
                                                     );

        request.uploadHandler = new UploadHandlerRaw(UserInfoObjectRaw);

        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
                                    "Content-Type",
                                    "application/json"
                                );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log("Raw Server Response : \n" + ServerResponseJSON);

            StationeryInfoVerificationResponse ResponseObject = 
                JsonUtility.FromJson<StationeryInfoVerificationResponse>(ServerResponseJSON);

            StationeryInfoWrapperObject.StationeryInfoObject.StationeryID = ResponseObject.StationeryID;

            Script1.SetStationeryInfo(StationeryInfoWrapperObject.StationeryInfoObject);

            SceneManager.LoadScene("Scene3_HomePage");
        }
        else
        {
            Debug.LogError(request.error);

            Scene2ManagerScript.MessageText.text = "An error occured.\nRestart the app if you think this is a mistake";
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetProfileInfoPanel();
        }
    }

}
