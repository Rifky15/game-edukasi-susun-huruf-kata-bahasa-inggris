using UnityEngine;
using System.Collections.Generic;

public class ListeningQuestManager : MonoBehaviour
{
    public static ListeningQuestManager Instance { get; private set; }

    private HashSet<string> audioSudahDihitung = new HashSet<string>(); // Menyimpan ID materi yang sudah dihitung
    private const int audioSelesaiUntukTambahBuku = 5; // Dapat 1 buku setiap 5 audio unik selesai

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
    }

    private void Start()
    {
        audioSudahDihitung.Clear();
    }

    // Fungsi ini dipanggil saat audio selesai diputar, parameter adalah ID materi/audio
    public string AudioSelesaiDiputar(string audioId)
    {
        if (audioSudahDihitung.Contains(audioId))
        {
            // Sudah dihitung sebelumnya, tidak tambah progress
            return "";
        }

        // Tambahkan ke HashSet karena ini pertama kali diputar
        audioSudahDihitung.Add(audioId);

        if (audioSudahDihitung.Count % audioSelesaiUntukTambahBuku == 0)
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.GetJumlahBuku() < 5)
                {
                    GameManager.Instance.TambahBuku(1);
                    return "Selamat! Kamu mendapat 1 buku!";
                }
                else
                {
                    return "Audio selesai! Tapi buku sudah penuh.";
                }
            }
            else
            {
                Debug.LogWarning("GameManager tidak ditemukan!");
                return "";
            }
        }

        return ""; // belum waktunya reward
    }
    
    public void ResetProgress()
    {
        audioSudahDihitung.Clear();  // Reset progress dengan mengosongkan hashset
    }

    public int GetJumlahAudioSelesai()
    {
        return audioSudahDihitung.Count;
    }
}
