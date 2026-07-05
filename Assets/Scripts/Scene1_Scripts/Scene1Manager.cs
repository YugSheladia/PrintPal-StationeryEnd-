using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Scene1Manager : MonoBehaviour
{
    [SerializeField] private GameObject BlankPanel;

    [SerializeField] private GameObject LoadingScreenPanel;

    [SerializeField] private float Delay = 5f;



    private void Start()
    {
        DisableAllPanels();
        EnableLoadingScreen();

        StartCoroutine(LoadNextPanelAfterDelay());
    }
    IEnumerator LoadNextPanelAfterDelay()
    {
        yield return new WaitForSeconds(Delay);

        SceneManager.LoadScene("Scene2_SetupProfileInfo");
    }



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