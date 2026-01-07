using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GameEdu.Utils;

public class DetailListening : MonoBehaviour
{
    public string apiUrl = "http://localhost/GameEdukasiDB/BackEnd/get_materi_listening_detail.php";

    [Header("UI Elements")]
    public TMP_Text namaText;
    public TMP_Text spellingText;
    public TMP_Text terjemahanText;
    public RawImage gambarImage;
    public AudioSource audioSource;
    public Button playButton;
    public Image progressImage;
    public GameObject notificationPopup;
    public TMP_Text notificationText;
    public CanvasGroup canvasGroup;

    private int idMateriTerakhirDiputar = -1;
    public string idMateri;

    private string audioCachePath;
    private bool sudahReportSelesai = false;

    private void Start()
    {
        Debug.Log("🔍 Memulai DetailListening...");
        
        // Buat folder cache jika belum ada
        audioCachePath = Path.Combine(Application.persistentDataPath, "AudioCache");
        if (!Directory.Exists(audioCachePath)) Directory.CreateDirectory(audioCachePath);

        // Pastikan ID materi tersedia dan diambil dengan benar
        int materiId = PlayerPrefs.GetInt("materiId", -1);
        if (materiId == -1) {
            Debug.LogError("❌ ID materi tidak ditemukan di PlayerPrefs!");
            return;
        }

        // Pastikan index sesuai dengan materi yang diklik sebelumnya
        GlobalMateriData.indexMateriSekarang = GlobalMateriData.semuaMateri.FindIndex(m => m.id_kosakata == materiId);

        if (GlobalMateriData.indexMateriSekarang == -1) {
            Debug.LogError($"❌ ID materi {materiId} tidak ditemukan dalam daftar semuaMateri!");
            return;
        }

        Debug.Log($"📡 Memuat Detail Materi untuk ID Kosakata: {materiId}");

        // Hentikan semua coroutine sebelum mulai mengambil data baru
        StopAllCoroutines();

        // Mulai mengambil detail materi
        StartCoroutine(FetchDetailMateri(materiId));
    }

    IEnumerator FetchDetailMateri(int id)
    {
        string requestUrl = apiUrl + "?id=" + id;

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.timeout = 10; // Tambahkan timeout agar request tidak menggantung
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Gagal mengambil data: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log($"📡 Data Diterima: {json}");

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("❌ JSON kosong, kemungkinan ada masalah dengan server.");
                yield break;
            }

            DetailMateriListening data = JsonUtility.FromJson<DetailMateriListening>(json);

            if (data == null)
            {
                Debug.LogError("❌ Parsing JSON gagal, cek apakah formatnya benar.");
                yield break;
            }

            if (!string.IsNullOrEmpty(data.error))
            {
                Debug.LogError($"❌ Data tidak ditemukan: {data.error}");
                yield break;
            }

            // Pastikan UI diperbarui hanya jika data valid
            namaText.text = !string.IsNullOrEmpty(data.kosakata) ? data.kosakata : "Kosakata tidak tersedia";
            spellingText.text = !string.IsNullOrEmpty(data.spelling) ? data.spelling : "Spelling tidak tersedia";
            terjemahanText.text = !string.IsNullOrEmpty(data.terjemahan) ? data.terjemahan : "Terjemahan tidak tersedia";

            if (!string.IsNullOrEmpty(data.gambar))
                StartCoroutine(LoadImage(data.gambar, gambarImage));
            else
                Debug.LogWarning("⚠ Gambar tidak tersedia");

            if (!string.IsNullOrEmpty(data.suara))
                StartCoroutine(LoadAudio(data.suara));
            else
                Debug.LogWarning("⚠ Audio tidak tersedia");
        }
    }

    IEnumerator LoadImage(string url, RawImage targetImage) 
    {
        string fullUrl = "http://localhost/GameEdukasiDB/" + url;

        if (ImageCacheManager.HasImage(fullUrl)) {
            Texture2D cachedTexture = ImageCacheManager.GetImage(fullUrl);
            if (cachedTexture != null) {
                targetImage.texture = cachedTexture;
                yield break; // Stop coroutine karena gambar sudah ada di cache
            }
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullUrl)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                ImageCacheManager.AddImage(fullUrl, texture); // Simpan ke cache
                targetImage.texture = texture;
            } else {
                Debug.LogError("Gagal memuat gambar: " + request.error);
            }
        }
    }

    IEnumerator LoadAudio(string audioUrl)
    {
        if (string.IsNullOrEmpty(audioUrl))
        {
            Debug.LogWarning("Audio URL kosong, skip load audio.");
            yield break;
        }

        // Gunakan Proxy PHP agar file MP3 bisa diakses
        string fullUrl = "http://localhost/GameEdukasiDB/audio_proxy.php?file=" + Path.GetFileName(audioUrl) + "&cache=" + Time.time;
        string audioFilePath = Path.Combine(audioCachePath, Path.GetFileName(audioUrl));

        // **Jika file ada di cache, langsung gunakan**
        if (File.Exists(audioFilePath))
        {
            Debug.Log("Menggunakan audio dari cache.");
            StartCoroutine(PlayCachedAudio(audioFilePath));
        }
        else
        {
            Debug.Log("Mengunduh audio baru karena tidak ditemukan di cache.");
            yield return StartCoroutine(DownloadAudio(fullUrl, audioFilePath));
        }
    }

    IEnumerator DownloadAudio(string url, string filePath)
    {
        // **Gunakan UnityWebRequestMultimedia untuk MP3**
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
                // **Tampilkan notifikasi bahwa audio sedang diunduh**
            notificationPopup.SetActive(true);
            notificationText.text = "Mengunduh audio...";
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                File.WriteAllBytes(filePath, request.downloadHandler.data); // Simpan file ke cache
                StartCoroutine(PlayCachedAudio(filePath));

                notificationPopup.SetActive(false);
            }
            else
            {
                notificationText.text = "Gagal mengunduh audio!";
                Debug.LogError("Gagal mengunduh audio: " + request.error);
            }
        }
    }

    IEnumerator PlayCachedAudio(string filePath)
    {
        yield return StartCoroutine(LoadAudioClipFromFile(filePath, (AudioClip clip) =>
        {
            if (clip == null)
            {
                Debug.LogError("Gagal memuat AudioClip dari file: " + filePath);
                return;
            }

            audioSource.clip = clip;

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() =>
            {
                audioSource.Play();
                sudahReportSelesai = false;
                StartCoroutine(CekAudioSelesai());
            });
        }));
    }

    IEnumerator CekAudioSelesai()
    {
        while (audioSource.isPlaying)
            yield return null;

        if (!sudahReportSelesai)
        {
            sudahReportSelesai = true;

            // Ambil ID materi saat ini
            int materiIdSekarang = GlobalMateriData.semuaMateri[GlobalMateriData.indexMateriSekarang].id_kosakata;

            // Cek apakah sudah pernah diputar
            if (materiIdSekarang != idMateriTerakhirDiputar)
            {
                idMateriTerakhirDiputar = materiIdSekarang;

                string notif = ListeningQuestManager.Instance.AudioSelesaiDiputar(materiIdSekarang.ToString());

                // Tambahkan progress bar
                UpdateProgressBar();

                // Tampilkan notifikasi jika ada
                if (!string.IsNullOrEmpty(notif))
                {
                    notificationText.text = notif;
                    notificationPopup.SetActive(true);
                    StartCoroutine(ShowNotification(notif));
                }
            }
            else
            {
                Debug.Log(" Audio sudah diputar sebelumnya, tidak menambah progress.");
            }
        }
    }

    IEnumerator LoadAudioClipFromFile(string filePath, System.Action<AudioClip> onAudioLoaded)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Path file MP3 kosong!");
            yield break;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogError("File MP3 tidak ditemukan: " + filePath);
            yield break;
        }

        // Gunakan MP3Utility untuk memuat audio MP3
        yield return MP3Utility.LoadMP3FromFile(filePath, onAudioLoaded);
    }

    private void UpdateProgressBar()
    {
        float progress = (float)ListeningQuestManager.Instance.GetJumlahAudioSelesai() / 5f;

        if (progress >= 1f)
        {
            // Reset progress ke 0
            ListeningQuestManager.Instance.ResetProgress(); // Asumsikan kamu punya fungsi ini untuk reset internal progres

            progress = 0f;
        }

        progressImage.fillAmount = progress;
        progressImage.gameObject.SetActive(true);
    }


    public void TampilkanMateri(int index)
    {
        audioSource.Stop();
        audioSource.clip = null;
        playButton.onClick.RemoveAllListeners();
        sudahReportSelesai = false;

        if (index < 0 || index >= GlobalMateriData.semuaMateri.Count) {
            Debug.LogError("❌ Index materi di luar batas! Tidak bisa menampilkan data.");
            return;
        }

        Materi data = GlobalMateriData.semuaMateri[index];

        if (data == null) {
            Debug.LogError("❌ Data masih null, cek apakah semuaMateri sudah terisi!");
            return;
        }

        namaText.text = data.kosakata;
        spellingText.text = data.spelling;
        terjemahanText.text = data.terjemahan;

        StartCoroutine(LoadImage(data.gambar, gambarImage));
        StartCoroutine(LoadAudio(data.suara));
    }

    public void NextMateri()
    {
        if (GlobalMateriData.indexMateriSekarang < GlobalMateriData.semuaMateri.Count - 1)
        {
            GlobalMateriData.indexMateriSekarang++;
            StartCoroutine(FadeMateri());
        }
    }

    public void PreviousMateri()
    {
        if (GlobalMateriData.indexMateriSekarang > 0)
        {
            GlobalMateriData.indexMateriSekarang--;
            StartCoroutine(FadeMateri());
        }
    }

    IEnumerator FadeMateri()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        TampilkanMateri(GlobalMateriData.indexMateriSekarang);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }
    }

    public void PilihKembali()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject);
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("ListMateri");
    }

    IEnumerator ShowNotification(string message)
    {
        notificationPopup.SetActive(true);
        notificationText.text = message;
        yield return new WaitForSeconds(3f);
        notificationPopup.SetActive(false);
    }
    Queue<string> notificationQueue = new Queue<string>();

    void ShowNextNotification()
    {
        if (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();
            notificationPopup.SetActive(true);
            notificationText.text = message;
            StartCoroutine(HideNotificationAfterDelay());
        }
    }

    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        notificationPopup.SetActive(false);
        ShowNextNotification(); // Cek apakah masih ada notifikasi lain dalam antrian
    }

    void AddNotification(string message)
    {
        notificationQueue.Enqueue(message);
        if (!notificationPopup.activeSelf)
            ShowNextNotification();
    }
}

[System.Serializable]
public class DetailMateriListening
{
    public string kosakata;
    public string spelling;
    public string terjemahan;
    public string gambar;
    public string suara;
    public string error;
}
