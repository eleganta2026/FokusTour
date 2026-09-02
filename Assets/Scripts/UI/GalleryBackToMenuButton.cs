using FokusTour.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FokusTour.UI
{
    /// <summary>
    /// Runtime exit icon button in the gallery that returns to MainMenuScene.
    /// Place on Canvas or any active object in MainScene.
    /// </summary>
    public class GalleryBackToMenuButton : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private ArtworkInfoUI artworkInfoUI;
        [SerializeField] private Vector2 buttonSize = new Vector2(80f, 80f);
        [SerializeField] private Vector2 topRightOffset = new Vector2(-72f, -32f);
        [SerializeField] private Color buttonColor = new Color(0.09f, 0.10f, 0.12f, 0.92f);
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private float iconPadding = 14f;

        private GameObject _buttonRoot;
        private Sprite _roundedSprite;
        private Sprite _exitIconSprite;

        private void Awake()
        {
            if (artworkInfoUI == null)
                artworkInfoUI = FindFirstObjectByType<ArtworkInfoUI>();

            _roundedSprite = CreateRoundedSprite(128, 32f);
            _exitIconSprite = CreateExitIconSprite(128);
            BuildButton();
        }

        private void Update()
        {
            if (_buttonRoot == null)
                return;

            bool hideForInfo = artworkInfoUI != null && artworkInfoUI.IsOpen;
            if (_buttonRoot.activeSelf == hideForInfo)
                _buttonRoot.SetActive(!hideForInfo);
        }

        private void OnDestroy()
        {
            DestroyGeneratedSprite(ref _roundedSprite);
            DestroyGeneratedSprite(ref _exitIconSprite);
        }

        private void BuildButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogWarning("GalleryBackToMenuButton: Canvas tidak ditemukan.");
                return;
            }

            _buttonRoot = new GameObject("BackToMenuButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            _buttonRoot.transform.SetParent(canvas.transform, false);
            _buttonRoot.transform.SetAsLastSibling();

            RectTransform rect = _buttonRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = topRightOffset;
            rect.sizeDelta = buttonSize;

            Image background = _buttonRoot.GetComponent<Image>();
            if (_roundedSprite != null)
            {
                background.sprite = _roundedSprite;
                background.type = Image.Type.Sliced;
                background.pixelsPerUnitMultiplier = 1.2f;
            }
            background.color = buttonColor;

            Button button = _buttonRoot.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(GoToMainMenu);

            GameObject iconObject = new GameObject("ExitIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(_buttonRoot.transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(iconPadding, iconPadding);
            iconRect.offsetMax = new Vector2(-iconPadding, -iconPadding);

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = _exitIconSprite;
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.color = iconColor;
            icon.raycastTarget = false;
        }

        private void GoToMainMenu()
        {
            if (ArtworkSessionCache.Instance != null)
            {
                ArtworkSessionCache.Instance.Clear();
                Destroy(ArtworkSessionCache.Instance.gameObject);
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private static Sprite CreateExitIconSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "ExitIconTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            float stroke = size * 0.08f;

            // Door/frame on the left.
            DrawRect(pixels, size, size * 0.18f, size * 0.20f, size * 0.18f, size * 0.60f, stroke);
            // Open side (right of door) stays empty so arrow can leave.

            // Arrow shaft.
            DrawLine(pixels, size, size * 0.42f, size * 0.50f, size * 0.78f, size * 0.50f, stroke);
            // Arrow head.
            DrawLine(pixels, size, size * 0.62f, size * 0.34f, size * 0.80f, size * 0.50f, stroke);
            DrawLine(pixels, size, size * 0.62f, size * 0.66f, size * 0.80f, size * 0.50f, stroke);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static void DrawRect(Color32[] pixels, int size, float x, float y, float width, float height, float stroke)
        {
            DrawLine(pixels, size, x, y, x + width, y, stroke);
            DrawLine(pixels, size, x, y + height, x + width, y + height, stroke);
            DrawLine(pixels, size, x, y, x, y + height, stroke);
            DrawLine(pixels, size, x + width, y, x + width, y + height, stroke);
        }

        private static void DrawLine(Color32[] pixels, int size, float x0, float y0, float x1, float y1, float thickness)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float length = Mathf.Max(1f, Mathf.Sqrt(dx * dx + dy * dy));
            int steps = Mathf.CeilToInt(length * 2f);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float x = Mathf.Lerp(x0, x1, t);
                float y = Mathf.Lerp(y0, y1, t);
                StampCircle(pixels, size, x, y, thickness * 0.5f);
            }
        }

        private static void StampCircle(Color32[] pixels, int size, float cx, float cy, float radius)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - radius - 1f));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + radius + 1f));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - radius - 1f));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + radius + 1f));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((radius - dist) / 1.2f);
                    if (alpha <= 0f)
                        continue;

                    int index = y * size + x;
                    byte value = (byte)(alpha * 255f);
                    if (value > pixels[index].a)
                        pixels[index] = new Color32(255, 255, 255, value);
                }
            }
        }

        private static Sprite CreateRoundedSprite(int size, float cornerRadiusPixels)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "GalleryMenuButtonTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            float radius = Mathf.Clamp(cornerRadiusPixels, 4f, size * 0.45f);
            float softEdge = 1.25f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = RoundedRectAlpha(x, y, size, size, radius, softEdge);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            int border = Mathf.CeilToInt(radius + softEdge + 1f);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
        }

        private static float RoundedRectAlpha(int x, int y, int width, int height, float radius, float softEdge)
        {
            float left = radius;
            float right = width - 1 - radius;
            float bottom = radius;
            float top = height - 1 - radius;

            float px = x;
            float py = y;
            float dx = 0f;
            float dy = 0f;

            if (px < left)
                dx = left - px;
            else if (px > right)
                dx = px - right;

            if (py < bottom)
                dy = bottom - py;
            else if (py > top)
                dy = py - top;

            if (dx <= 0f && dy <= 0f)
                return 1f;

            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01((radius - dist) / softEdge);
        }

        private static void DestroyGeneratedSprite(ref Sprite sprite)
        {
            if (sprite == null)
                return;

            Texture2D texture = sprite.texture;
            Destroy(sprite);
            if (texture != null)
                Destroy(texture);
            sprite = null;
        }
    }
}
