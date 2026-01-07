using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Materi materi;      // Data materi yang di-set dari ListMateriManager
    public int materiIndex;    // Indeks atau informasi tambahan

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) 
        {
            Debug.Log("⏳ Skip klik karena sedang scroll");
            return;
        }

        Debug.Log($"🖱 Klik Materi: ID Frasa = {materi.id_frasa}, ID Kosakata = {materi.id_kosakata}");

        // Update indexMateriSekarang dengan ID yang sesuai
        GlobalMateriData.indexMateriSekarang = GlobalMateriData.semuaMateri.FindIndex(m => 
            (materi.id_frasa > 0 && m.id_frasa == materi.id_frasa) || 
            (materi.id_kosakata > 0 && m.id_kosakata == materi.id_kosakata));

        Debug.Log($"🔄 Index yang ditemukan berdasarkan klik: {GlobalMateriData.indexMateriSekarang}");

        if (GlobalMateriData.indexMateriSekarang == -1) {
            Debug.LogError("❌ ID Materi Tidak Ditemukan dalam ListMateri! Cek apakah data sudah benar.");
            return;
        }

        // Simpan index dan ID Kosakata ke PlayerPrefs agar konsisten saat pindah scene
        PlayerPrefs.SetInt("indexMateriSekarang", GlobalMateriData.indexMateriSekarang);
        PlayerPrefs.SetInt("materiId", materi.id_kosakata);
        PlayerPrefs.Save();

        if (PlayerPrefs.GetString("pilihan_materi") == "Reading")
        {
            SceneManager.LoadScene("Reading");
        }
        else if (PlayerPrefs.GetString("pilihan_materi") == "Listening")
        {
            SceneManager.LoadScene("ListeningScene");
        }
    }
}
