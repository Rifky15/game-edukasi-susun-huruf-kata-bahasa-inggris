using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class BootSceneManager : MonoBehaviour
{
    [Header("Loading Panel (opsional)")]
    public CanvasGroup loadingPanel;

    [Tooltip("Durasi animasi fade in panel loading.")]
    public float loadingDuration = 3f;

    void Start()
    {
        StartCoroutine(InitializeAndTransition());
    }

    private IEnumerator InitializeAndTransition()
    {
        // Pastikan Singleton yang dibutuhkan sudah ada
        EnsureSingleton<SceneTransitionManager>("SceneTransitionManager");
        EnsureSingleton<GameManager>("GameManager");
        EnsureSingleton<GlobalSFX>("GlobalSFX");

        // Fade in animasi loading (jika ada)
        if (loadingPanel != null)
        {
            loadingPanel.alpha = 0;
            loadingPanel.DOFade(1, loadingDuration);
        }

        yield return new WaitForSeconds(loadingDuration);

        // Cek login status dari PlayerPrefs
        if (PlayerPrefs.HasKey("isLoggedIn") && PlayerPrefs.GetInt("isLoggedIn") == 1)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("MainMenuScene");
        }
        else
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("LoginScene");
        }
    }

    // Pastikan Singleton tidak duplikat
    private void EnsureSingleton<T>(string prefabName) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() == null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                DontDestroyOnLoad(instance);
            }
            else
            {
                Debug.LogError($"Prefab '{prefabName}' tidak ditemukan di folder Resources.");
            }
        }
    }
}
