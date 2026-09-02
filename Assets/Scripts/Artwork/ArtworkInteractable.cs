using System.Text.RegularExpressions;
using UnityEngine;

namespace FokusTour.Artwork
{
    /// <summary>
    /// Place on each artwork object. Requires a Collider for proximity detection.
    /// Set artworkId to match row id in MySQL (e.g. Artwork_01_01 → 1).
    /// Frame texture is loaded at runtime from API only.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ArtworkInteractable : MonoBehaviour
    {
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        [SerializeField] private int artworkId;

        private ArtworkItem _runtimeItem;

        public int ArtworkId => artworkId;

        public bool HasData => _runtimeItem != null && _runtimeItem.HasMetadata;

        private void Awake()
        {
            if (artworkId <= 0)
                artworkId = ParseIdFromObjectName(gameObject.name);

            ClearFrameTexture();
        }

        public ArtworkItem GetActiveItem()
        {
            return HasData ? _runtimeItem : null;
        }

        public void ApplyRuntimeData(ArtworkItem item)
        {
            _runtimeItem = item;
            ApplyFrameTexture(item?.PreviewTexture);
        }

        private void ApplyFrameTexture(Texture2D texture)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            Material material = renderer.material;
            if (material == null)
                return;

            if (texture == null)
            {
                material.SetTexture(BaseMapId, null);
                return;
            }

            material.SetTexture(BaseMapId, texture);
        }

        private void ClearFrameTexture()
        {
            ApplyFrameTexture(null);
        }

        private static int ParseIdFromObjectName(string objectName)
        {
            Match match = Regex.Match(objectName, @"Artwork_\d+_(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                return id;

            return 0;
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
#endif
    }
}
