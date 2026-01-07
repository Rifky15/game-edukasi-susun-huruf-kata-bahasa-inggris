using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip correctBonusSound;
    // public AudioClip gameOverSound;
    // public AudioClip gameWinSound;


    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component tidak ditemukan di GameObject SFXManager!");
        }
    }

    public void PlayCorrect()
    {
        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            Debug.LogWarning("correctSound atau audioSource belum di-assign!");
        }
    }

    public void PlayWrong()
    {
        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }
        else
        {
            Debug.LogWarning("wrongSound atau audioSource belum di-assign!");
        }
    }

    public void PlayCorrectBonus()
    {
        if (correctBonusSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctBonusSound);
        }
        else
        {
            Debug.LogWarning("correctBonusSound atau audioSource belum di-assign!");
        }
    }

    // public void PlayGameOver()
    // {
    //     if (gameOverSound != null && audioSource != null)
    //     {
    //         audioSource.PlayOneShot(gameOverSound);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("gameOverSound atau audioSource belum di-assign!");
    //     }
    // }

    // public void PlayGameWin()
    // {
    //     if (gameWinSound != null && audioSource != null)
    //     {
    //         audioSource.PlayOneShot(gameWinSound);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("gameWinSound atau audioSource belum di-assign!");
    //     }
    // }

}
