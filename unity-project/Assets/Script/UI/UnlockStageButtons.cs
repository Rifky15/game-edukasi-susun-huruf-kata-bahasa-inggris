using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UnlockStageButtons : MonoBehaviour
{
    public Transform stageButtonParent; // GameObject parent yang berisi semua tombol stage
    private Button[] stageButtons;  // Array tombol stage
    private int highestStage;
    public static UnlockStageButtons Instance;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Stage")
        {
            return; // Tidak perlu dihapus, cukup hentikan eksekusi
        }

        stageButtons = stageButtonParent.GetComponentsInChildren<Button>();

        int currentLevel = PlayerPrefs.GetInt("level", 1); // Ambil level saat ini dari Playe
        
        highestStage = PlayerPrefs.GetInt("highest_stage", 1);
        Debug.Log($"HighestStage terbaru: {highestStage}");

        LoadStageButtons(currentLevel); // Load tombol stage berdasarkan data terbaru
    }

    // public void CompleteStage(int stageNumber)
    // {
    //     // Update highestStage jika stage yang baru lebih tinggi
    //     if (stageNumber >= highestStage)
    //     {
    //         highestStage = stageNumber + 1;
    //         PlayerPrefs.SetInt("highest_stage", highestStage);
    //         PlayerPrefs.Save();
    //     }

    //     Debug.Log("Stage " + stageNumber + " Completed!");
        
    //     // Perbarui status tombol untuk menunjukkan stage baru yang bisa dibuka
    //     LoadStageButtons();
    // }

    public void CompleteStage(int level, int stageNumber)
    {
        string stageKey = $"highest_stage_level_{level}";
        int highestStage = PlayerPrefs.GetInt(stageKey, 1);
        
        if (stageNumber >= highestStage)
        {
            highestStage = stageNumber + 1;
            PlayerPrefs.SetInt(stageKey, highestStage);
            PlayerPrefs.Save();
        }

        Debug.Log($"Level {level} - Stage {stageNumber} Completed! Highest Stage di level ini: {highestStage}");
    }

    // public void LoadStageButtons()
    // {
    //     highestStage = PlayerPrefs.GetInt("highest_stage", 1);
    //     Debug.Log("Highest Stage from PlayerPrefs: " + highestStage);
        
    //     // Debug untuk memeriksa status setiap tombol
    //     for (int i = 0; i < stageButtons.Length; i++) // Stage dimulai dari index 0 (stage 1)
    //     {
    //         // Tombol stage pertama selalu bisa diklik
    //         if (i == 0)
    //         {
    //             stageButtons[i].interactable = true;  // Stage 1 selalu bisa diakses
    //         }
    //         else
    //         {
    //             // Tombol hanya bisa diklik jika stage sudah terbuka (sesuai highestStage)
    //             stageButtons[i].interactable = (i < highestStage);
    //         }

    //         // Debug untuk memeriksa interaktivitas tiap tombol
    //         Debug.Log("Stage " + (i + 1) + " (index " + i + ") is interactable: " + stageButtons[i].interactable);
    //     }
    // }

    public void LoadStageButtons(int level)
    {
        string stageKey = $"highest_stage_level_{level}";
        highestStage = PlayerPrefs.GetInt(stageKey, 1);
        
        Debug.Log($"Highest Stage dari PlayerPrefs untuk Level {level}: {highestStage}");

        for (int i = 0; i < stageButtons.Length; i++)
        {
            stageButtons[i].interactable = (i + 1 <= highestStage);
            Debug.Log($"Level {level} - Stage {i + 1} bisa diklik? {stageButtons[i].interactable}");
        }
    }

    public void ButtonBack()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("PilihanLvl");  // Pindah ke scene PilihanLvl
    }

    public void PilihStage(int stage)
    {
        PlayerPrefs.SetInt("stage", stage);
        PlayerPrefs.Save();

        if (StageLevelManager.Instance != null)
        {
            int level = PlayerPrefs.GetInt("level", 1);
            StageLevelManager.Instance.PilihLevelDanStage(level, stage);
        }

        Debug.Log($"Stage {stage} telah disimpan!");
        SceneTransitionManager.Instance.LoadSceneWithFade("GamePlay");  // Pindah ke scene GamePlay
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Stage") // Ganti dengan nama scene stage kamu
        {
            // Cari ulang stageButtonParent dan update tombol
            stageButtonParent = GameObject.Find("bgstage")?.transform;

            if (stageButtonParent != null)
            {
                stageButtons = stageButtonParent.GetComponentsInChildren<Button>();
                
                int currentLevel = PlayerPrefs.GetInt("level", 1); // Ambil level saat ini dari PlayerPrefs
                LoadStageButtons(currentLevel); // Kirim level sebagai parameter
            
                Debug.Log("[UnlockStageButtons] Reloaded buttons after scene change.");
            }
            else
            {
                Debug.LogWarning("[UnlockStageButtons] stageButtonParent not found in Stage scene.");
            }
        }
    }
}
