using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GameEdu.Utils;

public class DetailReading : MonoBehaviour
{
    public string apiUrl = "http://localhost/GameEdukasiDB/BackEnd/get_materi_reading_detail.php";

    [Header("UI Elements")]
    public TMP_Text judulText;
    public RawImage gambarImage;
    public TMP_Text deskripsiText;
    public TMP_Text terjemahanText;

    [Header("Reading Timer")]
    public TMP_Text countdownText;      // ➔ Teks hitung mundur (60 detik)
    public TMP_Text notificationText;    // ➔ Teks notifikasi selesai
    public GameObject bgNotif;           // ➔ Background notifikasi

    private float readingDuration = 60f; // 60 detik
    private bool isReading = false;
    
    public CanvasGroup canvasGroup;

    void Start()
    {
        Debug.Log($"🔍 Index Saat Masuk: {GlobalMateriData.indexMateriSekarang}");
        Debug.Log($"📌 Total Materi Saat Ini: {GlobalMateriData.semuaMateri.Count}");

        if (GlobalMateriData.semuaMateri.Count > 0)
        {
            // Kalau ada data list materi, langsung tampilkan berdasarkan index
            TampilkanMateri(GlobalMateriData.indexMateriSekarang);
        }
        else
        {
            // Kalau tidak ada, fallback ke sistem lama (ambil dari server pakai ID)
            int materiId = PlayerPrefs.GetInt("materiId", -1);
            Debug.Log("Di DetailReading, materiId: " + materiId);

            if (materiId > 0)
            {
                StartCoroutine(FetchDetailMateri(materiId));
            }
            else
            {
                Debug.LogError("ID materi tidak ditemukan!");
            }
        }

        StartCoroutine(StartReadingSession());
    }


    IEnumerator FadeMateri()
    {
        // Fade Out
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        // Ganti materi
        TampilkanMateri(GlobalMateriData.indexMateriSekarang);

        // Fade In
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }
    }
    IEnumerator FetchDetailMateri(int id)
    {
        string requestUrl = apiUrl + "?id=" + id;
        Debug.Log("Request URL: " + requestUrl);

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Response: " + json);
                DetailMateri data = JsonUtility.FromJson<DetailMateri>(json);

                if (data != null && string.IsNullOrEmpty(data.error))
                {
                    judulText.text = data.judul;
                    deskripsiText.text = data.deskripsi;
                    terjemahanText.text = data.terjemahan;
                    string fullUrl = "http://localhost/GameEdukasiDB/" + data.gambar;
                    Texture2D cachedTex = ImageCacheManager.GetImage(fullUrl);

                    if (cachedTex != null)
                    {
                        gambarImage.texture = cachedTex;
                    }
                    else
                    {
                        StartCoroutine(LoadImage(data.gambar, gambarImage));
                    }

                }
                else
                {
                    Debug.LogError("Data tidak ditemukan: " + json);
                }
            }
            else
            {
                Debug.LogError("Gagal mengambil data: " + request.error);
            }
        }
    }

    IEnumerator LoadImage(string url, RawImage targetImage) 
    {
        string fullUrl = "http://localhost/GameEdukasiDB/" + url;

        if (ImageCacheManager.HasImage(fullUrl)) {
            Texture2D cachedTexture = ImageCacheManager.GetImage(fullUrl);
            if (cachedTexture != null) {
                targetImage.texture = cachedTexture;
                // targetImage.SetNativeSize();
                yield break; // Hentikan coroutine karena cache sudah tersedia
            }
        }

        // Jika gambar belum ada di cache, lakukan request
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullUrl)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null) {
                    ImageCacheManager.AddImage(fullUrl, texture); // Simpan ke cache
                    targetImage.texture = texture;
                    // targetImage.SetNativeSize();
                }
            } else {
                Debug.LogError("Gagal memuat gambar: " + request.error);
            }
        }
    }
    void LoadDetailMateri(string imageUrl, RawImage targetImage) 
    {
        StartCoroutine(LoadImage(imageUrl, targetImage));
    }

    IEnumerator StartReadingSession()
    {
        while (true)  // loop tak berujung
        {
            isReading = true;
            notificationText.text = "";
            bgNotif.SetActive(false);

            float timer = readingDuration;

            while (timer > 0f)
            {
                countdownText.text = Mathf.CeilToInt(timer).ToString() + " detik";
                timer -= Time.deltaTime;
                yield return null;
            }

            countdownText.text = "0 detik";

            notificationText.text = "Selamat anda selesai membaca mendapatkan 1 buku!";
            bgNotif.SetActive(true);

            GameManager.Instance.TambahBuku(1);

            // Delay notifikasi tampil beberapa detik agar user sempat baca
            yield return new WaitForSeconds(2f);
        }
    }



    public void TampilkanMateri(int index)
    {
        if (index >= 0 && index < GlobalMateriData.semuaMateri.Count)
        {
            Materi materiSekarang = GlobalMateriData.semuaMateri[index];

            judulText.text = materiSekarang.judul;
            deskripsiText.text = materiSekarang.deskripsi;
            terjemahanText.text = materiSekarang.terjemahan;

            string fullUrl = "http://localhost/GameEdukasiDB/" + materiSekarang.gambar;
            Texture2D cachedTex = ImageCacheManager.GetImage(fullUrl);

            if (cachedTex != null)
            {
                gambarImage.texture = cachedTex;
            }
            else
            {
                StartCoroutine(LoadImage(materiSekarang.gambar, gambarImage));
            }
        }
        else
        {
            Debug.LogError("Index materi di luar batas!");
        }
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


    public void Pilihkembalimainmemu()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("ListMateri");
    }
}

[System.Serializable]
public class DetailMateri
{
    public string judul;
    public string gambar;
    public string deskripsi;
    public string terjemahan;
    public string error;
}
