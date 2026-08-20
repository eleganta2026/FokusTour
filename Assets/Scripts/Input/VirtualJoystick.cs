using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FokusTour.Input
{
    /// <summary>
    /// Virtual joystick for mobile movement.
    /// Generates simple circular sprites at runtime so no external art is required.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 75f;

        [Header("Look")]
        [SerializeField] private bool generateVisuals = true;
        [SerializeField] private Color backgroundColor = new Color(1f, 1f, 1f, 0.28f);
        [SerializeField] private Color handleColor = new Color(1f, 1f, 1f, 0.75f);
        [SerializeField] private Color handleActiveColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private int spriteSize = 128;
        [SerializeField] [Range(0.05f, 0.45f)] private float backgroundRingThickness = 0.18f;

        private Vector2 _input;
        private Canvas _canvas;
        private Camera _uiCamera;
        private Image _backgroundImage;
        private Image _handleImage;
        private Sprite _backgroundSprite;
        private Sprite _handleSprite;

        public Vector2 Value => _input;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _uiCamera = _canvas.worldCamera;

            CacheImages();

            if (generateVisuals)
                ApplyGeneratedVisuals();

            if (handle != null)
                handle.anchoredPosition = Vector2.zero;

            if (handleRange <= 0f && background != null)
                handleRange = Mathf.Min(background.sizeDelta.x, background.sizeDelta.y) * 0.45f;
        }

        private void OnDestroy()
        {
            DestroyGeneratedSprite(ref _backgroundSprite);
            DestroyGeneratedSprite(ref _handleSprite);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_handleImage != null)
                _handleImage.color = handleActiveColor;

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                _uiCamera,
                out Vector2 localPoint);

            Vector2 radius = background.sizeDelta * 0.5f;
            if (radius.x < 0.01f || radius.y < 0.01f)
                return;

            _input = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);
            _input = Vector2.ClampMagnitude(_input, 1f);

            if (handle != null)
                handle.anchoredPosition = _input * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;

            if (handle != null)
                handle.anchoredPosition = Vector2.zero;

            if (_handleImage != null)
                _handleImage.color = handleColor;
        }

        private void CacheImages()
        {
            if (background != null)
                _backgroundImage = background.GetComponent<Image>();

            if (handle != null)
                _handleImage = handle.GetComponent<Image>();
        }

        private void ApplyGeneratedVisuals()
        {
            if (_backgroundImage != null)
            {
                DestroyGeneratedSprite(ref _backgroundSprite);
                _backgroundSprite = CreateRingSprite(spriteSize, backgroundRingThickness);
                _backgroundImage.sprite = _backgroundSprite;
                _backgroundImage.color = backgroundColor;
                _backgroundImage.type = Image.Type.Simple;
                _backgroundImage.preserveAspect = true;
                _backgroundImage.raycastTarget = true;
            }

            if (_handleImage != null)
            {
                DestroyGeneratedSprite(ref _handleSprite);
                _handleSprite = CreateFilledCircleSprite(spriteSize);
                _handleImage.sprite = _handleSprite;
                _handleImage.color = handleColor;
                _handleImage.type = Image.Type.Simple;
                _handleImage.preserveAspect = true;
                _handleImage.raycastTarget = false;
            }
        }

        private static Sprite CreateFilledCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "JoystickHandleTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            float softEdge = Mathf.Max(1.5f, size * 0.03f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((radius - dist) / softEdge);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static Sprite CreateRingSprite(int size, float thicknessNormalized)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "JoystickBackgroundTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius * (1f - Mathf.Clamp(thicknessNormalized, 0.05f, 0.45f));
            float softEdge = Mathf.Max(1.5f, size * 0.03f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float outerAlpha = Mathf.Clamp01((outerRadius - dist) / softEdge);
                    float innerAlpha = Mathf.Clamp01((dist - innerRadius) / softEdge);
                    float alpha = outerAlpha * innerAlpha;

                    // Soft fill inside the ring so the touch area feels solid.
                    float fill = Mathf.Clamp01((innerRadius - dist) / softEdge) * 0.25f;
                    alpha = Mathf.Max(alpha, fill);

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
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
