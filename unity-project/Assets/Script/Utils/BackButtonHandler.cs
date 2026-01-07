using UnityEngine;
using UnityEngine.UI;

public class BackButtonHandler : MonoBehaviour
{
    public Button backUIButton; // Drag tombol back UI kamu ke sini dari Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (backUIButton != null)
            {
                backUIButton.onClick.Invoke(); // Jalankan fungsi yang sudah dipasang di tombol UI
            }
            else
            {
                Debug.LogWarning("Back UI Button belum di-assign di Inspector.");
            }
        }
    }
}
