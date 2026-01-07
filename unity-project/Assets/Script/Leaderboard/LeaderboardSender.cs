using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LeaderboardSender : MonoBehaviour
{
    [SerializeField] private string apiURL = "http://localhost/GameEdukasiDB/BackEnd/update_leaderboard.php";

    public void SendScore(int idLevel, int idStage, int score)
    {
        string nisn = PlayerPrefs.GetString("nisn", "");
        if (string.IsNullOrEmpty(nisn))
        {
            Debug.LogError("NISN tidak tersedia, pastikan user sudah login.");
            return;
        }

        StartCoroutine(SendScoreCoroutine(nisn, idLevel, idStage, score));
    }

    IEnumerator SendScoreCoroutine(string nisn, int idLevel, int idStage, int skor)
    {
        WWWForm form = new WWWForm();
        form.AddField("nisn", nisn);
        form.AddField("id_level", idLevel);  // Mengirimkan id_level
        form.AddField("id_stage", idStage);  // Mengirimkan id_stage
        form.AddField("skor", skor);         // Mengirimkan skor
        Debug.Log("NISN yang dikirim ke API: " + nisn);

        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Skor berhasil dikirim: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Gagal kirim skor: " + www.error);
        }
    }
}