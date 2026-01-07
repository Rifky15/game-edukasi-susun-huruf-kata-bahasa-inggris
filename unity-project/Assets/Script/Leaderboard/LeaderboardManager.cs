using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject leaderboardPrefab;
    public Transform leaderboardContainer;
    public string apiUrl = "http://localhost/GameEdukasiDB/BackEnd/get_leaderboard.php";

    void Start()
    {
        StartCoroutine(FetchLeaderboard());
    }

    IEnumerator FetchLeaderboard()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                LeaderboardList leaderboardList = JsonUtility.FromJson<LeaderboardList>(json);

                for (int i = 0; i < leaderboardList.data.Count; i++)
                {
                    GameObject item = Instantiate(leaderboardPrefab, leaderboardContainer);
                    item.transform.Find("NoUrut").GetComponent<Text>().text = (i + 1).ToString();
                    item.transform.Find("Nama").GetComponent<Text>().text = leaderboardList.data[i].nama;
                    item.transform.Find("TotalSkor").GetComponent<Text>().text = leaderboardList.data[i].total_skor.ToString();
                }
            }
            else
            {
                Debug.LogError("Gagal mengambil data leaderboard: " + request.error);
            }
        }
    }

    public void BacktoMainMenu()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
    }
}

[System.Serializable]
public class LeaderboardData
{
    public string nisn;
    public string nama;
    public int total_skor;
}

[System.Serializable]
public class LeaderboardList
{
    public List<LeaderboardData> data;
}