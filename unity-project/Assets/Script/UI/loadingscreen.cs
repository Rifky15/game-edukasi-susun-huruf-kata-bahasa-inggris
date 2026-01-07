using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    public float loadingTime = 3.8f; // Waktu loading dalam detik

    void Start()
    {
        StartCoroutine(LoadBootScene());
    }

    IEnumerator LoadBootScene()
    {
        yield return new WaitForSeconds(loadingTime);

        // Pindah ke BootScene tanpa fade (karena fade ada di BootScene)
        SceneManager.LoadScene("BootScene");
    }
}
