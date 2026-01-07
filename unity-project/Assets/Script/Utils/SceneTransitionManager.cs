using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    public Image fadeImage;

    [Tooltip("Durasi fade. Jika aktifkan randomize, nilai ini akan diabaikan.")]
    public float fadeDuration = 0.5f;

    [Tooltip("Acak durasi fade antara 0.5 dan 1 detik setiap transisi.")]
    public bool randomizeFadeDuration = false;

    private float GetFadeDuration()
    {
        return randomizeFadeDuration ? Random.Range(0.5f, 0.75f) : fadeDuration;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
            fadeImage.DOFade(0f, GetFadeDuration());
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        float currentFadeDuration = GetFadeDuration();
        fadeImage.raycastTarget = true;

        fadeImage.DOFade(1f, currentFadeDuration).OnComplete(() =>
        {
            StartCoroutine(PerformSceneLoad(sceneName, currentFadeDuration));
        });
    }

    private IEnumerator PerformSceneLoad(string sceneName, float fadeDuration)
    {
        yield return null; // beri waktu 1 frame agar sistem tidak overload

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Tunggu sampai scene benar-benar siap
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Beri delay agar sistem stabil sebelum melanjutkan
        yield return new WaitForSeconds(0.1f);

        // Hapus asset tak terpakai agar memory turun
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // Fade out setelah scene sudah siap
        fadeImage.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            fadeImage.raycastTarget = false;
        });
    }
}
