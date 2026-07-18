using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


//To verify if the updated stationery info ( updated through profile info -> view profile -> edit ) has reacehd the server or not.
public class UpdatedStationeryInfoWrapper
{
    public string Message;
    public bool Success;
}




public class Scene4Backend : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] ViewProfile ViewProfileScript;
 
    StationeryInfoWrapper StationeryInfoWrapperObject = new StationeryInfoWrapper();


    //To update a stationery's profile info.
    public void StartCoroutine_UpdateStationeryInfoInServer ( UpdatedStationeryInfo UpdatedStationeryInfoObject )
    {
        StartCoroutine
        (
            UpdateStationeryInfoInServer
            (
                UpdatedStationeryInfoObject,
                null
            )
        );
    }
    public IEnumerator UpdateStationeryInfoInServer ( UpdatedStationeryInfo UpdatedStationeryInfoObject, System.Action<bool> OnComplete )
    {
        string UpdatedStationeryInfoJSON = JsonUtility.ToJson(UpdatedStationeryInfoObject);

        byte[] UpdatedStationeryInfoRaw = System.Text.Encoding.UTF8.GetBytes(UpdatedStationeryInfoJSON);

        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/update-stationeryUser",
                                                        "POST"
                                                     );

        request.uploadHandler = new UploadHandlerRaw(UpdatedStationeryInfoRaw);

        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
                                    "Content-Type",
                                    "application/json"
                                );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log
            (
                "Raw Server Response : \n"
                + ServerResponseJSON
            );

            UpdatedStationeryInfoWrapper Response =
                JsonUtility.FromJson<UpdatedStationeryInfoWrapper>
                (
                    ServerResponseJSON
                );

            if (Response == null)
            {
                Debug.LogError("JSON Parsing Failed.");

                OnComplete?.Invoke(false);

                yield break;
            }

            if (!Response.Success)
            {
                Debug.LogError
                (
                    "Server Error: "
                    + Response.Message
                );

                OnComplete?.Invoke(false);

                yield break;
            }

            Script1.SetStationeryInfo2(UpdatedStationeryInfoObject);

            OnComplete?.Invoke(true);
        }
        else
        {
            Debug.LogError(request.error);

            OnComplete?.Invoke(false);

            Scene4ManagerScript.DisableInvisiblePanel();

            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();

            yield break;
        }
    }



    //To recieve a stationery's info.
    public IEnumerator GetStationeryInfoFromServer ( System.Action<bool> OnComplete )
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/stationeryInfo/" + Script1.StationeryInfoObject.PhoneNumber); 

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;
            Debug.Log("Raw Server response = \n" + ServerResponseJSON);

            StationeryInfoWrapperObject =
            JsonUtility.FromJson<StationeryInfoWrapper>
            (
                ServerResponseJSON
            );

            if (StationeryInfoWrapperObject == null)
            {
                Debug.LogError("JSON Parsing Failed.");

                OnComplete?.Invoke(false);

                yield break;
            }

            if (!StationeryInfoWrapperObject.Success)
            {
                Debug.LogError
                (
                    "Server Error: "
                    + StationeryInfoWrapperObject.Message
                );

                OnComplete?.Invoke(false);

                yield break;
            }

            Debug.Log("Stationery info received.");

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

            yield break;
        }
    }
}
