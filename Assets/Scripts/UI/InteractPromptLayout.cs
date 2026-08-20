using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FokusTour.UI
{
    /// <summary>
    /// Styles the "Lihat Karya" prompt button for landscape mobile.
    /// Place this on InteractPrompt.
    /// </summary>
    public class InteractPromptLayout : MonoBehaviour
    {
        [SerializeField] private Button interactButton;
        [SerializeField] private Vector2 buttonSize = new Vector2(280f, 72f);
        [SerializeField] private float bottomOffset = 110f;
        [SerializeField] private Color buttonColor = new Color(0.09f, 0.10f, 0.12f, 0.94f);
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private float fontSize = 28f;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            if (interactButton == null)
                interactButton = GetComponentInChildren<Button>(true);

            StretchOverlay(transform as RectTransform);
            StyleButton(interactButton);
        }

        private static void StretchOverlay(RectTransform overlay)
        {
            if (overlay == null)
                return;

            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.pivot = new Vector2(0.5f, 0.5f);
            overlay.anchoredPosition = Vector2.zero;
            overlay.sizeDelta = Vector2.zero;
            overlay.localScale = Vector3.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;
        }

        private void StyleButton(Button button)
        {
            if (button == null)
                return;

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomOffset);
            rect.sizeDelta = buttonSize;
            rect.localScale = Vector3.one;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = buttonColor;
                image.raycastTarget = true;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
                return;

            StretchOverlay(label.rectTransform);
            label.text = "Lihat Karya";
            label.color = labelColor;
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = false;
            label.raycastTarget = false;
        }
    }
}
