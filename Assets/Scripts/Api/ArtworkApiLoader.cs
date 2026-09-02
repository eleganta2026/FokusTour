using System.Collections;
using System.Collections.Generic;
using FokusTour.Artwork;
using FokusTour.Player;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace FokusTour.Api
{
    /// <summary>
    /// Binds artwork data to ArtworkInteractable in the gallery scene.
    /// Prefers ArtworkSessionCache (prefetched from Main Menu); falls back to live API fetch.
    /// </summary>
    public class ArtworkApiLoader : MonoBehaviour
    {
        public static ArtworkApiLoader Instance { get; private set; }

        [Header("API")]
        [Tooltip("Preferred API URL. App tries HTTPS first, then HTTP.")]
        [SerializeField] private string apiUrl = ApiEndpoint.HttpsApiUrl;
        [SerializeField] private bool loadOnStart = true;

        [Header("Loading Gate")]
        [SerializeField] private bool lockPlayerUntilLoaded = true;
        [SerializeField] private bool showLoadingOverlay = true;

        private readonly Dictionary<int, ArtworkItem> _itemsById = new Dictionary<int, ArtworkItem>();

        private GameObject _loadingRoot;
        private TextMeshProUGUI _loadingStatusText;
        private FirstPersonController _playerController;
        private bool _playerWasEnabled;

        public bool IsLoaded { get; private set; }
        public string LastError { get; private set; }

        public string ApiUrl => apiUrl;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (loadOnStart)
                StartCoroutine(LoadAndBindRoutine());
        }

        public void Reload()
        {
            StopAllCoroutines();
            _itemsById.Clear();
            IsLoaded = false;
            LastError = null;
            StartCoroutine(LoadAndBindRoutine());
        }

        public bool TryGetItem(int artworkId, out ArtworkItem item)
        {
            return _itemsById.TryGetValue(artworkId, out item);
        }

        private IEnumerator LoadAndBindRoutine()
        {
            IsLoaded = false;
            LastError = null;

            BeginLoadingGate();
            SetLoadingStatus("Memuat data karya...");

            ArtworkSessionCache cache = ArtworkSessionCache.Instance;
            if (cache != null && cache.IsReady)
            {
                CopyFromCache(cache);
                SetLoadingStatus("Menyiapkan karya...");
                yield return BindInteractables();
                FinishLoadingGate(success: true);
                yield break;
            }

            yield return FetchFromApi();
            if (!string.IsNullOrEmpty(LastError))
            {
                SetLoadingStatus(LastError);
                FinishLoadingGate(success: false);
                yield break;
            }

            yield return BindInteractables();
            FinishLoadingGate(success: true);
        }

        private void CopyFromCache(ArtworkSessionCache cache)
        {
            _itemsById.Clear();
            foreach (KeyValuePair<int, ArtworkItem> pair in cache.Items)
                _itemsById[pair.Key] = pair.Value;
        }

        private IEnumerator FetchFromApi()
        {
            string workingUrl = null;
            yield return ApiEndpoint.ResolveWorkingApiUrl(
                apiUrl,
                SetLoadingStatus,
                url => workingUrl = url,
                err => LastError = err);

            if (string.IsNullOrEmpty(workingUrl))
            {
                if (string.IsNullOrEmpty(LastError))
                    LastError = "Gagal terhubung ke server.";
                Debug.LogError($"ArtworkApiLoader: {LastError}");
                yield break;
            }

            SetLoadingStatus("Mengunduh data karya...");

            using (UnityWebRequest request = UnityWebRequest.Get(workingUrl))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();

                if (ApiEndpoint.IsRequestFailed(request))
                {
                    LastError = request.error;
                    Debug.LogError($"ArtworkApiLoader: gagal memuat API ({workingUrl}). {request.error}");
                    yield break;
                }

                ArtworkListResponse response = JsonUtility.FromJson<ArtworkListResponse>(request.downloadHandler.text);
                if (response == null || !response.success || response.data == null)
                {
                    LastError = "Respons API tidak valid.";
                    Debug.LogError("ArtworkApiLoader: respons API tidak valid.");
                    yield break;
                }

                _itemsById.Clear();
                foreach (ArtworkDto dto in response.data)
                {
                    if (dto == null || dto.is_active == 0)
                        continue;

                    ArtworkItem item = ArtworkItem.FromDto(dto);
                    if (item != null)
                        _itemsById[item.Id] = item;
                }
            }

            if (_itemsById.Count == 0)
            {
                LastError = "Tidak ada data karya aktif.";
                yield break;
            }

            List<ArtworkItem> items = new List<ArtworkItem>(_itemsById.Values);
            for (int i = 0; i < items.Count; i++)
            {
                ArtworkItem item = items[i];
                SetLoadingStatus($"Mengunduh gambar ({i + 1}/{items.Count})...");

                if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    yield return ApiEndpoint.DownloadTexturePreferHttps(item.ImageUrl, texture =>
                    {
                        item.PreviewTexture = texture;
                    });
                }
            }
        }

        private IEnumerator BindInteractables()
        {
            ArtworkInteractable[] interactables = FindObjectsByType<ArtworkInteractable>(FindObjectsSortMode.None);
            foreach (ArtworkInteractable interactable in interactables)
            {
                int id = interactable.ArtworkId;
                if (id <= 0 || !_itemsById.TryGetValue(id, out ArtworkItem item))
                    continue;

                interactable.ApplyRuntimeData(item);
            }

            IsLoaded = true;
            Debug.Log($"ArtworkApiLoader: {_itemsById.Count} karya dimuat.");
            yield return null;
        }

        private void BeginLoadingGate()
        {
            if (lockPlayerUntilLoaded)
            {
                _playerController = FindFirstObjectByType<FirstPersonController>();
                if (_playerController != null)
                {
                    _playerWasEnabled = _playerController.enabled;
                    _playerController.enabled = false;
                }
            }

            if (showLoadingOverlay)
                EnsureLoadingOverlay();

            if (_loadingRoot != null)
                _loadingRoot.SetActive(true);
        }

        private void FinishLoadingGate(bool success)
        {
            IsLoaded = success || IsLoaded;

            if (_loadingRoot != null)
            {
                if (success)
                    _loadingRoot.SetActive(false);
                else
                    SetLoadingStatus(string.IsNullOrEmpty(LastError)
                        ? "Gagal memuat data."
                        : LastError + "\nKembali ke menu untuk mencoba lagi.");
            }

            if (success && lockPlayerUntilLoaded && _playerController != null)
                _playerController.enabled = _playerWasEnabled;
        }

        private void SetLoadingStatus(string message)
        {
            if (_loadingStatusText != null)
                _loadingStatusText.text = message;
        }

        private void EnsureLoadingOverlay()
        {
            if (_loadingRoot != null)
                return;

            GameObject canvasObject = new GameObject("TourLoadingCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject dimmerObject = new GameObject("Dimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dimmerObject.transform.SetParent(canvasObject.transform, false);
            RectTransform dimmerRect = dimmerObject.GetComponent<RectTransform>();
            dimmerRect.anchorMin = Vector2.zero;
            dimmerRect.anchorMax = Vector2.one;
            dimmerRect.offsetMin = Vector2.zero;
            dimmerRect.offsetMax = Vector2.zero;
            Image dimmer = dimmerObject.GetComponent<Image>();
            dimmer.color = new Color(0.05f, 0.06f, 0.08f, 0.92f);
            dimmer.raycastTarget = true;

            GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(dimmerObject.transform, false);
            TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
            title.text = "Memuat Tour";
            title.fontSize = 42f;
            title.fontStyle = FontStyles.Bold;
            title.color = Color.white;
            title.alignment = TextAlignmentOptions.Center;
            title.raycastTarget = false;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 40f);
            titleRect.sizeDelta = new Vector2(900f, 60f);

            GameObject statusObject = new GameObject("Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            statusObject.transform.SetParent(dimmerObject.transform, false);
            _loadingStatusText = statusObject.GetComponent<TextMeshProUGUI>();
            _loadingStatusText.text = "Memuat data karya...";
            _loadingStatusText.fontSize = 26f;
            _loadingStatusText.color = new Color(0.75f, 0.78f, 0.82f, 1f);
            _loadingStatusText.alignment = TextAlignmentOptions.Center;
            _loadingStatusText.raycastTarget = false;
            RectTransform statusRect = _loadingStatusText.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = new Vector2(0f, -20f);
            statusRect.sizeDelta = new Vector2(1000f, 100f);

            _loadingRoot = canvasObject;
        }
    }
}
