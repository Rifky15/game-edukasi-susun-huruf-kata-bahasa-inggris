using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoAdjust : MonoBehaviour
{
    public float targetAspect = 9f / 18f; // 1080x1920 aspect ratio

    void Start()
    {
        Debug.Log("Aspect adjusted: " + Camera.main.rect);
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            Rect rect = cam.rect;

            float scaleWidth = 1.0f / scaleHeight;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}
