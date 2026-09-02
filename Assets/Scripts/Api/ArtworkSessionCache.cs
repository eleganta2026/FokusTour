using System;
using System.Collections;
using System.Collections.Generic;
using FokusTour.Artwork;
using UnityEngine;
using UnityEngine.Networking;

namespace FokusTour.Api
{
    /// <summary>
    /// Prefetches artwork metadata + textures before entering the gallery scene.
    /// Survives scene loads via DontDestroyOnLoad.
    /// </summary>
    public class ArtworkSessionCache : MonoBehaviour
    {
        public static ArtworkSessionCache Instance { get; private set; }

        private readonly Dictionary<int, ArtworkItem> _itemsById = new Dictionary<int, ArtworkItem>();

        public bool IsReady { get; private set; }
        public string LastError { get; private set; }
        public string ResolvedApiUrl { get; private set; }
        public int TotalCount { get; private set; }
        public int LoadedTextureCount { get; private set; }

        public IReadOnlyDictionary<int, ArtworkItem> Items => _itemsById;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static ArtworkSessionCache EnsureExists()
        {
            if (Instance != null)
                return Instance;

            GameObject go = new GameObject("ArtworkSessionCache");
            return go.AddComponent<ArtworkSessionCache>();
        }

        public void Clear()
        {
            StopAllCoroutines();

            foreach (ArtworkItem item in _itemsById.Values)
            {
                if (item?.PreviewTexture != null)
                    Destroy(item.PreviewTexture);
            }

            _itemsById.Clear();
            IsReady = false;
            LastError = null;
            ResolvedApiUrl = null;
            TotalCount = 0;
            LoadedTextureCount = 0;
        }

        public bool TryGetItem(int artworkId, out ArtworkItem item)
        {
            return _itemsById.TryGetValue(artworkId, out item);
        }

        public IEnumerator Prefetch(string preferredApiUrl, Action<string> onStatus = null)
        {
            Clear();
            IsReady = false;
            LastError = null;

            string workingUrl = null;
            yield return ApiEndpoint.ResolveWorkingApiUrl(
                preferredApiUrl,
                onStatus,
                url => workingUrl = url,
                err => LastError = err);

            if (string.IsNullOrEmpty(workingUrl))
            {
                if (string.IsNullOrEmpty(LastError))
                    LastError = "Gagal terhubung ke server.";
                yield break;
            }

            ResolvedApiUrl = workingUrl;
            onStatus?.Invoke("Mengunduh data karya...");

            using (UnityWebRequest request = UnityWebRequest.Get(workingUrl))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();

                if (ApiEndpoint.IsRequestFailed(request))
                {
                    LastError = string.IsNullOrEmpty(request.error)
                        ? "Gagal terhubung ke server."
                        : request.error;
                    yield break;
                }

                ArtworkListResponse response = JsonUtility.FromJson<ArtworkListResponse>(request.downloadHandler.text);
                if (response == null || !response.success || response.data == null)
                {
                    LastError = "Respons API tidak valid.";
                    yield break;
                }

                foreach (ArtworkDto dto in response.data)
                {
                    if (dto == null || dto.is_active == 0)
                        continue;

                    ArtworkItem item = ArtworkItem.FromDto(dto);
                    if (item != null)
                        _itemsById[item.Id] = item;
                }
            }

            TotalCount = _itemsById.Count;
            if (TotalCount == 0)
            {
                LastError = "Tidak ada data karya aktif.";
                yield break;
            }

            LoadedTextureCount = 0;
            List<ArtworkItem> items = new List<ArtworkItem>(_itemsById.Values);
            for (int i = 0; i < items.Count; i++)
            {
                ArtworkItem item = items[i];
                onStatus?.Invoke($"Mengunduh gambar ({i + 1}/{items.Count})...");

                if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    yield return ApiEndpoint.DownloadTexturePreferHttps(item.ImageUrl, texture =>
                    {
                        item.PreviewTexture = texture;
                    });
                }

                LoadedTextureCount++;
            }

            IsReady = true;
            onStatus?.Invoke("Data siap.");
        }
    }
}
