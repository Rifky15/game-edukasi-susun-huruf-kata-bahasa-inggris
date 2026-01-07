using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Tambahkan namespace TextMeshPro

public class PanelInformasi : MonoBehaviour
{
    public GameObject panelPetunjuk;  
    public GameObject panelInfoGame;  
    public Button buttonPetunjuk;  
    public Button buttonInfoGame;  
    public TMP_Text textPetunjuk; // Referensi ke teks dalam tombol Petunjuk
    public TMP_Text textInfoGame; // Referensi ke teks dalam tombol InfoGame
    public CanvasGroup canvasPetunjuk;  
    public CanvasGroup canvasInfoGame;  

    void Start()
    {
        ShowPetunjuk();
    }

    public void ShowPetunjuk()
    {
        StartCoroutine(FadePanel(canvasPetunjuk, true));
        StartCoroutine(FadePanel(canvasInfoGame, false));

        panelPetunjuk.SetActive(true);
        panelInfoGame.SetActive(false);

        UpdateButtonStyle(buttonPetunjuk, textPetunjuk, true);
        UpdateButtonStyle(buttonInfoGame, textInfoGame, false);
    }

    public void ShowInfoGame()
    {
        StartCoroutine(FadePanel(canvasPetunjuk, false));
        StartCoroutine(FadePanel(canvasInfoGame, true));

        panelPetunjuk.SetActive(false);
        panelInfoGame.SetActive(true);

        UpdateButtonStyle(buttonPetunjuk, textPetunjuk, false);
        UpdateButtonStyle(buttonInfoGame, textInfoGame, true);
    }

    public void BackToMainMenu()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
    }

    private void UpdateButtonStyle(Button btn, TMP_Text textComponent, bool isActive)
    {
        if (btn == null || textComponent == null) return;

        ColorBlock colors = btn.colors;
        colors.normalColor = isActive ? Color.white : new Color(0.2f, 0.2f, 0.2f);
        colors.highlightedColor = isActive ? Color.white : new Color(0.3f, 0.3f, 0.3f);
        colors.pressedColor = Color.gray;
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        btn.colors = colors;

        // Mengubah warna teks pada tombol
        textComponent.color = isActive ? Color.black : Color.white;
    }

    private IEnumerator FadePanel(CanvasGroup canvasGroup, bool fadeIn)
    {
        float duration = 0.3f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}