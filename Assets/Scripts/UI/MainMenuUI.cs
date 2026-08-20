using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FokusTour.UI
{
    /// <summary>
    /// Builds the entire Main Menu UI at runtime (landscape mobile).
    /// Attach to an empty GameObject in MainMenuScene.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string gallerySceneName = "MainScene";

        [Header("Content")]
        [SerializeField] [TextArea(4, 12)]
        private string sejarahText =
            "PROFIL UKM FOTOGRAFI KAMPUS UMPAR\n\n" +
            "1. Nama Organisasi\n" +
            "Unit Kegiatan Mahasiswa Fotografi Kampus Universitas Muhammadiyah Parepare, " +
            "disingkat UKM FOKUS UMPAR.\n\n" +
            "2. Maksud, Tujuan, Visi dan Misi\n" +
            "Mengembangkan potensi mahasiswa pada bidang Fotografi, Videografi, dan Pers, " +
            "baik secara praktis maupun teknis, agar bermanfaat bagi intra maupun ekstra kampus, " +
            "serta mengacu pada visi dan misi Universitas demi terwujudnya mahasiswa yang kreatif " +
            "dan kompetitif dalam ipteks.\n\n" +
            "Visi dan misi UKM Fotografi Kampus meliputi:\n" +
            "a. Menghasilkan model pengembangan kreativitas mahasiswa di bidang Fotografi, " +
            "Videografi, dan Pers yang terencana, terprogram, dan berkelanjutan.\n" +
            "b. Meningkatkan relevansi kegiatan mahasiswa untuk memperkuat komitmen, semangat, " +
            "kemandirian, dan ketangguhan.\n" +
            "c. Meningkatkan kerja sama dan kemitraan dengan dunia usaha agar lulusan memiliki " +
            "daya saing di dunia kerja.\n\n" +
            "3. Sejarah Singkat\n" +
            "Pencetusan UKM Fotografi Kampus UMPAR diawali diskusi lepas pada bulan Ramadhan, " +
            "15 Agustus 2010, oleh mahasiswa dari berbagai fakultas, antara lain Ridwan Syam, " +
            "Ade Riswan, Kaharuddin Hanun, Fathuddin Djollong, Ali Wira Rahman, dan sejumlah " +
            "mahasiswa lainnya.\n\n" +
            "Dari diskusi tersebut muncul keinginan mendirikan UKM sebagai wadah kompetitif " +
            "di bidang Fotografi, Videografi, dan Pers. Pada rapat paripurna 24 Agustus 2010 " +
            "dibentuk struktur pengurus dan dibahas AD/ART. Selanjutnya disusun proposal " +
            "pendirian, dikumpulkan dukungan mahasiswa antar fakultas, lalu diajukan melalui " +
            "BEM UMPAR kepada pimpinan universitas untuk melegalkan organisasi.\n\n" +
            "4. Atribut Logo UKM FOKUS UMPAR\n" +
            "a. Sinar Matahari Biru\n" +
            "Melambangkan sinar sebagai simbol Perserikatan Muhammadiyah.\n\n" +
            "b. Lingkaran Biru\n" +
            "Melambangkan Ikatan Mahasiswa Universitas Muhammadiyah Parepare.\n\n" +
            "c. Lingkaran Merah\n" +
            "Lingkaran berwarna merah dengan dasar hitam di dalamnya, melambangkan " +
            "wujud gerakan dan tujuan UKM FOKUS UMPAR.\n\n" +
            "Sumber: ukmfokusumpar.blogspot.com";

        [SerializeField] [TextArea(4, 12)]
        private string tentangPembuatText =
            "Aplikasi FokusTour dikembangkan sebagai tugas akhir / skripsi " +
            "Program Studi Teknik Informatika.\n\n" +
            "Judul:\n" +
            "Rancang Bangun Virtual Tour Hasil Karya Fotografi UKM Fokus " +
            "Universitas Muhammadiyah Parepare\n\n" +
            "Pembuat:\n" +
            "Chaerul Anwar\n" +
            "NIM. 219280182\n\n" +
            "Aplikasi ini menampilkan hasil karya fotografi UKM Fokus dalam bentuk " +
            "virtual tour berbasis 3D, sehingga pengguna dapat menjelajahi ruang " +
            "pameran virtual dan melihat informasi tiap karya.";

        [Header("Look")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);
        [SerializeField] private Color cardColor = new Color(0.10f, 0.11f, 0.14f, 0.96f);
        [SerializeField] private Color buttonColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color primaryButtonColor = new Color(0.92f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color subtitleColor = new Color(0.72f, 0.75f, 0.80f, 1f);
        [SerializeField] private Color buttonLabelColor = Color.white;
        [SerializeField] private Color primaryButtonLabelColor = new Color(0.12f, 0.13f, 0.15f, 1f);
        [SerializeField] private float cardCornerRadius = 28f;
        [SerializeField] private float buttonCornerRadius = 18f;

        private GameObject _infoPanel;
        private TextMeshProUGUI _infoTitle;
        private TextMeshProUGUI _infoBody;
        private RawImage _infoLogo;
        private RectTransform _infoContent;
        private Sprite _roundedCardSprite;
        private Sprite _roundedButtonSprite;
        private const float LogoSize = 180f;
        private const float LogoSpacing = 24f;

        private void Awake()
        {
            EnsureCamera();
            EnsureEventSystem();
            _roundedCardSprite = CreateRoundedSprite(128, 40f);
            _roundedButtonSprite = CreateRoundedSprite(128, 32f);
            BuildUI();
        }

        private void OnDestroy()
        {
            DestroyGeneratedSprite(ref _roundedCardSprite);
            DestroyGeneratedSprite(ref _roundedButtonSprite);
        }

        private void EnsureCamera()
        {
            if (Camera.main != null)
                return;

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildUI()
        {
            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            CreateBackground(canvasObject.transform);
            CreateMenuCard(canvasObject.transform);
            CreateInfoPanel(canvasObject.transform);
        }

        private void CreateBackground(Transform parent)
        {
            Image background = CreateImage("Background", parent);
            StretchFull(background.rectTransform);
            background.color = backgroundColor;
            background.raycastTarget = false;
        }

        private void CreateMenuCard(Transform parent)
        {
            Image card = CreateImage("MenuCard", parent);
            RectTransform cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 620f);
            cardRect.anchoredPosition = Vector2.zero;
            ApplyRoundedSprite(card, _roundedCardSprite, cardColor, 1.15f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "FokusTour", 54f, FontStyles.Bold, titleColor);
            SetTopCenter(title.rectTransform, 0f, -48f, 680f, 70f);

            TextMeshProUGUI subtitle = CreateText(
                "Subtitle",
                card.transform,
                "Virtual Tour Karya Fotografi UKM Fokus\nUniversitas Muhammadiyah Parepare",
                22f,
                FontStyles.Normal,
                subtitleColor);
            SetTopCenter(subtitle.rectTransform, 0f, -118f, 680f, 56f);

            float buttonY = -210f;
            float buttonGap = 88f;
            Vector2 buttonSize = new Vector2(420f, 72f);

            CreateMenuButton(card.transform, "Mulai Tour", buttonSize, buttonY, primaryButtonColor, primaryButtonLabelColor, StartTour);
            CreateMenuButton(card.transform, "Sejarah UKM Fokus", buttonSize, buttonY - buttonGap, buttonColor, buttonLabelColor, ShowSejarah);
            CreateMenuButton(card.transform, "Tentang Pembuat", buttonSize, buttonY - buttonGap * 2f, buttonColor, buttonLabelColor, ShowTentangPembuat);
            CreateMenuButton(card.transform, "Keluar", buttonSize, buttonY - buttonGap * 3f, buttonColor, buttonLabelColor, QuitApp);
        }

        private void CreateInfoPanel(Transform parent)
        {
            Image dimmer = CreateImage("InfoDimmer", parent);
            StretchFull(dimmer.rectTransform);
            dimmer.color = new Color(0f, 0f, 0f, 0.55f);

            Image panel = CreateImage("InfoPanel", dimmer.transform);
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.04f, 0.05f);
            panelRect.anchorMax = new Vector2(0.96f, 0.95f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            ApplyRoundedSprite(panel, _roundedCardSprite, cardColor, 1.15f);

            _infoTitle = CreateText("InfoTitle", panel.transform, "Judul", 42f, FontStyles.Bold, titleColor);
            SetTopCenter(_infoTitle.rectTransform, 0f, -44f, 1400f, 56f);
            _infoTitle.alignment = TextAlignmentOptions.Center;

            GameObject viewportObject = new GameObject("InfoViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(panel.transform, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(56f, 110f);
            viewportRect.offsetMax = new Vector2(-56f, -118f);
            viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

            GameObject contentObject = new GameObject("InfoContent", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            _infoContent = contentObject.GetComponent<RectTransform>();
            _infoContent.anchorMin = new Vector2(0f, 1f);
            _infoContent.anchorMax = new Vector2(1f, 1f);
            _infoContent.pivot = new Vector2(0.5f, 1f);
            _infoContent.anchoredPosition = Vector2.zero;
            _infoContent.sizeDelta = new Vector2(0f, 1200f);

            GameObject logoObject = new GameObject("InfoLogo", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            logoObject.transform.SetParent(contentObject.transform, false);
            _infoLogo = logoObject.GetComponent<RawImage>();
            RectTransform logoRect = _infoLogo.rectTransform;
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.pivot = new Vector2(0.5f, 1f);
            logoRect.anchoredPosition = new Vector2(0f, -8f);
            logoRect.sizeDelta = new Vector2(LogoSize, LogoSize);
            _infoLogo.color = Color.white;
            _infoLogo.raycastTarget = false;
            _infoLogo.texture = Resources.Load<Texture2D>("Logo_UKM_FOKUS");
            _infoLogo.gameObject.SetActive(false);

            _infoBody = CreateText("InfoBody", contentObject.transform, "Isi", 28f, FontStyles.Normal, subtitleColor);
            RectTransform bodyRect = _infoBody.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.anchoredPosition = new Vector2(0f, -(LogoSize + LogoSpacing));
            bodyRect.sizeDelta = new Vector2(0f, 800f);
            _infoBody.alignment = TextAlignmentOptions.Top;
            _infoBody.horizontalAlignment = HorizontalAlignmentOptions.Center;
            _infoBody.verticalAlignment = VerticalAlignmentOptions.Top;
            _infoBody.textWrappingMode = TextWrappingModes.Normal;
            _infoBody.overflowMode = TextOverflowModes.Overflow;
            _infoBody.lineSpacing = 12f;

            ScrollRect scroll = panel.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = _infoContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45f;

            CreateMenuButton(panel.transform, "Tutup", new Vector2(200f, 60f), 0f, primaryButtonColor, primaryButtonLabelColor, HideInfoPanel, true);

            _infoPanel = dimmer.gameObject;
            _infoPanel.SetActive(false);
        }

        private void RefreshInfoContentHeight(bool showLogo)
        {
            if (_infoBody == null || _infoContent == null)
                return;

            float logoBlock = showLogo ? LogoSize + LogoSpacing : 0f;
            RectTransform bodyRect = _infoBody.rectTransform;
            bodyRect.anchoredPosition = new Vector2(0f, -logoBlock);

            float width = _infoContent.rect.width;
            if (width < 10f)
                width = 1400f;

            Vector2 preferred = _infoBody.GetPreferredValues(_infoBody.text, width, 0f);
            float bodyHeight = Mathf.Max(preferred.y + 40f, 400f);
            bodyRect.sizeDelta = new Vector2(0f, bodyHeight);
            _infoContent.sizeDelta = new Vector2(0f, logoBlock + bodyHeight);
            _infoContent.anchoredPosition = Vector2.zero;
        }

        private void CreateMenuButton(
            Transform parent,
            string label,
            Vector2 size,
            float anchoredY,
            Color background,
            Color labelColor,
            UnityEngine.Events.UnityAction onClick,
            bool bottomRight = false)
        {
            GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (bottomRight)
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-40f, 32f);
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, anchoredY);
            }

            rect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            ApplyRoundedSprite(image, _roundedButtonSprite, background, 1.25f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            TextMeshProUGUI text = CreateText(label + "Label", buttonObject.transform, label, 26f, FontStyles.Bold, labelColor);
            StretchFull(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        private void StartTour()
        {
            SceneManager.LoadScene(gallerySceneName);
        }

        private void ShowSejarah()
        {
            ShowInfoPanel("Sejarah UKM Fokus", sejarahText, true);
        }

        private void ShowTentangPembuat()
        {
            ShowInfoPanel("Tentang Pembuat", tentangPembuatText, false);
        }

        private void ShowInfoPanel(string title, string body, bool showLogo)
        {
            if (_infoPanel == null)
                return;

            _infoTitle.text = title;
            _infoBody.text = body;

            if (_infoLogo != null)
            {
                if (_infoLogo.texture == null)
                    _infoLogo.texture = Resources.Load<Texture2D>("Logo_UKM_FOKUS");

                _infoLogo.gameObject.SetActive(showLogo && _infoLogo.texture != null);
            }

            _infoPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            RefreshInfoContentHeight(showLogo && _infoLogo != null && _infoLogo.texture != null);
        }

        private void HideInfoPanel()
        {
            if (_infoPanel != null)
                _infoPanel.SetActive(false);
        }

        private void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void ApplyRoundedSprite(Image image, Sprite sprite, Color color, float pixelsPerUnitMultiplier = 1f)
        {
            if (image == null)
                return;

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                image.useSpriteMesh = false;
            }

            image.color = color;
        }

        private static Sprite CreateRoundedSprite(int size, float cornerRadiusPixels)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "RoundedUITexture",
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

        private static Image CreateImage(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string content,
            float fontSize,
            FontStyles style,
            Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetTopCenter(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
