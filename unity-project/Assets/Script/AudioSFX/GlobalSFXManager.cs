using UnityEngine;

public class GlobalSFX : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        // Deteksi klik kiri mouse atau tap layar
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogWarning("AudioSource atau ClickSound belum di-assign!");
        }
    }
}
