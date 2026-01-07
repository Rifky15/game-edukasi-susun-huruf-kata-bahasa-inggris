using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }
    public float waktuMax = 60f; // Default 60 detik
    private float waktuSisa;
    public TMP_Text timerText;
    private bool timerAktif = false;

    public event Action OnTimeUp; // Event saat waktu habis
    private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    private void Update()
    {
        if (timerAktif && waktuSisa > 0)
        {
            waktuSisa -= Time.deltaTime;
            UpdateTampilanTimer();

            if (waktuSisa <= 0)
            {
                waktuSisa = 0;
                timerAktif = false;
                OnTimeUp?.Invoke(); // Memanggil event waktu habis
            }
        }
    }

    public void MulaiTimer(float durasi)
    {
        waktuSisa = durasi;
        timerAktif = true;
        UpdateTampilanTimer();
    }

    public void ResetTimer()
    {
        timerAktif = false;
        waktuSisa = waktuMax;
        UpdateTampilanTimer();
    }

    private void UpdateTampilanTimer()
    {
        timerText.text = Mathf.CeilToInt(waktuSisa).ToString();
    }
}
