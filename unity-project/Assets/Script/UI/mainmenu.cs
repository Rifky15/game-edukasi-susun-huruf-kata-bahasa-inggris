using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Tambahkan ini
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Networking;

public class mainmenu : MonoBehaviour
{
    public Animator animator; // Referensi ke Animator
    public float animationSpeed = 1.0f; // Kecepatan default animasi

    public GameObject popupPanel; // Referensi ke Panel Pop-up
    public TMP_Text popupText; // Referensi ke teks dalam popupPanel
    public string materiSceneName = "Materi"; // Nama scene materi

    public TMP_Text tmpJumlahBuku; // UI untuk jumlah buku
    public TMP_Text tmpTotalSkor; // UI untuk total skor

    public TMP_Text tmpCountdownBuku; // UI teks hitung mundur

    public GameObject quitConfirmationPanel; // UI Panel Konfirmasi Keluar

    private int lastJumlahBuku = -1;
    private float lastWaktuSisa = -1f;

    // private string apiTotalSkorUrl = "https://vibeproject.web.id/GameEdukasiDB/BackEnd/get_total_skor.php";
    private string apiProgressUrl = "http://localhost/GameEdukasiDB/BackEnd/get_progress.php";

    private void Start()
    {
        UpdateUIBuku();
        StartCoroutine(FetchTotalSkor());

        Debug.Log($"[MainMenu] Level Aktif: {GameManager.Instance.GetCurrentLevel()}");
        Debug.Log($"[MainMenu] Stage Aktif: {GameManager.Instance.GetCurrentStage()}");
        Debug.Log($"[MainMenu] Jumlah Buku: {GameManager.Instance.GetJumlahBuku()}");
        Debug.Log($"[MainMenu] Waktu Sisa: {GameManager.Instance.GetWaktuSisa()} detik");
    }

    public void ForceRefreshUI()
    {
        UpdateUIBuku();
        StartCoroutine(FetchTotalSkor());

        // Paksa reset last value agar Update() trigger
        lastJumlahBuku = -1;
    }


    private void UpdateUIBuku()
    {
        if (tmpJumlahBuku != null)
            tmpJumlahBuku.text = GameManager.Instance.GetJumlahBuku().ToString(); // Hanya angka
    }

    IEnumerator FetchTotalSkor()
    {
        string nisnPlayer = PlayerPrefs.GetString("nisn", "");
        string url = $"{apiProgressUrl}?nisn={nisnPlayer}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ProgressResponse response = JsonUtility.FromJson<ProgressResponse>(json);

                if (response.status == "success")
                {
                    tmpTotalSkor.text = response.total_skor.ToString();

                    // Debug log tambahan
                    Debug.Log($"[MainMenu] Data Loaded => LevelID: {response.id_level}, StageID: {response.id_stage}, TotalSkor: {response.total_skor}");
                }
                else
                {
                    Debug.LogWarning("Status gagal saat ambil progress");
                    tmpTotalSkor.text = "Gagal Load";
                }
            }
            else
            {
                Debug.LogError("Gagal mengambil progress: " + request.error);
                tmpTotalSkor.text = "Gagal Load";
            }
        }
    }


    private void Update()
    {
        int jumlahBuku = GameManager.Instance.GetJumlahBuku();

        if (tmpJumlahBuku != null && jumlahBuku != lastJumlahBuku)
        {
            lastJumlahBuku = jumlahBuku;
            tmpJumlahBuku.text = jumlahBuku.ToString();
        }

        if (jumlahBuku < 5)
        {
            float waktuSekarang = GameManager.Instance.GetWaktuSisa();

            if (Mathf.Abs(waktuSekarang - lastWaktuSisa) > 1f) // Update tiap detik
            {
                lastWaktuSisa = waktuSekarang;
                tmpCountdownBuku.text = "Buku Pulih: " + FormatTime(waktuSekarang);
            }

            if (!tmpCountdownBuku.gameObject.activeSelf)
                tmpCountdownBuku.gameObject.SetActive(true);
        }
        else
        {
            if (tmpCountdownBuku.gameObject.activeSelf)
                tmpCountdownBuku.gameObject.SetActive(false);
        }
    }

    private string FormatTime(float seconds)
    {
        int menit = Mathf.FloorToInt(seconds / 60);
        int detik = Mathf.FloorToInt(seconds % 60);
        return string.Format("{0:00}:{1:00}", menit, detik);
    }


    // Fungsi untuk pindah ke scene Play
    // public void PlayGame()
    // {
    //     EventSystem eventSystem = FindObjectOfType<EventSystem>();
    //     if (eventSystem != null)
    //     {
    //         Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
    //     }
    //     SceneTransitionManager.Instance.LoadSceneWithFade("Pilihanlvl"); // Tambahkan LoadSceneMode.Single
    // }
    public void PlayGame()
    {
        int buku = GameManager.Instance.GetJumlahBuku();

        if (buku <= 0)
        {
            // Tampilkan popupPanel dengan pesan
            if (popupPanel != null)
            {
                popupPanel.SetActive(true);

                if (popupText != null)
                {
                    popupText.text = "Yah, kamu kehabisan buku.\nSilakan Membaca materi atau tunggu 10 menit.";
                }
            }
            else
            {
                Debug.LogWarning("popupPanel tidak ditemukan!");
            }

            return; // Jangan lanjut ke scene
        }

        // Jika jumlah buku cukup, lanjut ke scene Pilihanlvl
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject);
        }

        SceneTransitionManager.Instance.LoadSceneWithFade("Pilihanlvl");
    }

    public void Materi()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("MenuMateri");
    }
    public void Informasi()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("InformasiScene");
    }
    public void Leaderboard()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("LeaderboardScene");
    }

    //panelconfirmation quit
    public void ShowQuitConfirmation()
    {
        quitConfirmationPanel.SetActive(true); // Tampilkan panel konfirmasi
    }

    public void HideQuitConfirmation()
    {
        quitConfirmationPanel.SetActive(false); // Sembunyikan panel konfirmasi
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit(); // Keluar dari game (berfungsi di build)
    }

    public void Logout()
    {
        // Hapus semua PlayerPrefs yang berkaitan dengan progres
        PlayerPrefs.DeleteAll(); // Ini menghapus SEMUA kunci, termasuk nisn, skor, level, dsb
        PlayerPrefs.Save();

        Debug.Log("Logout: Semua data PlayerPrefs dihapus. Kembali ke LoginScene.");

        // Pindah ke scene login
        SceneTransitionManager.Instance.LoadSceneWithFade("LoginScene");
    }


    // Fungsi untuk mengatur kecepatan animasi
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed; // Simpan nilai kecepatan
        if (animator != null)
        {
            animator.speed = animationSpeed; // Atur kecepatan animator
        }
        else
        {
            Debug.LogWarning("Animator tidak ditemukan!");
        }
    }

    // Fungsi untuk menampilkan pop-up
    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true); // Aktifkan Panel Pop-up
        }
        else
        {
            Debug.LogWarning("Popup Panel tidak ditemukan!");
        }
    }

    // Fungsi untuk menyembunyikan pop-up
    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false); // Nonaktifkan Panel Pop-up
        }
    }

    // Fungsi untuk pindah ke scene materi dari pop-up
    public void GoToMateriScene()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MenuMateri"); // Pindah ke scene Materi
    }
    
    public void GoToHistoryNilai()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("History");
    }
}

[System.Serializable]
public class ProgressResponse
{
    public string status;
    public int id_level;
    public int id_stage;
    public int total_skor;
    public Dictionary<string, int> highest_stages; // Gunakan string sebagai key
}
