using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMateriManager : MonoBehaviour
{
    public void PilihReading()
    {
        PlayerPrefs.SetString("pilihan_materi", "Reading");
        PlayerPrefs.Save();
        SceneTransitionManager.Instance.LoadSceneWithFade("ListMateri"); // Ganti ke scene list materi
    }

    public void PilihListening()
    {
        PlayerPrefs.SetString("pilihan_materi", "Listening");
        PlayerPrefs.Save();
        SceneTransitionManager.Instance.LoadSceneWithFade("ListMateri"); // Ganti ke scene list materi
    }
    public void Pilihkembalimainmemu()
    {
       SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu"); // Ganti ke scene list materi
    }
}