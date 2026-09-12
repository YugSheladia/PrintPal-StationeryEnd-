//namespace = 
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;





public class Scene1Manager : MonoBehaviour
{
    //Panels of Scene1 =
    [SerializeField] private GameObject BlankPanel;
    [SerializeField] private GameObject LoadingScreenPanel;

    //Delay = used to create a make believe loading screen.
    [SerializeField] private float Delay = 5f;


    




    private void Start()
    {
        DisableAllPanels();
        EnableLoadingScreen();

        StartCoroutine(LoadScene2AfterDelay());
    }

    //LoadSceneAfterDelay() = Waits for Delay seconds, & switches from Scene1 -> Scene2.
    IEnumerator LoadScene2AfterDelay()
    {
        yield return new WaitForSeconds(Delay);

        SceneManager.LoadScene("Scene2_SetupProfileInfo");
    }







    //Enabling & Disabling panels =
    public void DisableAllPanels()
    {
        DisableBlankPanel();
        DisableLoadingScreen();
    }
    public void EnableBlankPanel()
    {
        BlankPanel.SetActive(true);
    }
    public void DisableBlankPanel()
    {
        BlankPanel?.SetActive(false);
    }
    public void EnableLoadingScreen()
    {
        LoadingScreenPanel.SetActive(true);
    }
    public void DisableLoadingScreen()
    {
        LoadingScreenPanel.SetActive(false);
    }
}