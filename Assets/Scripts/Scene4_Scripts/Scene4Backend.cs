using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

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



    public void StartCoroutine_UpdateStationeryInfoInServer(UpdatedStationeryInfo UpdatedStationeryInfoObject)
    {
        StartCoroutine(UpdateStationeryInfoInServer(UpdatedStationeryInfoObject));
    }
    IEnumerator UpdateStationeryInfoInServer(UpdatedStationeryInfo UpdatedStationeryInfoObject)
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
            Debug.Log("Raw Server Response : \n" + ServerResponseJSON);

            Script1.StationeryInfoObject.StationeryName = UpdatedStationeryInfoObject.StationeryName;
            Script1.StationeryInfoObject.UPINumber      = UpdatedStationeryInfoObject.UPINumber;
            Script1.StationeryInfoObject.Email          = UpdatedStationeryInfoObject.Email;

            ViewProfileScript.UpdateProfileInfo();
            ViewProfileScript.UpdateUI();

            Scene4ManagerScript.DisableAllPanels();
            Scene4ManagerScript.EnableViewProfileInfoPanel();
        }
        else
        {
            Debug.LogError(request.error);
        }
    }




    public void StartCoroutine_GetStationeryInfoFromServer()
    {
        StartCoroutine(GetStationeryInfoFromServer());
    }
    IEnumerator GetStationeryInfoFromServer()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/stationeryInfo/" + Script1.StationeryInfoObject.PhoneNumber); 

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string ServerResponseJSON = request.downloadHandler.text;
            Debug.Log("Raw Server response = \n" + ServerResponseJSON);

            StationeryInfoWrapperObject = JsonUtility.FromJson<StationeryInfoWrapper>(ServerResponseJSON);

            if (StationeryInfoWrapperObject.Success)
            {
                Debug.Log("User info received");

                Script1.SetStationeryInfo(StationeryInfoWrapperObject.StationeryInfoObject);

                ViewProfileScript.UpdateUI();
            }
            else
            {
                Debug.LogError("User not found");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
}
