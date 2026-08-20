using UnityEngine;

namespace FokusTour.Artwork
{
    /// <summary>
    /// Place on each artwork object. Requires a Collider for proximity detection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ArtworkInteractable : MonoBehaviour
    {
        [SerializeField] private ArtworkData data;

        public ArtworkData Data => data;
        public bool HasData => data != null;

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
