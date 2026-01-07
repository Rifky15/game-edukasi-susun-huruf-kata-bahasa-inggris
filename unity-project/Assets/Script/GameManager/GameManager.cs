using UnityEngine;
using UnityEngine.SceneManagement; // Tambahkan ini
using System;
using System.Collections.Generic; // Tambahkan ini

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int jumlahBuku;
    private float waktuSisa;
    private const int waktuPeningkatan = 600;
    private const int maxBuku = 5;

    private bool isReading;
    private float readingTimeRemaining;

    private int skorTotal;

    private string nisn;

    private int skorStage; // skor hanya untuk 1 stage aktif
    private int currentLevelId;
    private int currentStageId;
    private int highestStage;

    // public AudioSource bgmAudio; // Tambahkan ini!

    public Dictionary<string, int> highestStagesAllLevel = new Dictionary<string, int>();

    private int skorPerStage = 0;
    // public int totalPoin = 0;
    // public int poinSementara = 0;
    private int jawabanBenarBeruntun = 0;

    // public Text textTotalPoin;
    // public Text textSkorSementara;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Load data dari PlayerPrefs
        nisn = PlayerPrefs.GetString("nisn", "");
        jumlahBuku = PlayerPrefs.GetInt("jumlahBuku", maxBuku);
        waktuSisa = PlayerPrefs.GetFloat("waktuSisa", waktuPeningkatan);
        skorTotal = PlayerPrefs.GetInt("SkorTotal", 0);
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateBuku), 0f, 1f);
        LoadTime();
    }

    private void Update()
    {
        if (isReading && jumlahBuku < maxBuku)
        {
            readingTimeRemaining -= Time.deltaTime;
            if (readingTimeRemaining <= 0f)
            {
                TambahBuku(1);
                readingTimeRemaining = 60f;
            }
        }
    }

    //-----baru----
    public void TambahSkorJikaBenar()
    {
        jawabanBenarBeruntun++;

        if (jawabanBenarBeruntun % 3 == 0) // Setiap 3 jawaban benar berturut-turut
        {
            skorStage += 20;  // Tambahkan ke skor stage saat ini
            skorTotal += 20;  // Tambahkan ke skor total keseluruhan

            Debug.Log($"Bonus skor +20! Skor per stage: {skorStage}, Total skor: {skorTotal}");
        }

        // **Tambahan: Cek jika semua soal dalam stage dijawab benar**
        int totalSoal = 6; // Soal per stage
        if (jawabanBenarBeruntun >= totalSoal) // Jika semua benar dalam satu stage
        {
            skorStage += 30; // Bonus skor sempurna
            skorTotal += 30;
            Debug.Log($"Bonus skor +30! Semua soal benar di stage ini! Skor per stage: {skorStage}, Total skor: {skorTotal}");

            jawabanBenarBeruntun = 0; // Reset penghitung setelah bonus diberikan
        }
    }

    // **Reset penghitung saat jawaban salah**
    public void ResetJawabanBenar()
    {
        jawabanBenarBeruntun = 0;
    }

    public void SetStageInfo(int levelId, int stageId)
    {
        currentLevelId = levelId;
        currentStageId = stageId;
        skorStage = 0; // reset saat mulai stage baru
    }

    public void TambahSkorStage(int poin)
    {
        skorStage += poin;
    }

    public int GetSkorStage()
    {
        return skorStage;
    }

    public int GetCurrentLevelId()
    {
        return currentLevelId;
    }

    public int GetCurrentStageId()
    {
        return currentStageId;
    }

    // ------------------ Buku (Nyawa) ------------------

    private void UpdateBuku()
    {
        if (jumlahBuku >= maxBuku)
        {
            waktuSisa = waktuPeningkatan;
            PlayerPrefs.SetFloat("waktuSisa", waktuSisa);
            return;
        }

        waktuSisa -= 1f;

        if (waktuSisa <= 0f)
        {
            jumlahBuku = Mathf.Min(jumlahBuku + 1, maxBuku);
            PlayerPrefs.SetInt("jumlahBuku", jumlahBuku);

            if (jumlahBuku < maxBuku)
                waktuSisa = waktuPeningkatan;
            else
                waktuSisa = waktuPeningkatan;
        }

        PlayerPrefs.SetFloat("waktuSisa", waktuSisa);
    }

    public void KurangiBuku()
    {
        if (jumlahBuku > 0)
        {
            jumlahBuku--;
            PlayerPrefs.SetInt("jumlahBuku", jumlahBuku);

            if (jumlahBuku <= 0)
            {
                // Trigger GameOver dari UI handler
                UI_GameOverTrigger();
            }
        }
    }

    public void TambahBuku(int jumlah)
    {
        jumlahBuku += jumlah;
        jumlahBuku = Mathf.Clamp(jumlahBuku, 0, maxBuku);
        PlayerPrefs.SetInt("jumlahBuku", jumlahBuku);
    }

    public int GetJumlahBuku()
    {
        return jumlahBuku;
    }

    // ------------------ Skor ------------------

    public void TambahSkor(int jumlah)
    {
        skorTotal += jumlah;
        PlayerPrefs.SetInt("SkorTotal", skorTotal);
        PlayerPrefs.Save();
    }

    public void KurangiSkor(int penalti)
    {
        skorTotal = Mathf.Max(0, skorTotal - penalti);
        skorPerStage = Mathf.Max(0, skorPerStage - penalti); // tambahkan ini
        PlayerPrefs.SetInt("SkorTotal", skorTotal);
        PlayerPrefs.Save();
    }

    public void ResetSkor()
    {
        skorTotal = 0;
        PlayerPrefs.SetInt("SkorTotal", skorTotal);
        PlayerPrefs.Save();
    }

    public int GetSkorTotal()
    {
        return skorTotal;
    }

    //------------------- SkorPerSatge-----------------
    public int GetSkorPerStage()
    {
        return skorPerStage;
    }

    public void SetSkorPerStage(int nilai)
    {
        skorPerStage = nilai;
        PlayerPrefs.SetInt("SkorPerStage", skorPerStage);
    }

    public void TambahSkorPerStage(int jumlah)
    {
        skorPerStage += jumlah;
    }

    public void KurangiSkorPerStage(int jumlah)
    {
        skorPerStage = Mathf.Max(0, skorPerStage - jumlah);
    }
    public void ResetSkorPerStage()
    {
        skorPerStage = 0;  // Reset skor per stage
        PlayerPrefs.SetInt("SkorPerStage", skorPerStage);
        PlayerPrefs.Save();
    }


    // ------------------ Reading Session ------------------

    public void StartReadingSession()
    {
        isReading = true;
        readingTimeRemaining = 60f;
    }

    public void StopReadingSession()
    {
        isReading = false;
        readingTimeRemaining = 0f;
    }

    // ------------------ Offline Time Support ------------------

    // private void OnApplicationPause(bool pause)
    // {
    //     if (pause) SaveTime();
    //     else LoadTime();
    // }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveTime();
        else
        {
            LoadTime();

            // Tambahkan pemicu agar MainMenu bisa tahu
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                mainmenu main = FindObjectOfType<mainmenu>();
                if (main != null) main.ForceRefreshUI();
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveTime();
    }

    private void SaveTime()
    {
        PlayerPrefs.SetString("lastExitTime", DateTime.Now.ToString());
        PlayerPrefs.SetFloat("waktuSisa", waktuSisa);
        PlayerPrefs.Save();
    }

    private void LoadTime()
    {
        if (PlayerPrefs.HasKey("lastExitTime"))
        {
            string lastExitTimeStr = PlayerPrefs.GetString("lastExitTime");
            DateTime lastExitTime = DateTime.Parse(lastExitTimeStr);
            TimeSpan timeAway = DateTime.Now - lastExitTime;

            float waktuTerlewat = (float)timeAway.TotalSeconds;

            if (jumlahBuku < maxBuku)
            {
                waktuSisa -= waktuTerlewat;

                while (waktuSisa <= 0f && jumlahBuku < maxBuku)
                {
                    jumlahBuku++;
                    waktuSisa += waktuPeningkatan;
                }

                if (jumlahBuku >= maxBuku)
                    waktuSisa = waktuPeningkatan;

                PlayerPrefs.SetInt("jumlahBuku", jumlahBuku);
                PlayerPrefs.SetFloat("waktuSisa", waktuSisa);
            }
        }
    }

    public float GetWaktuSisa()
    {
        return waktuSisa;
    }

    // ------------------ NISN ------------------

    public void SetNISN(string value)
    {
        nisn = value;
        PlayerPrefs.SetString("nisn", nisn);
    }

    public string GetNISN()
    {
        if (string.IsNullOrEmpty(nisn))
        {
            nisn = PlayerPrefs.GetString("nisn", "");
        }
        return nisn;
    }
    
    // ------------------ method eksplisit ------------------
    // Untuk kompatibilitas dengan login.cs
    public void SetLevel(int level)
    {
        currentLevelId = level;
    }

    public void SetStage(int stage)
    {
        currentStageId = stage;
    }

    public void SetTotalSkor(int total)
    {
        skorTotal = total;
        PlayerPrefs.SetInt("SkorTotal", total);
    }

    // Untuk kompatibilitas dengan mainmenu.cs
    public int GetCurrentLevel()
    {
        return currentLevelId;
    }

    public int GetCurrentStage()
    {
        return currentStageId;
    }

    
    public void SetProgress(ProgressResponse progress)
    {
        GameManager.Instance.SetLevel(progress.id_level);
        GameManager.Instance.SetStage(progress.id_stage);
        GameManager.Instance.SetTotalSkor(progress.total_skor);

        PlayerPrefs.SetInt("level", progress.id_level);
        PlayerPrefs.SetInt("stage", progress.id_stage);
        PlayerPrefs.SetInt("total_skor", progress.total_skor);

        // Menyimpan highest stages dari response
        PlayerPrefs.SetString("highest_stages", JsonUtility.ToJson(progress.highest_stages));  // Serialize ke string
        PlayerPrefs.Save();
    }

    // Setter untuk highestStage
    public void SetHighestStage(int stage)
    {
        highestStage = stage;
    }

    // Getter untuk highestStage
    public int GetHighestStage()
    {
        return highestStage;
    }

    public void SetAllHighestStages(Dictionary<string, int> data)
    {
        highestStagesAllLevel = data;
    }

    // ------------------ Game Over Trigger Placeholder ------------------

    private void UI_GameOverTrigger()
    {
        Debug.Log("Game Over: Buku habis!");
        // Panggil dari script LifeSystem.cs yang UI handler
        // LifeSystem.Instance.TampilkanPanelGameOver();
    }
}
