using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class ScrollPassThrough : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Memastikan ScrollView tetap menerima input saat item diklik
        eventData.pointerDrag = GetComponentInParent<ScrollRect>().gameObject;
    }
}