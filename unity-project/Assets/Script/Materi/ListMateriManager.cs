using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using GameEdu.Utils;

public class ListMateriManager : MonoBehaviour
{
    [Header("Judul Materi")]
    public TMP_Text judulMateriText;

    [Header("Panel Materi")]
    public GameObject panelKosakata;
    public GameObject panelFrasa;

    [Header("Content Container")]
    public Transform contentKosakata;
    public Transform contentFrasa;

    [Header("Prefab Materi")]
    public GameObject prefabKosakata;
    public GameObject prefabFrasa;

    [Header("ScrollRect")]
    public ScrollRect scrollKosakata;
    public ScrollRect scrollFrasa;

    [Header("Data")]
    public List<Materi> allMateriData = new List<Materi>();


    private Queue<System.Action> downloadQueue = new Queue<System.Action>();
    private bool isDownloading = false;
    private bool isLoading = false;
    private string pilihanMateri;
    private string url;
    // private int currentLoadedCount = 0;
    private const int batchSize = 20; // Jumlah item yang dimuat dalam setiap batch
    int currentIndex = 0;   // posisi data terakhir yang diload

    void Start()
    {
        pilihanMateri = PlayerPrefs.GetString("pilihan_materi", "Reading");

        // Set judul materi
        if (judulMateriText != null)
        {
            judulMateriText.text = (pilihanMateri == "Reading") ? "Materi Frasa" : "Materi Kosakata";
        }

        // Tentukan URL berdasarkan materi
        bool isReading = pilihanMateri == "Reading";
        url = isReading 
            ? "http://localhost/GameEdukasiDB/BackEnd/get_materi_reading.php?limit=30&offset=0" 
            : "http://localhost/GameEdukasiDB/BackEnd/get_materi_listening.php?limit=30&offset=0";

        // Panel dan scroll
        panelFrasa.SetActive(pilihanMateri == "Reading");
        panelKosakata.SetActive(pilihanMateri == "Listening");

        if (isReading)
            scrollFrasa.onValueChanged.AddListener(OnScrollFrasa);
        else
            scrollKosakata.onValueChanged.AddListener(OnScrollKosakata);

        StartCoroutine(LoadMateri());
    }

    void OnScrollKosakata(Vector2 pos)
    {
        if (pos.y <= 0.05f && !isLoading && currentIndex < allMateriData.Count)
        {
            StartCoroutine(LoadMoreItems());
        }
    }

    void OnScrollFrasa(Vector2 pos)
    {
        if (pos.y <= 0.05f && !isLoading && currentIndex < allMateriData.Count)
        {
            StartCoroutine(LoadMoreItems());
        }
    }


    IEnumerator LoadMateri()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Gagal memuat data: " + request.error);
            yield break;
        }

        MateriList materiList = JsonUtility.FromJson<MateriList>("{\"materi\":" + request.downloadHandler.text + "}");
        allMateriData = materiList.materi;

        // Simpan juga ke global (jika dibutuhkan di tempat lain)
        GlobalMateriData.semuaMateri = allMateriData;

        // Mulai lazy load batch pertama
        StartCoroutine(LoadMoreItems());
    }

    IEnumerator LoadMoreItems()
    {
        if (isLoading) yield break;
        isLoading = true;

        yield return new WaitForSeconds(0.2f); // opsional: biar smooth

        int endIndex = Mathf.Min(currentIndex + batchSize, allMateriData.Count);

        for (int i = currentIndex; i < endIndex; i++)
        {
            Materi data = allMateriData[i];

            // Pilih prefab & parent berdasarkan pilihanMateri
            GameObject prefab;
            Transform parent;

            if (pilihanMateri == "Listening")
            {
                prefab = prefabKosakata;
                parent = contentKosakata;
            }
            else // Reading
            {
                prefab = prefabFrasa;
                parent = contentFrasa;
            }

            GameObject item = Instantiate(prefab, parent);
            item.transform.localScale = Vector3.one;

            // Set judul
            var judulText = item.transform.Find("Judul")?.GetComponent<TMP_Text>();
            if (judulText != null)
            {
                if (pilihanMateri == "Listening")
                    judulText.text = data.kosakata;
                else
                    judulText.text = data.judul;
            }

            // Set nomor urut untuk frasa
            var noText = item.transform.Find("No")?.GetComponent<TMP_Text>();
            if (pilihanMateri == "Reading" && noText != null)
                noText.text = (i + 1).ToString();

            // Gambar
            RawImage rawImage = item.transform.Find("Gambar")?.GetComponent<RawImage>();
            if (rawImage != null)
                StartCoroutine(LoadImage(data.gambar, rawImage));

            // Klik handler (jika ada)
            var clickHandler = item.GetComponent<ImageClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.materi = data;
                clickHandler.materiIndex = i;
            }
        }

        currentIndex = endIndex;
        isLoading = false;
    }

    IEnumerator LoadImage(string imageUrl, RawImage targetImage)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogWarning("URL gambar kosong");
            yield break;
        }

        string fileName = ImageCacheManager.GetSafeFileName(imageUrl);
        string fullUrl = "http://localhost/GameEdukasiDB/" + imageUrl;

        Debug.Log($"[LoadImage] Mencoba load: {fullUrl}");

        if (ImageCacheManager.HasImage(fileName))
        {
            Debug.Log($"[LoadImage] Ambil dari cache: {fileName}");
            targetImage.texture = ImageCacheManager.GetImage(fileName);
            yield break;
        }

        // Masukkan ke queue download
        downloadQueue.Enqueue(() =>
        {
            if (targetImage != null)
                StartCoroutine(DownloadImage(fullUrl, fileName, targetImage));
        });

        if (!isDownloading)
            StartCoroutine(ProcessDownloadQueue());
    }

    IEnumerator DownloadImage(string url, string fileName, RawImage targetImage)
    {
        Debug.Log($"[DownloadImage] Mengunduh dari URL: {url}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                ImageCacheManager.AddImage(fileName, texture);
                targetImage.texture = texture;
                Debug.Log($"[DownloadImage] Berhasil unduh dan pasang gambar: {fileName}");
            }
            else
            {
                Debug.LogWarning("[DownloadImage] Texture null dari URL: " + url);
            }
        }
        else
        {
            Debug.LogError("[DownloadImage] Gagal unduh gambar dari URL: " + url + " Error: " + request.error);
        }
    }

    IEnumerator ProcessDownloadQueue()
    {
        isDownloading = true;

        while (downloadQueue.Count > 0)
        {
            var action = downloadQueue.Dequeue();
            action.Invoke();
            yield return new WaitForSeconds(0.5f); // delay antar download
        }

        isDownloading = false;
    }

    // IEnumerator DownloadImage(string url, string fileName, RawImage targetImage)
    // {
    //     UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
    //     yield return request.SendWebRequest();

    //     if (request.result == UnityWebRequest.Result.Success)
    //     {
    //         Texture2D texture = DownloadHandlerTexture.GetContent(request);
    //         if (texture != null)
    //         {
    //             ImageCacheManager.AddImage(fileName, texture);
    //             targetImage.texture = texture;
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Gagal unduh gambar: " + request.error);
    //     }
    // }

    public void Pilihkembalimainmemu()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MenuMateri");
    }
}

[System.Serializable]
public class Materi
{
    public string judul;
    public string kosakata;
    public string gambar;
    public int id_frasa;
    public int id_kosakata;
    public string deskripsi;
    public string terjemahan;
    public string suara;
    public string spelling;
    public string jenis; // << TAMBAHKAN INI
}

[System.Serializable]
public class MateriList
{
    public List<Materi> materi;
}
