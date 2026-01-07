using System.Collections;
using UnityEngine;

public class PanelLoadingManager : MonoBehaviour
{
    public GameObject panelLoading; // Referensi ke Panel Loading

    void Start()
    {
        panelLoading.SetActive(true); // Tampilkan loading saat scene gameplay dimulai
        StartCoroutine(HideLoadingPanel());
    }

    private IEnumerator HideLoadingPanel()
    {
        yield return new WaitForSeconds(3f); // Tunggu waktu loading soal
        panelLoading.SetActive(false); // Sembunyikan Panel Loading setelah soal siap
    }
}