using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonLevel : MonoBehaviour
{
    public Button[] levelButtons; // assign di Inspector
    private int highestLevel;

    private void Start()
    {
        highestLevel = PlayerPrefs.GetInt("highest_level", 1); // Default level 1 terbuka
        Debug.Log($"[ButtonLevel] Highest level unlocked: {highestLevel}");
        Debug.Log($"[ButtonLevel] Di-start ulang. highest_level sekarang: {PlayerPrefs.GetInt("highest_level")}");


        UpdateLevelButtons();
    }

    public void PilihLevel(int level)
    {
        if (level > highestLevel)
        {
            Debug.LogWarning($"[ButtonLevel] Level {level} belum terbuka!");
            return;
        }

        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.Save();

        Debug.Log($"[ButtonLevel] Level {level} disimpan ke PlayerPrefs.");
        SceneTransitionManager.Instance.LoadSceneWithFade("Stage"); // ke scene stage
    }

    public void ButtonBack()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            bool unlocked = (levelNumber <= highestLevel);

            levelButtons[i].interactable = unlocked;

            Debug.Log($"[ButtonLevel] Level {levelNumber} is interactable: {unlocked}");
        }
    }

    // Fungsi dipanggil dari luar saat menyelesaikan level
    public static void CompleteLevel(int level)
    {
        int currentHighest = PlayerPrefs.GetInt("highest_level", 1);
        if (level >= currentHighest)
        {
            PlayerPrefs.SetInt("highest_level", level + 1);
            PlayerPrefs.Save();
            Debug.Log($"[ButtonLevel] Level {level} selesai! Level {level + 1} dibuka.");
        }
    }
}
