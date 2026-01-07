using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class JawabanManager : MonoBehaviour
{
    public static JawabanManager Instance { get; private set; }
    private List<JawabanUser> jawabanSementara = new List<JawabanUser>();
    private int idStage=1;
    private int idUser;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        idUser = PlayerPrefs.GetInt("id_user", 0);
        string level = PlayerPrefs.GetString("level", "mudah");
        int stage = PlayerPrefs.GetInt("stage", 1);
    }

    public void SimpanJawaban(int idSoal, string jawaban)
    {
        JawabanUser jawabanBaru = new JawabanUser
        {
            id_soal = idSoal,
            id_user = idUser,
            id_stage = idStage,
            jawaban = jawaban
        };

        jawabanSementara.Add(jawabanBaru);
    }

    public void CekSelesaiStage()
    {
        if (jawabanSementara.Count >= 5)
        {
            StartCoroutine(KirimJawabanKeDatabase());
        }
    }

    private IEnumerator KirimJawabanKeDatabase()
    {
        // Implementasi pengiriman data ke database melalui API atau sistem backend
        Debug.Log("Mengirim jawaban ke database...");

        string url = "https://vibeproject.web.id/GameEdukasiDB/BackEnd/kirim_jawaban.php"; // Ganti dengan URL backend kamu
        
        // Buat salinan list sebelum iterasi
        List<JawabanUser> jawabanUntukDikirim = new List<JawabanUser>(jawabanSementara);

        foreach (JawabanUser jawaban in jawabanUntukDikirim)
        {
            WWWForm form = new WWWForm();
            form.AddField("id_soal", jawaban.id_soal);
            form.AddField("id_user", jawaban.id_user);
            form.AddField("id_stage", jawaban.id_stage);
            form.AddField("jawaban", jawaban.jawaban);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Jawaban berhasil dikirim: " + jawaban.jawaban);
                }
                else
                {
                    Debug.LogError("Gagal mengirim jawaban: " + www.error);
                }
            }
        }

        // Hapus jawaban hanya setelah loop selesai
        jawabanSementara.Clear();
    }
}

[System.Serializable]
public class JawabanUser
{
    public int id_soal;
    public int id_user;
    public int id_stage;
    public string jawaban;
}
