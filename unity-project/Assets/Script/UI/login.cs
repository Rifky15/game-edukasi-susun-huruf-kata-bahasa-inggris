using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Tambahkan ini
using Newtonsoft.Json;


public class LoginManager : MonoBehaviour
{
    public TMP_InputField nisnInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public GameObject bgNotif;
    public TMP_Text errorLog;

    public Button btnResetPlayerPrefs;

    private string loginURL = "http://localhost/GameEdukasiDB/BackEnd/login.php";
    private string progressURL = "http://localhost/GameEdukasiDB/BackEnd/get_progress.php";

    void Start()
    {
        loginButton.onClick.AddListener(AttemptLogin);

        if (btnResetPlayerPrefs != null)
        {
            btnResetPlayerPrefs.onClick.AddListener(ResetPlayerPrefs);
        }

        // Cek jika user sudah login sebelumnya
        if (PlayerPrefs.HasKey("IsLoggedIn") && PlayerPrefs.GetInt("IsLoggedIn") == 1)
        {
            string savedNISN = PlayerPrefs.GetString("nisn");
            GameManager.Instance.SetNISN(savedNISN);

            // Ambil level dan stage dari PlayerPrefs
            if (PlayerPrefs.HasKey("level") && PlayerPrefs.HasKey("stage"))
            {
                int level = PlayerPrefs.GetInt("level");
                int stage = PlayerPrefs.GetInt("stage");
                int totalSkor = PlayerPrefs.GetInt("total_skor", 0); // Jika total_skor tidak ada, set ke 0

                GameManager.Instance.SetLevel(level);
                GameManager.Instance.SetStage(stage);
                GameManager.Instance.SetTotalSkor(totalSkor);

                Debug.Log($"Auto-login: Level {level}, Stage {stage}, Skor {totalSkor}");
            }

            Debug.Log("Auto-login sebagai: " + savedNISN);
            SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
        }

    }

    void AttemptLogin()
    {
        ShowNotification("Logging in...", Color.yellow);
        StartCoroutine(LoginCoroutine());
    }

    IEnumerator LoginCoroutine()
    {
        string nisn = nisnInput.text;
        // 💡 Tambahkan ini untuk membersihkan data sebelumnya
        ClearPreviousProgress();

        WWWForm form = new WWWForm();
        form.AddField("nisn", nisn);
        form.AddField("password", passwordInput.text);

        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Server Response: " + www.downloadHandler.text);
                LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);

                if (response.status == "success")
                {
                    ShowNotification("Login berhasil!", Color.green);
                    Debug.Log("Login berhasil!");

                    PlayerPrefs.SetString("nisn", nisn);
                    PlayerPrefs.SetInt("IsLoggedIn", 1);
                    PlayerPrefs.Save();

                    GameManager.Instance.SetNISN(nisn);

                    // Ambil progress setelah login berhasil
                    string progressUrlWithParam = progressURL + "?nisn=" + nisn;
                    using (UnityWebRequest progressRequest = UnityWebRequest.Get(progressUrlWithParam))
                    {
                        yield return progressRequest.SendWebRequest();

                        if (progressRequest.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log("Progress Response: " + progressRequest.downloadHandler.text);
                            ProgressResponse progress = JsonConvert.DeserializeObject<ProgressResponse>(progressRequest.downloadHandler.text);

                            if (progress != null && progress.status == "success")
                            {
                                // Cek apakah data progres valid, jika tidak, gunakan default
                                int level = progress.id_level > 0 ? progress.id_level : 1;
                                int stage = progress.id_stage > 0 ? progress.id_stage : 1;
                                int totalSkor = progress.total_skor;

                                GameManager.Instance.SetLevel(level);
                                GameManager.Instance.SetStage(stage);
                                GameManager.Instance.SetTotalSkor(totalSkor);

                                PlayerPrefs.SetInt("level", level);
                                PlayerPrefs.SetInt("stage", stage);
                                PlayerPrefs.SetInt("total_skor", totalSkor);

                                // Menyimpan stage-stage tertinggi per level
                                if (progress.highest_stages != null)
                                {
                                    GameManager.Instance.SetAllHighestStages(progress.highest_stages);

                                    foreach (var entry in progress.highest_stages)
                                    {
                                        string stageKey = $"highest_stage_level_{entry.Key}";
                                        PlayerPrefs.SetInt(stageKey, entry.Value);
                                        Debug.Log($"[Login] Set {stageKey} = {entry.Value}");
                                    }

                                    // Set highest_stage untuk level saat ini
                                    string currentKey = level.ToString();
                                    if (progress.highest_stages.ContainsKey(currentKey))
                                    {
                                        int highestStage = progress.highest_stages[currentKey];
                                        PlayerPrefs.SetInt("highest_stage", highestStage);
                                        GameManager.Instance.SetHighestStage(highestStage);
                                    }
                                }
                                else
                                {
                                    Debug.Log("[Login] highest_stages kosong. Ini mungkin akun baru.");
                                    // Inisialisasi default (opsional)
                                    Dictionary<string, int> defaultStages = new Dictionary<string, int>
                                    {
                                        { level.ToString(), 1 }
                                    };
                                    GameManager.Instance.SetAllHighestStages(defaultStages);
                                    PlayerPrefs.SetInt("highest_stage", 1);
                                }

                                // ✅ Unlock berdasarkan total_skor dan highest_stage
                                UnlockNextLevelsBasedOnProgress(progress);

                                // ✅ Baru setelah unlock, kita hitung ulang highest_level
                                int finalHighestLevel = 1;
                                if (PlayerPrefs.GetInt("unlocked_level_3", 0) == 1) finalHighestLevel = 3;
                                else if (PlayerPrefs.GetInt("unlocked_level_2", 0) == 1) finalHighestLevel = 2;

                                PlayerPrefs.SetInt("highest_level", finalHighestLevel);
                                Debug.Log($"[Login] (Recalculate) Set highest_level = {finalHighestLevel}");

                                PlayerPrefs.Save();

                            }
                            else
                            {
                                Debug.Log("[Login] Progres tidak ditemukan, inisialisasi akun baru.");

                                // Inisialisasi default untuk akun baru
                                GameManager.Instance.SetLevel(1);
                                GameManager.Instance.SetStage(1);
                                GameManager.Instance.SetTotalSkor(0);
                                GameManager.Instance.SetHighestStage(1);
                                GameManager.Instance.SetAllHighestStages(new Dictionary<string, int> { { "1", 1 } });

                                PlayerPrefs.SetInt("level", 1);
                                PlayerPrefs.SetInt("stage", 1);
                                PlayerPrefs.SetInt("total_skor", 0);
                                PlayerPrefs.SetInt("highest_stage", 1);
                                PlayerPrefs.SetInt("highest_stage_level_1", 1);
                                PlayerPrefs.SetInt("highest_level", 1); // <--- Tambahan ini
                                PlayerPrefs.Save();
                            }
                        }
                    }

                    yield return new WaitForSeconds(1f);
                    SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
                }
                else if (response.status == "incorrect_password")
                {
                    ShowNotification("Password salah! Coba lagi.", Color.red);
                    Debug.LogError("Password salah!");
                }
                else
                {
                    ShowNotification("Login gagal. Periksa username dan password Anda.", Color.red);
                    Debug.LogError("Login request failed: " + response.message);
                }
            }
            else
            {
                ShowNotification("Koneksi gagal!", Color.red);
                Debug.LogError("Login request failed: " + www.error);
            }
        }
    }

    private void UnlockNextLevelsBasedOnProgress(ProgressResponse progress)
    {
        int totalSkor = progress.total_skor;
        Dictionary<string, int> highestStages = progress.highest_stages;

        // Misal aturan buka level:
        // Level 2 (sedang) = minimal stage 5 di level 1 dan skor >= 40
        // Level 3 (sulit) = minimal stage 5 di level 2 dan skor >= 60
        bool level2Unlocked = highestStages.ContainsKey("1") && highestStages["1"] >= 5 && totalSkor >= 40;
        bool level3Unlocked = highestStages.ContainsKey("2") && highestStages["2"] >= 5 && totalSkor >= 60;

        PlayerPrefs.SetInt("unlocked_level_2", level2Unlocked ? 1 : 0);
        PlayerPrefs.SetInt("unlocked_level_3", level3Unlocked ? 1 : 0);

        Debug.Log($"Level 2 unlocked: {level2Unlocked}, Level 3 unlocked: {level3Unlocked}");
        PlayerPrefs.Save();
    }

    private void ClearPreviousProgress()
    {
        // Hapus semua progress terkait level dan stage
        PlayerPrefs.DeleteKey("level");
        PlayerPrefs.DeleteKey("stage");
        PlayerPrefs.DeleteKey("total_skor");
        PlayerPrefs.DeleteKey("highest_stage");
        PlayerPrefs.DeleteKey("highest_level");
        PlayerPrefs.DeleteKey("unlocked_level_2");
        PlayerPrefs.DeleteKey("unlocked_level_3");

        // Hapus juga per-level stage tertinggi yang tersimpan
        for (int i = 1; i <= 3; i++)
        {
            PlayerPrefs.DeleteKey($"highest_stage_level_{i}");
        }

        PlayerPrefs.Save();
    }



    private void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        GameManager.Instance.ResetSkor();
        Debug.Log("Semua data PlayerPrefs telah direset!");
    }

    private void ShowNotification(string message, Color color)
    {
        if (bgNotif != null && errorLog != null)
        {
            bgNotif.SetActive(true);
            errorLog.text = message;
            errorLog.color = color;

            CancelInvoke("HideNotification");
            Invoke("HideNotification", 3f);
        }
    }

    private void HideNotification()
    {
        if (bgNotif != null)
        {
            bgNotif.SetActive(false);
        }
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string status;
        public string message;
    }
    [System.Serializable]
    public class ProgressResponse 
    {
        public string status;
        public int id_level;
        public int id_stage;
        public int total_skor;
        public Dictionary<string, int> highest_stages; // Ubah key menjadi string
    }
}
