using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace FokusTour.Api
{
    /// <summary>
    /// Resolves API URL: prefer HTTPS, fall back to HTTP.
    /// Default host: fokustour.my.id
    /// </summary>
    public static class ApiEndpoint
    {
        public const string DefaultHost = "fokustour.my.id";
        public const string DefaultApiPath = "/api/artworks.php";

        public static string HttpsApiUrl => "https://" + DefaultHost + DefaultApiPath;
        public static string HttpApiUrl => "http://" + DefaultHost + DefaultApiPath;

        public static string ToHttps(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase))
                return "https://" + url.Substring("http://".Length);

            return url;
        }

        public static string ToHttp(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                return "http://" + url.Substring("https://".Length);

            return url;
        }

        public static bool IsRequestFailed(UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            return request.result != UnityWebRequest.Result.Success;
#else
            return request.isNetworkError || request.isHttpError;
#endif
        }

        public static string BuildCandidate(string preferredUrl, bool preferHttps)
        {
            string url = string.IsNullOrWhiteSpace(preferredUrl) ? HttpsApiUrl : preferredUrl.Trim();

            if (!url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
            {
                url = (preferHttps ? "https://" : "http://") + url.TrimStart('/');
            }

            return preferHttps ? ToHttps(url) : ToHttp(url);
        }

        /// <summary>
        /// Probe HTTPS then HTTP. Invokes onResolved with the working URL, or onFailed.
        /// </summary>
        public static IEnumerator ResolveWorkingApiUrl(
            string preferredUrl,
            System.Action<string> onStatus,
            System.Action<string> onResolved,
            System.Action<string> onFailed)
        {
            string httpsUrl = BuildCandidate(preferredUrl, preferHttps: true);
            string httpUrl = BuildCandidate(preferredUrl, preferHttps: false);

            onStatus?.Invoke("Menghubungkan (HTTPS)...");
            string httpsError = null;
            bool httpsOk = false;
            yield return ProbeGet(httpsUrl, (ok, err) =>
            {
                httpsOk = ok;
                httpsError = err;
            });

            if (httpsOk)
            {
                onResolved?.Invoke(httpsUrl);
                yield break;
            }

            onStatus?.Invoke("HTTPS gagal, mencoba HTTP...");
            string httpError = null;
            bool httpOk = false;
            yield return ProbeGet(httpUrl, (ok, err) =>
            {
                httpOk = ok;
                httpError = err;
            });

            if (httpOk)
            {
                onResolved?.Invoke(httpUrl);
                yield break;
            }

            string detail = !string.IsNullOrEmpty(httpsError) ? httpsError : httpError;
            if (string.IsNullOrEmpty(detail))
                detail = "Tidak dapat terhubung ke server.";

            onFailed?.Invoke(detail);
        }

        /// <summary>
        /// Download texture preferring HTTPS, then HTTP equivalent.
        /// </summary>
        public static IEnumerator DownloadTexturePreferHttps(string imageUrl, System.Action<Texture2D> onSuccess)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                yield break;

            string httpsUrl = ToHttps(imageUrl);
            string httpUrl = ToHttp(imageUrl);

            Texture2D texture = null;
            yield return TryDownloadTexture(httpsUrl, t => texture = t);
            if (texture != null)
            {
                onSuccess?.Invoke(texture);
                yield break;
            }

            if (!string.Equals(httpsUrl, httpUrl, System.StringComparison.OrdinalIgnoreCase))
            {
                yield return TryDownloadTexture(httpUrl, t => texture = t);
                if (texture != null)
                {
                    onSuccess?.Invoke(texture);
                    yield break;
                }
            }

            Debug.LogWarning($"ApiEndpoint: gagal unduh gambar ({imageUrl}).");
        }

        private static IEnumerator ProbeGet(string url, System.Action<bool, string> done)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 12;
                yield return request.SendWebRequest();

                if (IsRequestFailed(request))
                {
                    done?.Invoke(false, string.IsNullOrEmpty(request.error) ? "Koneksi gagal." : request.error);
                    yield break;
                }

                // Accept any successful HTTP response (JSON validated later by caller).
                done?.Invoke(true, null);
            }
        }

        private static IEnumerator TryDownloadTexture(string url, System.Action<Texture2D> done)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = 20;
                yield return request.SendWebRequest();

                if (IsRequestFailed(request))
                {
                    done?.Invoke(null);
                    yield break;
                }

                done?.Invoke(DownloadHandlerTexture.GetContent(request));
            }
        }
    }
}
