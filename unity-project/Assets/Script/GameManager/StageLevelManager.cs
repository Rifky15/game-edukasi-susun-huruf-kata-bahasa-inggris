using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class StageLevelManager : MonoBehaviour
{
    public static StageLevelManager Instance { get; private set; }

    private int level;

    private int highestLevel;
    private const int maxStagePerLevel = 5;
    private const int maxLevel = 3;

    private int stage;
    private int highestStage;
    private int idStage;
    public int jumlahSoalPerStage = 6;
    private List<Soal> soalSaatIni;
    private string apiUrl = "http://yourserver.com/get_stage_id.php"; // Ganti dengan URL-mu

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Aktifkan jika ingin bertahan antar scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadStageData();
        int currentLevel = PlayerPrefs.GetInt("level", 1); // Ambil level saat ini
        Debug.Log($"Memuat scene Stage dengan Level: {currentLevel}");
        if (UnlockStageButtons.Instance != null)
            {
                UnlockStageButtons.Instance.LoadStageButtons(currentLevel);
            }
            else
            {
                Debug.LogWarning("UnlockStageButtons tidak ditemukan di scene ini.");
            }
        Debug.Log($"[StageLevelManager] Loaded Level: {level}, Stage: {stage}");
    }

    private void LoadStageData()
    {
        level = PlayerPrefs.GetInt("level", 1);
        stage = PlayerPrefs.GetInt("stage", 1);
        highestStage = PlayerPrefs.GetInt("highest_stage", 1);
        highestLevel = PlayerPrefs.GetInt("highest_level", 1);
    }


    // ================= Soal & Progress Stage =================

    public List<Soal> AmbilSoalStage()
    {
        // Debug.Log("Ambil soal untuk Level: " + level + ", Stage: " + stage);
        return SoalManager.Instance.GetSoalUntukStage(jumlahSoalPerStage); 
    }

    public List<Soal> GetSoalSaatIni()
    {
        return soalSaatIni;
    }

    public int GetJumlahPengelabuan()
    {
        LoadStageData(); // Pastikan data stage diperbarui sebelum digunakan
        Debug.Log("Stage setelah LoadStageData: " + stage); // Tes apakah stage sudah benar

        int baseJumlah = 2; // Stage 1 harus memiliki 2 pengelabuan
        int tambahanPerStage = 1; // Setiap stage bertambah 1 pengelabuan

        // Memastikan bahwa stage 1 memiliki 2 pengelabuan, stage 2 memiliki 3, dst.
        int jumlahPengelabuan = baseJumlah + (stage - 1) * tambahanPerStage; 

        // Batas maksimal pengelabuan
        jumlahPengelabuan = Mathf.Min(jumlahPengelabuan, 10);

        Debug.Log($"Jumlah pengelabuan untuk stage {stage}: {jumlahPengelabuan}");
        return jumlahPengelabuan;
    }

    public void CompleteStage()
    {
        if (stage < maxStagePerLevel)
        {
            stage++;
        }

        if (stage > maxStagePerLevel)
        {
            stage = maxStagePerLevel; // Maksimal stage 5
        }

        int savedHighestStage = PlayerPrefs.GetInt("highest_stage", 1);
        if (stage > savedHighestStage)
        {
            PlayerPrefs.SetInt("highest_stage", stage);
        }

        // ======= Tambahan logika buka level =======
        if (stage == maxStagePerLevel && level < maxLevel)
        {
            int savedHighestLevel = PlayerPrefs.GetInt("highest_level", 1);
            if (level + 1 > savedHighestLevel)
            {
                PlayerPrefs.SetInt("highest_level", level + 1);
                PlayerPrefs.Save(); // << TAMBAHKAN INI
                Debug.Log("Level berikutnya terbuka: Level " + (level + 1));
            }
        }

        PlayerPrefs.SetInt("stage", stage);
        PlayerPrefs.Save();

        int currentLevel = PlayerPrefs.GetInt("level", 1);
        if (UnlockStageButtons.Instance != null)
            UnlockStageButtons.Instance.LoadStageButtons(currentLevel);

        Debug.Log("Stage " + stage + " Completed!");
        Debug.Log($"[CompleteStage] Current Level: {level}, Stage: {stage}");
        Debug.Log($"[CompleteStage] highest_level: {PlayerPrefs.GetInt("highest_level")}");

    }
    
    public int GetHighestLevel()
    {
        return PlayerPrefs.GetInt("highest_level", 1);
    }

    private IEnumerator FetchStageIDFromDatabase()
    {
        WWWForm form = new WWWForm();
        form.AddField("level", level);
        form.AddField("stage", stage);

        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                if (int.TryParse(www.downloadHandler.text, out idStage))
                {
                    Debug.Log("Stage ID Retrieved: " + idStage);
                }
                else
                {
                    Debug.LogError("Failed to parse Stage ID");
                }
            }
            else
            {
                Debug.LogError("Error fetching stage ID: " + www.error);
            }
        }
    }

    public void MulaiStageBerikutnya()
    {
        CompleteStage();  // Selesaikan stage dan buka stage berikutnya
        // Tambahkan logika lain jika diperlukan
        Debug.Log("Stage berikutnya dimulai: Level " + level + ", Stage " + stage);
    }

    public int GetStageID()
    {
        return idStage;
    }

    public void PilihLevelDanStage(int levelDipilih, int stageDipilih)
    {
        level = levelDipilih;
        stage = stageDipilih;
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("stage", stage);
        PlayerPrefs.Save();
    }

    // Fungsi untuk memeriksa apakah stage sudah dibuka
    public bool IsStageUnlocked(int stage)
    {
        return stage <= PlayerPrefs.GetInt("stage", 1);  // Stage terbuka jika stage kurang dari atau sama dengan stage yang disimpan
    }

    // ================= Level Utility =================

    public float GetTimeLimitForLevel(string level)
    {
        string kategori = PlayerPrefs.GetString("kategori", "kosakata");

        if (kategori == "kosakata")
        {
            switch (level)
            {
                case "mudah": return 60f;
                case "sedang": return 45f;
                case "sulit": return 30f;
            }
        }
        else if (kategori == "frasa")
        {
            switch (level)
            {
                case "mudah": return 75f;
                case "sedang": return 50f;
                case "sulit": return 35f;
            }
        }
        return 60f; // Default jika tidak ditemukan
    }

    // Mendapatkan poin per jawaban benar berdasarkan level
    public int GetPointsForCorrectAnswer(string level)
    {
        // string level = PlayerPrefs.GetString("level", "mudah");

        switch (level)
        {
            case "mudah": return 10;
            case "sedang": return 20;
            case "sulit": return 30;
            default: return 10;
        }
    }

    // Mendapatkan penalti poin untuk jawaban salah berdasarkan level
    public int GetPenaltyForWrongAnswer(string level)
    {
        // string level = PlayerPrefs.GetString("level", "mudah");

        switch (level)
        {
            case "mudah": return 5;
            case "sedang": return 10;
            case "sulit": return 15;
            default: return 5;
        }
    }
    public string getLevel(){
        int level = PlayerPrefs.GetInt("level", 1);
        switch (level)
        {
            case 1: return "mudah";
            case 2: return "sedang";
            case 3: return "sulit";
            default: return "mudah";
        }
    }
    
    public int GetStage()
    {
        return PlayerPrefs.GetInt("stage", 1); // Ambil langsung dari PlayerPrefs
    }
}