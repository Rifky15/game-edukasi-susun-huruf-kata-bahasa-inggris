using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SoalManager : MonoBehaviour
{
    private int currentSoalIndex = 0;
    public int GetCurrentSoalIndex() => currentSoalIndex + 1; // +1 karena index mulai dari 0
    public int GetTotalSoal() => daftarSoal.Count;
    public void SetCurrentSoalIndex(int index) => currentSoalIndex = index;

    public static SoalManager Instance { get; private set; }
    private List<Soal> daftarSoal = new List<Soal>();
    private HashSet<int> soalTerpakai = new HashSet<int>();
    private string apiUrl = "http://localhost/GameEdukasiDB/BackEnd/get_soal.php"; // Ganti dengan URL API Anda

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AmbilSoalDariAPI();
        StartCoroutine(CekSoal());
    }

    private IEnumerator CekSoal()
    {
        yield return new WaitForSeconds(3f); // Tunggu proses API
        Debug.Log($"Jumlah soal tersedia: {daftarSoal.Count}");
    }

    public void AmbilSoalDariAPI()
    {
        int stage = PlayerPrefs.GetInt("stage", 1);
        int level = PlayerPrefs.GetInt("level", 1);

        StartCoroutine(GetSoalDenganParameter(apiUrl, stage, level));
    }

    private IEnumerator GetSoalDenganParameter(string url, int stage, int level)
    {
        WWWForm form = new WWWForm();
        form.AddField("stage", stage);
        form.AddField("level", level);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                SoalList soalList = JsonUtility.FromJson<SoalList>("{\"soal\":" + jsonData + "}");
                daftarSoal = new List<Soal>(soalList.soal);
                Debug.Log($"✅ Soal berhasil dimuat untuk stage {stage} level {level}. Jumlah soal: {daftarSoal.Count}");
            }
            else
            {
                Debug.LogError("❌ Gagal mengambil soal: " + request.error);
            }
        }
    }

    public List<Soal> GetSoalUntukStage(int jumlahSoal)
    {
        string levelSaatIni = StageLevelManager.Instance.getLevel();
        int stageSaatIni = StageLevelManager.Instance.GetStage();  // Dapatkan stage sebagai int

        Debug.Log($"Mencari soal untuk Level: {levelSaatIni} dan Stage: {stageSaatIni}");

        // Konversi stageSaatIni menjadi string untuk pencocokan
        string stageSaatIniString = stageSaatIni.ToString();  // Mengonversi int ke string

        // Filter soal berdasarkan level dan stage
        List<Soal> soalSesuaiLevel = daftarSoal
            .Where(soal => soal.level == levelSaatIni && soal.id_stage == stageSaatIniString)  // id_stage seharusnya bertipe string
            .ToList();

        Debug.Log($"Jumlah soal ditemukan: {soalSesuaiLevel.Count}");
        return soalSesuaiLevel.Take(jumlahSoal).ToList();
    }

    public void ResetSoalUntukStage()
    {
        soalTerpakai.Clear();
    }

    public string GetJawaban(Soal soal)
    {
        int jumlahPengelabuan = StageLevelManager.Instance.GetJumlahPengelabuan(); // Ambil jumlah sesuai stage

        if (soal.jenis_soal.ToLower() == "kosakata")
        {
            // Pisahkan vokal dan konsonan dari database dengan membersihkan spasi
            string[] dataPengelabuan = soal.pengelabuan.Split('|');
            string[] vokalList = dataPengelabuan[0].Replace("Vokal: ", "").Split(',').Select(v => v.Trim()).ToArray();
            string[] konsonanList = dataPengelabuan[1].Replace("Konsonan: ", "").Split(',').Select(k => k.Trim()).ToArray();

            // Ambil jumlah pengelabuan sesuai stage dengan distribusi vokal & konsonan yang seimbang
            int jumlahVokal = jumlahPengelabuan / 2;
            int jumlahKonsonan = jumlahPengelabuan - jumlahVokal;

            string[] vokalAcak = vokalList.OrderBy(x => UnityEngine.Random.value).Take(jumlahVokal).ToArray();
            string[] konsonanAcak = konsonanList.OrderBy(x => UnityEngine.Random.value).Take(jumlahKonsonan).ToArray();

            // Gabungkan hasil pengelabuan dan acak huruf
            string semuaPengelabuan = string.Concat(vokalAcak) + string.Concat(konsonanAcak);
            return AcakHuruf(soal.jawaban + semuaPengelabuan);
        }
        else if (soal.jenis_soal.ToLower() == "frasa")
        {
            // Pisahkan kata dan ambil jumlah yang sesuai dengan stage
            string[] semuaPengelabuan = soal.pengelabuan.Split(' ').Select(k => k.Trim()).ToArray();
            string[] pengelabuanTerpakai = semuaPengelabuan.OrderBy(x => UnityEngine.Random.value).Take(jumlahPengelabuan).ToArray();

            return AcakKata(soal.jawaban + " " + string.Join(" ", pengelabuanTerpakai));
        }

        return soal.jawaban;
    }

    private string AcakHuruf(string input)
    {
        char[] huruf = input.ToCharArray();
        System.Random rng = new System.Random();
        int n = huruf.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (huruf[k], huruf[n]) = (huruf[n], huruf[k]);
        }
        return new string(huruf);
    }

    private string AcakKata(string input)
    {
        List<string> kataList = new List<string>(input.Split(' '));
        System.Random rng = new System.Random();
        int n = kataList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (kataList[k], kataList[n]) = (kataList[n], kataList[k]);
        }
        return string.Join(" ", kataList);
    }

    public string GetJawabanBenar(Soal soal)
    {
        return soal.jawaban;
    }

    public List<Soal> GetSemuaSoal()
    {
        return daftarSoal;
    }
}

[System.Serializable]
public class Soal
{
    public int id_soal;
    public string jenis_soal;
    public string pertanyaan;
    public string jawaban;
    public string pengelabuan;
    public string gambar;
    public string level;
    public string id_stage;  // Pastikan ini ditambahkan
}

[System.Serializable]
public class SoalList
{
    public Soal[] soal;
}
