using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowLayoutManager : MonoBehaviour
{
    public RectTransform panelContainer;
    public GameObject rowPrefab;
    public GameObject itemPrefab;
    public float maxRowWidth = 900f;

    private List<RectTransform> rows = new List<RectTransform>();

    public void ClearLayout()
    {
        foreach (Transform child in panelContainer)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();
    }

    public void PopulateItems(string[] kataList, bool isSlot = false, System.Action<Button> onClick = null)
    {
        ClearLayout();

        RectTransform currentRow = AddNewRow();
        float currentWidth = 0f;
        float spacing = 10f;

        foreach (string kata in kataList)
        {
            GameObject newItem = Instantiate(itemPrefab);
            TMP_Text text = newItem.GetComponentInChildren<TMP_Text>();

            // ✅ Placeholder untuk slot jawaban
            if (isSlot)
            {
                text.text = "___"; // placeholder
                text.color = Color.gray; // opsional: warna placeholder
                // text.fontStyle = FontStyles.Italic; // opsional: gaya italic
            }
            else
            {
                text.text = kata;
                text.color = Color.white;
                // text.fontStyle = FontStyles.Normal;
            }

            text.ForceMeshUpdate();

            float estimatedWidth = text.preferredWidth + 30f;
            float estimatedHeight = text.preferredHeight + 20f;

            if (currentWidth + estimatedWidth > maxRowWidth)
            {
                currentRow = AddNewRow();
                currentWidth = 0f;
            }

            newItem.transform.SetParent(currentRow, false);
            currentWidth += estimatedWidth + spacing;

            // ✅ Atur ukuran tombol berdasarkan teks
            LayoutElement le = newItem.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.preferredWidth = Mathf.Clamp(estimatedWidth, 125f, 350f);
                le.preferredHeight = Mathf.Clamp(estimatedHeight, 100f, 150f);
            }

            // 🔗 Listener
            Button b = newItem.GetComponent<Button>();
            if (onClick != null)
                b.onClick.AddListener(() => onClick.Invoke(b));

            // 🧠 Simpan ke daftar global GamePlay
            if (isSlot)
                GamePlay.Instance.slotJawabanButtons.Add(b);
            else
                GamePlay.Instance.inputJawabanButtons.Add(b);
        }
    }


    private RectTransform AddNewRow()
    {
        GameObject newRow = Instantiate(rowPrefab, panelContainer);
        RectTransform rowRect = newRow.GetComponent<RectTransform>();
        rows.Add(rowRect);
        return rowRect;
    }
}
