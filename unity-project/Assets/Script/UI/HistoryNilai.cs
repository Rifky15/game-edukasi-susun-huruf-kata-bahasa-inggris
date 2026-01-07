using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro; // ← penting untuk TMP_Text

[System.Serializable]
public class HistoryItemData
{
    public int id_level;
    public int id_stage;
    public int skor;
    public string updated_at;
}

public class HistoryNilai : MonoBehaviour
{
    [Header("UI Prefab & Container")]
    public GameObject dataHistoryPrefab; // Drag prefab "DataHistory"
    public Transform contentContainer;   // Drag Content dari ScrollView

    [Header("Info Pengguna")]
    public TMP_Text isiNIM;   // Drag Text TMP untuk menampilkan NIM
    // public TMP_Text isiNama;  // Drag Text TMP untuk menampilkan Nama

    [Header("API Settings")]
    public string apiUrl = "http://localhost/GameEdukasiDB/BackEnd/get_leaderboard_by_nisn.php";

    void Start()
    {
        string nisn = PlayerPrefs.GetString("nisn");
        string nama = PlayerPrefs.GetString("nama"); // pastikan nama sudah disimpan di PlayerPrefs juga

        // tampilkan ke UI TMP
        if (isiNIM != null) isiNIM.text = nisn;
        // if (isiNama != null) isiNama.text = nama;

        StartCoroutine(LoadHistoryData(nisn));
    }

    IEnumerator LoadHistoryData(string nisn)
    {
        string fullUrl = $"{apiUrl}?nisn={nisn}";
        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                if (string.IsNullOrEmpty(json) || json == "[]")
                {
                    Debug.Log("Tidak ada data history untuk NISN ini");
                    yield break;
                }

                List<HistoryItemData> historyList = JsonHelper.FromJson<HistoryItemData>(json);

                foreach (var item in historyList)
                {
                    GameObject go = Instantiate(dataHistoryPrefab, contentContainer);

                    go.transform.Find("Level").GetComponent<Text>().text =
                        $"Level: {(item.id_level == 1 ? "Mudah" : item.id_level == 2 ? "Sedang" : "Sulit")}";

                    go.transform.Find("Stage").GetComponent<Text>().text = $"Stage: {item.id_stage}";
                    go.transform.Find("Skor").GetComponent<Text>().text = $"Skor: {item.skor}";
                    go.transform.Find("waktu").GetComponent<Text>().text = $"Waktu: {FormatTanggal(item.updated_at)}";
                    go.transform.Find("Status").GetComponent<Text>().text =
                        $"Status: {(item.skor >= 75 ? "Lulus" : "Tidak Lulus")}";
                }
            }
            else
            {
                Debug.LogError("Gagal memuat data leaderboard: " + request.error);
            }
        }
    }

    string FormatTanggal(string tanggalAsli)
    {
        if (DateTime.TryParse(tanggalAsli, out DateTime waktu))
            return waktu.ToString("dd/MM/yyyy");
        else
            return tanggalAsli;
    }

    public void BacktoMainMenu()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
    }
}

/// Helper untuk parse JSON array ke List<T>
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}
