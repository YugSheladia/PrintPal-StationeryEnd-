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
        Debug.Log("1");
        string UpdatedStationeryInfoJSON = JsonUtility.ToJson(UpdatedStationeryInfoObject);

        Debug.Log("2");
        byte[] UpdatedStationeryInfoRaw = System.Text.Encoding.UTF8.GetBytes(UpdatedStationeryInfoJSON);

        Debug.Log("3");
        UnityWebRequest request = new UnityWebRequest(
                                                        "http://localhost:3000/update-stationeryUser",
                                                        "POST"
                                                     );

        Debug.Log("4");
        request.uploadHandler = new UploadHandlerRaw(UpdatedStationeryInfoRaw);

        Debug.Log("5");
        request.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log("6");
        request.SetRequestHeader(
                                    "Content-Type",
                                    "application/json"
                                );

        Debug.Log("7");
        yield return request.SendWebRequest();

        Debug.Log("8");
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("9");
            string ServerResponseJSON = request.downloadHandler.text;

            Debug.Log("10");
            Debug.Log
            (
                "Raw Server Response : \n"
                + ServerResponseJSON
            );

            Debug.Log("11");
            UpdatedStationeryInfoWrapper Response =
                JsonUtility.FromJson<UpdatedStationeryInfoWrapper>
                (
                    ServerResponseJSON
                );

            Debug.Log("12");
            if (Response == null)
            {
                Debug.Log("13");
                Debug.LogError("JSON Parsing Failed.");

                Debug.Log("14");
                OnComplete?.Invoke(false);

                Debug.Log("15");
                yield break;
            }

            Debug.Log("16");
            if (!Response.Success)
            {
                Debug.Log("17");
                Debug.LogError
                (
                    "Server Error: "
                    + Response.Message
                );

                Debug.Log("18");
                OnComplete?.Invoke(false);

                Debug.Log("19");
                yield break;
            }

            Debug.Log("20");
            Script1.StationeryInfoObject.StationeryName =
                UpdatedStationeryInfoObject.StationeryName;

            Debug.Log("21");
            Script1.StationeryInfoObject.UPINumber =
                UpdatedStationeryInfoObject.UPINumber;

            Debug.Log("22");
            Script1.StationeryInfoObject.Email =
                UpdatedStationeryInfoObject.Email;


            Debug.Log("23");
            OnComplete?.Invoke(true);
        }
        else
        {
            Debug.Log("24");
            Debug.LogError(request.error);

            Debug.Log("25");
            OnComplete?.Invoke(false);

            Debug.Log("26");
            Scene4ManagerScript.DisableInvisiblePanel();

            Debug.Log("27");
            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();
            
            Debug.Log("28");
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
