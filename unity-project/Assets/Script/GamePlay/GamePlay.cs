using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class GamePlay : MonoBehaviour
{
    //soal
    private int indeksSoalSaatIni = 0;
    private List<Soal> soalList = new List<Soal>();
    private Soal soalSekarang;
    private Soal soalAktif;

    [Header("UI Soal dan Jawaban")]
    public TextMeshProUGUI textIndikatorSoal; // Teks: "Soal ke X dari Y"
    public TextMeshProUGUI textPertanyaan;
    public RawImage soalImage;
    
    [Header("Panel Kosakata")]
    public Transform pilihanKosakataPanel;
    public Transform slotKosakataPanel;

    // [Header("Panel Frasa")]
    // public Transform pilihanFrasaPanel;
    // public Transform slotFrasaPanel;

    [Header("Prefab Kosakata")]
    public GameObject kosakataButtonPrefab;
    public GameObject kosakataSlotPrefab;

    [Header("Prefab Frasa")]
    public GameObject frasaButtonPrefab;
    public GameObject frasaSlotPrefab;
    public FlowLayoutManager pilihanFrasaLayout;
    public FlowLayoutManager inputFrasaLayout;
    public GameObject pilihanFrasaContainerPanel; // Panel yang punya VerticalLayoutGroup
    public GameObject inputFrasaContainerPanel;   // Panel yang punya VerticalLayoutGroup

    [Header("UI Skor & Buku")]
    public TMP_Text textSkorPerStage;
    public TMP_Text textJumlahBuku;
    public TMP_Text notifikasiText;
    public Image notifikasiBackground; // Menambahkan Image sebagai latar belakang
    public TMP_Text tmpBonusNotifikasi;
    public GameObject panelNotif; // Tambahkan referensi ke panel utama

    [Header("UI End Game")]
    public GameObject gameWinPanel;
    public TMP_Text textNotif; // Text pertama "Selamat..."
    public TMP_Text textSkor;  // Text kedua untuk skor
    public Button btnMainMenuWin; // Button untuk kembali ke Main Menu
    public Button btnNextStage; // Button untuk lanjut ke Stage berikutnya

    [Header("UI Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text tmpGameOverMessage;
    public TMP_Text tmpScore;
    public Button btnMainMenuGameOver;
    public Button btnMenuMateriGameOver;
    private bool isGameOverShown = false; // untuk mencegah tampil berkali-kali

    private Dictionary<Button, Color> originalTextColors = new Dictionary<Button, Color>();
    public List<Button> inputJawabanButtons = new List<Button>();
    public List<Button> slotJawabanButtons = new List<Button>();
    private Dictionary<int, string> savedAnswers = new Dictionary<int, string>();

    private int jawabanBenarBeruntun = 0; // Menghitung jawaban benar berturut-turut
    private int jawabanBenarTotal = 0; // Menghitung jumlah jawaban benar dalam satu stage

    [Header("Tombol Navigasi")]
     // Assign di Inspector
    public Button btnSubmit;
    public Button btnMainMenu;
    public GameObject confirmationPanel; // Panel konfirmasi
    public Button btnYes; // Tombol "Ya"
    public Button btnNo; // Tombol "Tidak

    // private int skorPerStage = 0;

    string level = "mudah";
    int idSoal = 1;
    
        public static GamePlay Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);  // To ensure only one instance exists.
            }
        }


    void Start()
    {
        // gameTimer = FindObjectOfType<GameTimer>(); 
        int stage = PlayerPrefs.GetInt("stage", 1);
        int level = PlayerPrefs.GetInt("level", 1);
        Debug.Log($"🔍 Stage saat ini: {stage}, Level: {level}");
        
        GameTimer.Instance.OnTimeUp += WaktuHabis;

        StartCoroutine(MuatSoal());
        
        GameManager.Instance.ResetSkorPerStage();
        
        if (textSkorPerStage != null)
        textSkorPerStage.text = "0";

        

        // btnMainMenu.onClick.AddListener(() => Exit());
        btnMainMenu.onClick.AddListener(() => ShowConfirmationPanel()); // Tampilkan panel konfirmasi saat klik tombol

        // Tambahkan listener untuk tombol "Ya" dan "Tidak"
        btnYes.onClick.AddListener(() => GoToMainMenu());
        btnNo.onClick.AddListener(() => HideConfirmationPanel());
        
    }

    void Update()
    {
        UpdateUIBukuDanSkor(); // Update setiap frame, atau pakai coroutine kalau mau lebih efisien
        if (!isGameOverShown && GameManager.Instance.GetJumlahBuku() <= 0)
        {
            TampilkanPanelGameOver();
        }
    }

    void WaktuHabis()
    {
         Debug.Log("Event waktu habis terpanggil!");
        List<Soal> soalList = StageLevelManager.Instance.GetSoalSaatIni();

        if (soalAktif != null)
        {
            int penalti = StageLevelManager.Instance.GetPenaltyForWrongAnswer(soalAktif.level);
            HandleWaktuHabis(penalti);
        }
        else
        {
            Debug.LogError("Tidak ada soal aktif saat waktu habis.");
        }

    }

    private void HandleWaktuHabis(int penalti)
    {
        GameManager.Instance.KurangiSkor(penalti);
        GameManager.Instance.KurangiBuku();

        UpdateUIBukuDanSkor();
    
        // 🔹 Tampilkan notifikasi waktu habis
        TampilkanNotifikasi("Waktu Habis! Jawaban Dianggap Salah.", Color.yellow, true, JenisSFX.Salah);

        // Tambahkan animasi notifikasi atau efek visual di sini
        Debug.Log("Waktu habis! Efek UI ditampilkan.");

        SoalSelanjutnya();
    }

    public enum JenisSFX
    {
        Benar,
        BenarBonus,
        Salah
    }


    public void TampilkanNotifikasi(string pesan, Color warna, bool tampilkanBackground, JenisSFX jenisSFX)
    {
        panelNotif.SetActive(true);
        notifikasiBackground.gameObject.SetActive(tampilkanBackground);
        notifikasiText.gameObject.SetActive(true);
        notifikasiText.text = pesan;
        notifikasiText.color = warna;

        // Mainkan SFX sesuai jenis
        switch (jenisSFX)
        {
            case JenisSFX.Benar:
                SFXManager.instance.PlayCorrect();
                break;
            case JenisSFX.BenarBonus:
                SFXManager.instance.PlayCorrectBonus();
                break;
            case JenisSFX.Salah:
                SFXManager.instance.PlayWrong();
                break;
        }

        StartCoroutine(FadeOutNotifikasi());
    }

    public void SembunyikanSemuaNotifikasi()
    {
        panelNotif.SetActive(false); // Sembunyikan panel notifikasi jawaban
        tmpBonusNotifikasi.gameObject.SetActive(false); // Sembunyikan panel notifikasi bonus
    }

    IEnumerator FadeOutNotifikasi()
    {
        yield return new WaitForSeconds(3.5f); // Tunggu sebelum mulai fade out

        for (float t = 1; t > 0; t -= Time.deltaTime)
        {
            notifikasiText.color = new Color(
                notifikasiText.color.r, 
                notifikasiText.color.g, 
                notifikasiText.color.b, 
                t // Ubah transparansi (alpha)
            );

            notifikasiBackground.color = new Color(
                notifikasiBackground.color.r, 
                notifikasiBackground.color.g, 
                notifikasiBackground.color.b, 
                t
            );

            yield return null;
        }

        // **Pastikan notifikasi tidak muncul kembali**
        if (!gameOverPanel.activeSelf && !gameWinPanel.activeSelf)
        {
            panelNotif.SetActive(false);
        }
    }

    private void UpdateUIBukuDanSkor()
    {
        if (textSkorPerStage != null)
            textSkorPerStage.text = GameManager.Instance.GetSkorPerStage().ToString();

        if (textJumlahBuku != null)
            textJumlahBuku.text = GameManager.Instance.GetJumlahBuku().ToString();
    }

    private void TampilkanPanelGameOver()
    {
        isGameOverShown = true;
        Time.timeScale = 0f;
        
        StartCoroutine(TampilkanGameOverPanel()); // Tunggu sejenak sebelum tampil

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (tmpGameOverMessage != null)
            tmpGameOverMessage.text = "Buku habis! Kamu gagal menyelesaikan stage.";

        if (tmpScore != null)
            tmpScore.text = "Skor kamu: " + GameManager.Instance.GetSkorPerStage().ToString();

        if (btnMainMenuGameOver != null)
            btnMainMenuGameOver.onClick.AddListener(() =>
            {
                EventSystem eventSystem = FindObjectOfType<EventSystem>();
                if (eventSystem != null)
                {
                    Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
                }
                Time.timeScale = 1f;
                SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
            });

        if (btnMenuMateriGameOver != null)
            btnMenuMateriGameOver.onClick.AddListener(() =>
            {
                 EventSystem eventSystem = FindObjectOfType<EventSystem>();
                if (eventSystem != null)
                {
                    Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
                }
                Time.timeScale = 1f;
                SceneTransitionManager.Instance.LoadSceneWithFade("MenuMateri"); // Ganti sesuai nama scene materi kamu
            });
    }
    
    IEnumerator TampilkanGameOverPanel()
    {
        SembunyikanSemuaNotifikasi();
        yield return new WaitForSeconds(1.5f); // Tunggu 1.5 detik sebelum panel tampil

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            // SFXManager.instance.PlayGameOver();

        if (tmpGameOverMessage != null)
            tmpGameOverMessage.text = "Buku habis! Kamu gagal menyelesaikan stage.";
    }


    private void ShowConfirmationPanel()
    {
        confirmationPanel.SetActive(true); // Tampilkan panel konfirmasi
    }

    // Menyembunyikan panel konfirmasi
    private void HideConfirmationPanel()
    {
        confirmationPanel.SetActive(false); // Sembunyikan panel konfirmasi
    }

    // Fungsi untuk pergi ke Main Menu
    private void GoToMainMenu()
    {
        // Hapus EventSystem (jika perlu) atau bersihkan scene sebelum berpindah
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }

        // Pindah ke scene Main Menu
        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
    }

    private IEnumerator MuatSoal()
    {
        SoalManager.Instance.AmbilSoalDariAPI();
        
        yield return new WaitForSeconds(3f); // Tunggu data diambil

        // Ambil soal hanya untuk stage saat ini
        soalList = new List<Soal>(StageLevelManager.Instance.AmbilSoalStage());
        
        if (soalList.Count > 0)
        {
            TampilkanSoal();
        }
        else
        {
            Debug.LogError("Tidak ada soal untuk stage ini!");
        }
    }

    private void MulaiWaktu(Soal soal){
        float waktuLevel = StageLevelManager.Instance.GetTimeLimitForLevel(soal.level); // Menggunakan level dari soal
        GameTimer.Instance.ResetTimer();  
        GameTimer.Instance.MulaiTimer(waktuLevel);
    }
    
    private void AktifkanKosakataLayout()
    {
        pilihanKosakataPanel.gameObject.SetActive(true);
        slotKosakataPanel.gameObject.SetActive(true);

        // Nonaktifkan layout frasa baru
        pilihanFrasaContainerPanel.SetActive(false);
        inputFrasaContainerPanel.SetActive(false);
    }

    private void AktifkanFrasaLayout()
    {
        pilihanKosakataPanel.gameObject.SetActive(false);
        slotKosakataPanel.gameObject.SetActive(false);

        // Aktifkan layout frasa baru
        pilihanFrasaContainerPanel.SetActive(true);
        inputFrasaContainerPanel.SetActive(true);
    }

    
    private void TampilkanJawaban(Soal soalSekarang)
    {
        string[] elemenInput;
        string[] elemenJawaban;
        string slotJawaban = SoalManager.Instance.GetJawaban(soalSekarang);
        string inputJawaban = SoalManager.Instance.GetJawabanBenar(soalSekarang);

        // Kosongkan list & dictionary
        inputJawabanButtons.Clear();
        slotJawabanButtons.Clear();
        savedAnswers.Clear();

        if (soalSekarang.jenis_soal.ToLower() == "kosakata")
        {
            AktifkanKosakataLayout();

            elemenInput = inputJawaban.ToCharArray().Select(c => c.ToString()).ToArray();
            elemenJawaban = slotJawaban.ToCharArray().Select(c => c.ToString()).ToArray();

            for (int i = 0; i < elemenJawaban.Length; i++)
            {
                GameObject newButton = Instantiate(kosakataButtonPrefab, pilihanKosakataPanel);
                TMP_Text buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = elemenJawaban[i];

                Button buttonComponent = newButton.GetComponent<Button>();
                inputJawabanButtons.Add(buttonComponent);
                buttonComponent.onClick.AddListener(() => SelectAnswer(buttonComponent));
            }

            for (int i = 0; i < elemenInput.Length; i++)
            {
                GameObject newSlot = Instantiate(kosakataSlotPrefab, slotKosakataPanel);
                Button slotButton = newSlot.GetComponent<Button>();
                slotJawabanButtons.Add(slotButton);
                slotButton.onClick.AddListener(() => ReturnAnswer(slotButton));
            }
        }
        else // frasa
        {
            AktifkanFrasaLayout();

            elemenInput = inputJawaban.Split(' ');
            elemenJawaban = slotJawaban.Split(' ');

            pilihanFrasaLayout.PopulateItems(elemenJawaban, false, SelectAnswer);
            inputFrasaLayout.PopulateItems(elemenInput, true, ReturnAnswer);
        }
    }


    private IEnumerator ForceLayoutRebuild(Transform panel)
    {
        yield return null; // Tunggu 1 frame
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
    }


    private void ClearLayout(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator ResetUI()
    {
        // 🔹 Bersihkan semua child di panel jawaban dan slot
        ClearLayout(pilihanKosakataPanel.transform);
        ClearLayout(pilihanFrasaContainerPanel.transform);
        ClearLayout(slotKosakataPanel.transform);
        ClearLayout(inputFrasaContainerPanel.transform);


        // 🔹 Kosongkan list dan dictionary
        inputJawabanButtons.Clear();
        slotJawabanButtons.Clear();
        savedAnswers.Clear();
        originalTextColors.Clear();

        yield return new WaitForSeconds(0.1f); // Delay agar layout refresh

        TampilkanSoal();
    }

    private void TampilkanSoal() 
    {
        if (soalList.Count > 0 && indeksSoalSaatIni < soalList.Count)
        Debug.Log($"📌 Total soal: {soalList.Count}, Soal ke: {indeksSoalSaatIni + 1}");
        {
            soalSekarang = soalList[indeksSoalSaatIni];
            soalAktif = soalSekarang;

            int current = indeksSoalSaatIni + 1; // Karena indeks dimulai dari 0
            int total = soalList.Count;
            textIndikatorSoal.text = $"Soal ke {current}/{total}";

            MulaiWaktu(soalSekarang);
            textPertanyaan.text = soalSekarang.pertanyaan;
            TampilkanJawaban(soalSekarang);
            level = soalSekarang.level;
            idSoal = soalSekarang.id_soal;

            // ✅ Tampilkan gambar jika tersedia
            if (!string.IsNullOrEmpty(soalSekarang.gambar))
            {
                soalImage.gameObject.SetActive(true);

                // Lengkapi URL jika belum ada https
                string imageUrl = soalSekarang.gambar;
                if (!imageUrl.StartsWith("http"))
                {
                    imageUrl = "http://localhost/GameEdukasiDB/" + imageUrl;
                }

                Debug.Log("🔗 Meminta gambar dari URL: " + imageUrl);
                StartCoroutine(LoadImageFromURL(imageUrl));
            }
            else
            {
                soalImage.texture = null;
                soalImage.gameObject.SetActive(false);
            }

            btnSubmit.onClick.RemoveAllListeners();
            btnSubmit.onClick.AddListener(() => CekJawaban(soalSekarang));
        }
    }

    private IEnumerator LoadImageFromURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("❌ URL gambar kosong atau tidak valid");
            soalImage.gameObject.SetActive(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                soalImage.texture = texture;
                // soalImage.SetNativeSize(); // ✅ Pertahankan aspek rasio
            }
            else
            {
                Debug.LogWarning("❌ Gagal memuat gambar soal: " + request.error);
                soalImage.gameObject.SetActive(false);
            }
        }
    }

    private void SelectAnswer(Button selectedButton)
    {
        if (!selectedButton.interactable) return;

        TMP_Text inputText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
        bool isKosakata = soalSekarang.jenis_soal.ToLower() == "kosakata";

        for (int i = 0; i < slotJawabanButtons.Count; i++)
        {
            TMP_Text slotText = slotJawabanButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            
            bool slotKosong = string.IsNullOrEmpty(slotText.text);
            bool slotUnderscore = slotText.text == "___";

            // Kondisi untuk mengisi slot:
            if ((isKosakata && slotKosong) || (!isKosakata && slotUnderscore))
            {
                slotText.text = inputText.text;
                slotText.color = Color.white;
                // slotText.fontStyle = FontStyles.Normal;

                // Simpan jawaban
                if (!savedAnswers.ContainsKey(i))
                    savedAnswers.Add(i, inputText.text);
                else
                    savedAnswers[i] = inputText.text;

                // Simpan warna asli
                if (!originalTextColors.ContainsKey(selectedButton))
                    originalTextColors[selectedButton] = inputText.color;

                selectedButton.interactable = false;
                inputText.color = new Color(0, 0, 0, 0.2f);

                // Resize slot (khusus frasa)
                slotText.ForceMeshUpdate();
                float preferredWidth = slotText.preferredWidth + 30f;
                preferredWidth = Mathf.Clamp(preferredWidth, 60f, 350f);

                LayoutElement le = slotJawabanButtons[i].GetComponent<LayoutElement>();
                if (le != null)
                    le.preferredWidth = preferredWidth;

                LayoutRebuilder.ForceRebuildLayoutImmediate(slotJawabanButtons[i].GetComponent<RectTransform>());

                return;
            }
        }
    }


    private void ReturnAnswer(Button selectedSlot)
    {
        for (int i = 0; i < slotJawabanButtons.Count; i++)
        {
            if (slotJawabanButtons[i] == selectedSlot)
            {
                TMP_Text slotText = slotJawabanButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (slotText.text == "___") return;

                foreach (Button inputButton in inputJawabanButtons)
                {
                    TMP_Text inputText = inputButton.GetComponentInChildren<TextMeshProUGUI>();

                    if (!inputButton.interactable && savedAnswers.ContainsKey(i) && inputText.text == savedAnswers[i])
                    {
                        inputButton.interactable = true;

                        if (originalTextColors.ContainsKey(inputButton))
                            inputText.color = originalTextColors[inputButton];

                        if (soalSekarang.jenis_soal.ToLower() == "kosakata")
                        {
                            slotText.text = "";
                            slotText.color = Color.white;
                        }
                        else
                        {
                            slotText.text = "___";
                            slotText.color = Color.gray;
                            // slotText.fontStyle = FontStyles.Italic;
                        }

                        savedAnswers.Remove(i);

                        // 🔥 Reset ukuran slot ke default
                        LayoutElement le = slotJawabanButtons[i].GetComponent<LayoutElement>();
                        if (le != null)
                            le.preferredWidth = 150f;

                        LayoutRebuilder.ForceRebuildLayoutImmediate(slotJawabanButtons[i].GetComponent<RectTransform>());

                        return;
                    }
                }
            }
        }
    }

    public void SoalSelanjutnya()
    {
        Debug.Log("➡️ Memanggil SoalSelanjutnya()");
        if (indeksSoalSaatIni < soalList.Count - 1)
        {
            indeksSoalSaatIni++;
            StartCoroutine(ResetUI());
        }
        else
        {
            // Tambahkan pengecekan ini
            if (GameManager.Instance.GetJumlahBuku() <= 0)
            {
                TampilkanPanelGameOver(); // jika nyawa habis, tampilkan Game Over
            }
            else
            {
                GameSelesai(); // hanya jika nyawa masih ada
            }
        }
    }
    
    IEnumerator TampilkanGameWinPanel()
    {
        SembunyikanSemuaNotifikasi();
        yield return new WaitForSeconds(1.5f); // Tunggu 1.5 detik agar notifikasi terlihat dulu
        
        if (gameWinPanel != null)
            gameWinPanel.SetActive(true); // Tampilkan panel GameWin
    //         SFXManager.instance.PlayGameWin();
    }

    private void GameSelesai()
    {
        Time.timeScale = 0f; // Pause game

        StartCoroutine(TampilkanGameWinPanel());

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true); // Tampilkan panel GameWin

            if (textSkorPerStage != null)
                textSkorPerStage.text = "Skor stage: " + GameManager.Instance.GetSkorPerStage().ToString();

            if (textNotif != null)
                textNotif.text = "Selamat kamu menyelesaikan Stage ini!";
            
            if (textSkor != null)
            {
                int skorPerStage = GameManager.Instance.GetSkorPerStage();
                textSkor.text = "Skor kamu: " + skorPerStage.ToString();
                
                int level = PlayerPrefs.GetInt("level", 1);
                int stage = PlayerPrefs.GetInt("stage", 1);
                int highestStage = PlayerPrefs.GetInt($"highest_stage_level_{level}", 1);

                // **Pastikan highest_stage diperbarui hanya jika pemain menyelesaikan stage tertinggi**
                if (stage >= highestStage)
                {
                    int nextStage = stage + 1;
                    PlayerPrefs.SetInt($"highest_stage_level_{level}", nextStage);
                    Debug.Log($"[GameSelesai] Highest stage diperbarui ke {nextStage} karena pemain berhasil menyelesaikan ulang stage tertinggi.");
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.Log("Pemain hanya mengulang stage lama, highest_stage tidak diperbarui.");
                }
                // ✅ Letakkan di akhir, agar selalu dipanggil
                StageLevelManager.Instance.CompleteStage();


                // 🔹 Debug log untuk verifikasi sebelum pengiriman data
                Debug.Log($"Data sebelum dikirim ke database: Level = {level}, Stage = {stage}, Skor = {skorPerStage}");
                
                // 🔹 Ambil skor tertinggi dari penyimpanan lokal
                int highestScore = PlayerPrefs.GetInt($"highest_score_level_{level}_stage_{stage}", 0);
                Debug.Log($"[DEBUG] Skor sebelum update: {highestScore}, Skor baru: {skorPerStage}");

                if (skorPerStage > highestScore)
                {
                    // 🔹 Perbarui skor tertinggi di PlayerPrefs
                    PlayerPrefs.SetInt($"highest_score_level_{level}_stage_{stage}", skorPerStage);
                    PlayerPrefs.Save();

                    // 🔹 Kirim data ke database leaderboard
                    LeaderboardSender leaderboardSender = FindObjectOfType<LeaderboardSender>();
                    if (leaderboardSender != null)
                    {
                        leaderboardSender.SendScore(level, stage, skorPerStage);
                        Debug.Log($"Skor baru lebih tinggi! Data terkirim ke database: Level {level}, Stage {stage}, Skor {skorPerStage}");
                    }
                    else
                    {
                        Debug.LogError("LeaderboardSender tidak ditemukan! Pastikan ada di scene.");
                    }
                }
                else
                {
                    Debug.Log($"Skor {skorPerStage} tidak lebih tinggi dari skor sebelumnya {highestScore}. Tidak mengupdate leaderboard.");
                }

            }

            if (btnMainMenuWin != null)
                btnMainMenuWin.onClick.AddListener(GoToMainMenuWin);
            
            if (btnNextStage != null)
            {
                int currentStage = PlayerPrefs.GetInt("stage", 1);
                int highestStage = PlayerPrefs.GetInt($"highest_stage_level_{PlayerPrefs.GetInt("level", 1)}", 1);

                if (currentStage >= 5) // Stage terakhir
                {
                    btnNextStage.gameObject.SetActive(false);
                }
                else
                {
                    btnNextStage.onClick.RemoveAllListeners();
                    btnNextStage.onClick.AddListener(BukaStageSelanjutnya);
                }
            }
        }
    }
    
    void GoToStageSelection()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("Stage"); // Pastikan scene ini ada!
    }

    private void GoToMainMenuWin()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Destroy(eventSystem.gameObject); // Hapus EventSystem di scene sekarang
        }

        Time.timeScale = 1f; // Balikin waktu normal

        // **Tambahkan penyimpanan highest_stage sebelum berpindah ke MainMenu**
        int level = PlayerPrefs.GetInt("level", 1);
        int currentStage = PlayerPrefs.GetInt("stage", 1);
        int highestStage = PlayerPrefs.GetInt($"highest_stage_level_{level}", 1);

        if (currentStage > highestStage) // **Pastikan hanya menyimpan jika mencapai stage baru**
        {
            PlayerPrefs.SetInt($"highest_stage_level_{level}", currentStage);
            PlayerPrefs.Save(); // **Pastikan perubahan tersimpan dengan benar**
            Debug.Log($"Highest stage diperbarui: Level {level}, Stage {currentStage}");
        }
        else
        {
            Debug.Log("Tidak menyimpan highest_stage karena pemain hanya mengulang stage.");
        }

        Debug.Log($"Verifikasi Penyimpanan Sebelum Menu: Highest Stage = {PlayerPrefs.GetInt($"highest_stage_level_{level}", 1)}");

        SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu"); // Pastikan scene MainMenu ada
    }

    private void BukaStageSelanjutnya()
    {
        Time.timeScale = 1f; // Balikin waktu normal

        int currentStage = PlayerPrefs.GetInt("stage", 1);

        if (currentStage >= 5) // Jika sudah mencapai stage terakhir, hentikan proses
        {
            Debug.Log("Sudah di stage terakhir.");
            return;
        }

        int nextStage = currentStage + 1;

        // **Tambahkan penyimpanan highest_stage**
        int level = PlayerPrefs.GetInt("level", 1);
        int highestStage = PlayerPrefs.GetInt($"highest_stage_level_{level}", 1);

        if (nextStage > highestStage)
        {
            PlayerPrefs.SetInt($"highest_stage_level_{level}", nextStage);
            Debug.Log($"Highest stage diperbarui: Level {level}, Stage {nextStage}");
        }

        PlayerPrefs.SetInt("stage", nextStage);
        PlayerPrefs.Save(); // **Pastikan perubahan tersimpan dengan benar!**

        // // Menghapus EventSystem jika ada di scene saat ini
        // EventSystem eventSystem = FindObjectOfType<EventSystem>();
        // if (eventSystem != null)
        // {
        //     Destroy(eventSystem.gameObject); // Hapus EventSystem jika ada di scene saat ini
        // }
        // else
        // {
        //     Debug.LogWarning("EventSystem tidak ditemukan, lanjutkan tanpa menghapus.");
        // }

        StartCoroutine(LoadNextStage());

    }
    
    IEnumerator LoadNextStage()
    {
        yield return new WaitForSeconds(0.5f); // Tunggu setengah detik
        SceneTransitionManager.Instance.LoadSceneWithFade("GamePlay");
    }

    private string GabungkanJawaban(Dictionary<int, string> savedAnswers, string jenisSoal)
    {
        // Urutkan berdasarkan key (index)
        var orderedValues = savedAnswers.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value);

        if (jenisSoal.ToLower() == "kosakata")
        {
            return string.Concat(orderedValues); // hasil: "APPLE"
        }
        else // frasa
        {
            return string.Join(" ", orderedValues); // hasil: "GOOD MORNING"
        }
    }

    // digunakann saat sebenarnya
    public void CekJawaban(Soal soalSaatIni)
    {
        string jawabanPenggunaGabung = GabungkanJawaban(savedAnswers, soalSaatIni.jenis_soal);
        string jawabanBenar = soalSaatIni.jawaban;

        if (jawabanPenggunaGabung.Equals(jawabanBenar, System.StringComparison.OrdinalIgnoreCase))
        {
            int poin = StageLevelManager.Instance.GetPointsForCorrectAnswer(soalSaatIni.level);
            GameManager.Instance.TambahSkorPerStage(poin);
            GameManager.Instance.TambahSkor(poin);

            jawabanBenarBeruntun++;
            jawabanBenarTotal++;

            // 🔹 Terapkan aturan: Setiap 3 jawaban benar → Bonus +20 poin
            if (jawabanBenarBeruntun % 3 == 0)
            {
                GameManager.Instance.TambahSkorPerStage(20);
                GameManager.Instance.TambahSkor(20);
                Debug.Log($"Bonus skor +20! Skor per stage: {GameManager.Instance.GetSkorStage()}, Total skor: {GameManager.Instance.GetSkorTotal()}");
                
                // **Gunakan TMP_Text untuk notifikasi khusus jawaban beruntun**
                tmpBonusNotifikasi.text = "Bonus! 3 Jawaban Benar Berturut-turut! +20 Poin";
                tmpBonusNotifikasi.gameObject.SetActive(true);

                // **Sembunyikan notifikasi setelah beberapa detik**
                StartCoroutine(HilangkanBonusNotifikasi());

            }

            // 🔹 Terapkan aturan: Semua soal benar dalam satu stage → Bonus +30 poin
            int totalSoalDalamStage = 6; // Jumlah soal per stage
            if (jawabanBenarTotal >= totalSoalDalamStage)
            {
                GameManager.Instance.TambahSkorPerStage(30);
                GameManager.Instance.TambahSkor(30);
                Debug.Log($"Bonus skor +30! Semua soal benar di stage ini! Skor per stage: {GameManager.Instance.GetSkorStage()}, Total skor: {GameManager.Instance.GetSkorTotal()}");

                jawabanBenarBeruntun = 0; // Reset penghitung berturut-turut setelah bonus diberikan
            }

            // 🔹 Tampilkan notifikasi jawaban benar
            TampilkanNotifikasi("Jawaban Benar!", Color.green, true, JenisSFX.Benar);
        }
        else
        {
            int penalti = StageLevelManager.Instance.GetPenaltyForWrongAnswer(soalSaatIni.level);
            GameManager.Instance.KurangiSkor(penalti);
            GameManager.Instance.KurangiBuku();

            jawabanBenarBeruntun = 0; // Reset penghitung jawaban benar berturut-turut jika salah

            // 🔹 Tampilkan notifikasi jawaban salah
            TampilkanNotifikasi("Jawaban Salah!", Color.red, true, JenisSFX.Salah);
        }

        SoalSelanjutnya();
        SimpanJawaban();
    }

    IEnumerator HilangkanBonusNotifikasi()
    {
        yield return new WaitForSeconds(4f);
        tmpBonusNotifikasi.gameObject.SetActive(false);
    }

    // testing untuk case jawaban benar dan jawaban salah
    public void JawabanBenar(string level){
        int poin = StageLevelManager.Instance.GetPointsForCorrectAnswer(level);
        // TambahSkor(poin); → ganti dengan:
        GameManager.Instance.TambahSkorPerStage(poin);
        GameManager.Instance.TambahSkor(poin); // jika skorTotal juga ingin ditambah

        SoalSelanjutnya();
        SimpanJawaban();
    }
    private void SimpanJawaban(){
        string dictString = string.Join("-", savedAnswers.Values);
        Debug.Log(dictString);
        savedAnswers.Clear();
        JawabanManager.Instance.SimpanJawaban(idSoal,dictString);
    }
    public void JawabanSalah(string level){
        int penalti = StageLevelManager.Instance.GetPenaltyForWrongAnswer(level);
        GameManager.Instance.KurangiSkor(penalti);
        GameManager.Instance.KurangiBuku();
    }
}