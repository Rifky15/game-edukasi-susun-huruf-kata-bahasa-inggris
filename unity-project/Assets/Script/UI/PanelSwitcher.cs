using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject panelWelcome; // Panel Selamat Datang
    public GameObject panelLogin;   // Panel Login
    public Button nextButton;       // Tombol Next untuk pindah panel
    public float delayBeforeEnableNextButton = 3f; // Waktu delay sebelum tombol Next muncul
    public float delayBeforeSwitch = 5f; // Waktu delay sebelum berpindah ke login otomatis

    void Start()
    {
        panelWelcome.SetActive(true);  // Panel Welcome aktif saat start
        panelLogin.SetActive(false);   // Panel Login disembunyikan
        nextButton.gameObject.SetActive(false); // Sembunyikan tombol Next saat start

        StartCoroutine(EnableNextButtonAfterDelay());
        StartCoroutine(SwitchToLoginPanel());
    }

    IEnumerator EnableNextButtonAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeEnableNextButton);
        nextButton.gameObject.SetActive(true); // Tampilkan tombol Next setelah delay
    }

    IEnumerator SwitchToLoginPanel()
    {
        yield return new WaitForSeconds(delayBeforeSwitch);
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        panelWelcome.SetActive(false); // Sembunyikan Welcome Panel
        panelLogin.SetActive(true);    // Tampilkan Login Panel
    }
}
