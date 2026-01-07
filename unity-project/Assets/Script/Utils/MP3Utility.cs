using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class MP3Utility
{
    public static IEnumerator LoadMP3FromFile(string filePath, System.Action<AudioClip> onAudioLoaded)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Path file MP3 kosong!");
            yield break;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogError("File MP3 tidak ditemukan: " + filePath);
            yield break;
        }

        string fileUrl = "file://" + filePath;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                onAudioLoaded?.Invoke(clip);
            }
            else
            {
                Debug.LogError("Gagal memuat MP3: " + request.error);
            }
        }
    }
}