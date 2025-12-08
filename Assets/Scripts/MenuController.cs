using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string arScannerScene = "SampleScene";
    [SerializeField] private string collectionScene = "CollectionScene";
    [SerializeField] private string aboutUsScene = "AboutUsScene";

    public void OpenARScanner()
    {
        SceneManager.LoadScene(arScannerScene);
    }

    public void OpenCollection()
    {
        SceneManager.LoadScene(collectionScene);
    }

    public void OpenAboutUs()
    {
        SceneManager.LoadScene(aboutUsScene);
    }

    public void ExitOrLogout()
    {
        //Add Firebase Auth sign-out here.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
