using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FokusTour.UI
{
    /// <summary>
    /// Styles the artwork info panel as a readable landscape card.
    /// Must sit on ArtworkUI together with ArtworkInfoUI.
    /// </summary>
    [RequireComponent(typeof(ArtworkInfoUI))]
    public class ArtworkInfoPanelLayout : MonoBehaviour
    {
        [SerializeField] private ArtworkInfoUI artworkInfoUI;

        [Header("Card")]
        [SerializeField] private Vector2 cardSize = new Vector2(1200f, 640f);
        [SerializeField] private Color dimmerColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color panelColor = new Color(0.09f, 0.10f, 0.12f, 0.96f);
        [SerializeField] private Color frameColor = new Color(1f, 1f, 1f, 0.14f);
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color creatorColor = new Color(0.72f, 0.75f, 0.80f, 1f);
        [SerializeField] private Color descriptionColor = new Color(0.90f, 0.91f, 0.93f, 1f);
        [SerializeField] private Color closeButtonColor = new Color(0.95f, 0.95f, 0.96f, 1f);
        [SerializeField] private Color closeButtonTextColor = new Color(0.13f, 0.14f, 0.16f, 1f);

        private GameObject _dimmer;

        private void Awake()
        {
            if (artworkInfoUI == null)
                artworkInfoUI = GetComponent<ArtworkInfoUI>();

            Apply();
            SetDimmerVisible(false);
        }

        public void Apply()
        {
            if (artworkInfoUI == null)
                artworkInfoUI = GetComponent<ArtworkInfoUI>();

            if (artworkInfoUI == null)
                return;

            StretchOverlay(transform as RectTransform);
            EnsureDimmer();
            SetDimmerVisible(true);
            StylePanel(artworkInfoUI.PanelRoot);
            StylePreview(artworkInfoUI.PreviewImage);
            StyleTitle(artworkInfoUI.TitleText);
            StyleCreator(artworkInfoUI.CreatorText);
            StyleDivider(artworkInfoUI.PanelRoot);
            StyleDescription(artworkInfoUI.DescriptionText);
            StyleCloseButton(artworkInfoUI.CloseButton);
        }

        public void SetDimmerVisible(bool visible)
        {
            if (_dimmer != null)
                _dimmer.SetActive(visible);
        }

        private void EnsureDimmer()
        {
            Transform existing = transform.Find("ArtworkDimmer");
            if (existing == null)
            {
                _dimmer = new GameObject("ArtworkDimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                _dimmer.transform.SetParent(transform, false);
            }
            else
            {
                _dimmer = existing.gameObject;
            }

            _dimmer.transform.SetAsFirstSibling();
            StretchOverlay(_dimmer.transform as RectTransform);

            Image image = _dimmer.GetComponent<Image>();
            image.color = dimmerColor;
            image.raycastTarget = true;
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

        private void StylePanel(GameObject panelRoot)
        {
            if (panelRoot == null)
                return;

            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = cardSize;
                rect.localScale = Vector3.one;
            }

            Image image = panelRoot.GetComponent<Image>();
            if (image != null)
            {
                image.color = panelColor;
                image.raycastTarget = true;
            }
        }

        private void StylePreview(RawImage previewImage)
        {
            if (previewImage == null)
                return;

            Vector2 box = new Vector2(520f, 500f);
            Vector2 size = box;

            if (previewImage.texture != null)
            {
                float aspect = previewImage.texture.width / (float)previewImage.texture.height;
                float boxAspect = box.x / box.y;
                if (aspect >= boxAspect)
                    size = new Vector2(box.x, box.x / aspect);
                else
                    size = new Vector2(box.y * aspect, box.y);
            }

            float x = 48f + (box.x - size.x) * 0.5f;
            float y = -48f - (box.y - size.y) * 0.5f;

            Image frame = EnsureChildImage(previewImage.transform.parent, "PreviewFrame");
            SetTopLeft(frame.rectTransform, x - 8f, y + 8f, size.x + 16f, size.y + 16f);
            frame.color = frameColor;
            frame.raycastTarget = false;
            frame.transform.SetSiblingIndex(0);
            previewImage.transform.SetSiblingIndex(1);

            SetTopLeft(previewImage.rectTransform, x, y, size.x, size.y);
            previewImage.color = Color.white;
            previewImage.raycastTarget = false;
        }

        private void StyleTitle(TextMeshProUGUI titleText)
        {
            if (titleText == null)
                return;

            SetTopLeft(titleText.rectTransform, 604f, -48f, 548f, 80f);
            ApplyTextStyle(titleText, titleColor, 42f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 32f;
            titleText.fontSizeMax = 42f;
        }

        private void StyleCreator(TextMeshProUGUI creatorText)
        {
            if (creatorText == null)
                return;

            SetTopLeft(creatorText.rectTransform, 604f, -132f, 548f, 36f);
            ApplyTextStyle(creatorText, creatorColor, 26f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        }

        private void StyleDivider(GameObject panelRoot)
        {
            if (panelRoot == null)
                return;

            Image divider = EnsureChildImage(panelRoot.transform, "ContentDivider");
            SetTopLeft(divider.rectTransform, 604f, -176f, 548f, 2f);
            divider.color = new Color(1f, 1f, 1f, 0.16f);
            divider.raycastTarget = false;
        }

        private void StyleDescription(TextMeshProUGUI descriptionText)
        {
            if (descriptionText == null)
                return;

            SetTopLeft(descriptionText.rectTransform, 604f, -196f, 548f, 328f);
            ApplyTextStyle(descriptionText, descriptionColor, 26f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            descriptionText.lineSpacing = 12f;
        }

        private void StyleCloseButton(Button closeButton)
        {
            if (closeButton == null)
                return;

            RectTransform rect = closeButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-40f, 32f);
            rect.sizeDelta = new Vector2(168f, 52f);
            rect.localScale = Vector3.one;

            Image image = closeButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = closeButtonColor;
                image.raycastTarget = true;
            }

            TextMeshProUGUI label = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                StretchOverlay(label.rectTransform);
                label.color = closeButtonTextColor;
                label.fontSize = 26f;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.raycastTarget = false;
            }
        }

        private static Image EnsureChildImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
            }

            return go.GetComponent<Image>();
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
        }

        private static void ApplyTextStyle(
            TextMeshProUGUI text,
            Color color,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment)
        {
            text.color = color;
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
            text.fontStyle = style;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;
            text.margin = Vector4.zero;
        }
    }
}
