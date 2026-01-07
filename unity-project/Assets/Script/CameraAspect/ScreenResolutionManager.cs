using UnityEngine;

public class ScreenResolutionManager : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        DetectResolution();
    }

    void DetectResolution()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        
        Debug.Log($"Resolusi layar: {screenWidth}x{screenHeight}");
        
        if (screenHeight >= 1920 && screenHeight <= 2400)
        {
            AdjustCameraAspect(screenWidth, screenHeight);
            AdjustUI(screenWidth, screenHeight);
        }
        else
        {
            Debug.LogWarning("Resolusi tidak didukung, UI mungkin tidak optimal.");
        }
    }

    void AdjustCameraAspect(int width, int height)
    {
        float targetAspect = 1080f / 2160f; // Rasio default
        float windowAspect = (float)width / height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = mainCamera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            mainCamera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = mainCamera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            mainCamera.rect = rect;
        }
    }

    void AdjustUI(int width, int height)
    {
        float aspectRatio = (float)width / height;
        Debug.Log($"Aspect Ratio: {aspectRatio}");

        if (aspectRatio > 0.5f)
        {
            Debug.Log("Menyesuaikan UI untuk aspect ratio tinggi.");
        }
        else
        {
            Debug.Log("Menyesuaikan UI untuk aspect ratio lebih rendah.");
        }
    }
}