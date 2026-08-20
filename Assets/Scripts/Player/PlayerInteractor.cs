using FokusTour.Artwork;
using FokusTour.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FokusTour.Player
{
    /// <summary>
    /// Detects nearby artwork and handles interact input (UI button + keyboard E).
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform detectionOrigin;
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private LayerMask detectionMask = ~0;

        [Header("UI")]
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private Button interactButton;
        [SerializeField] private ArtworkInfoUI artworkInfoUI;

        [Header("Player")]
        [SerializeField] private FirstPersonController firstPersonController;

        [Header("Editor Fallback")]
        [SerializeField] private bool enableKeyboardFallback = true;

        private ArtworkInteractable _currentArtwork;
        private readonly Collider[] _overlapResults = new Collider[16];

        private void Awake()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;

            if (firstPersonController == null)
                firstPersonController = GetComponent<FirstPersonController>();

            SetPromptVisible(false);

            if (interactButton != null)
                interactButton.onClick.AddListener(TryInteract);
        }

        private void OnDestroy()
        {
            if (interactButton != null)
                interactButton.onClick.RemoveListener(TryInteract);
        }

        private void Update()
        {
            if (artworkInfoUI != null && artworkInfoUI.IsOpen)
            {
                SetCurrentArtwork(null);
                return;
            }

            UpdateClosestArtwork();

            if (!enableKeyboardFallback)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                TryInteract();
        }

        public void TryInteract()
        {
            if (_currentArtwork == null || !_currentArtwork.HasData)
                return;

            if (artworkInfoUI == null)
                return;

            artworkInfoUI.Open(_currentArtwork.Data, OnInfoClosed);
            SetPromptVisible(false);
            SetPlayerControlEnabled(false);
        }

        private void OnInfoClosed()
        {
            SetPlayerControlEnabled(true);
            UpdateClosestArtwork();
        }

        private void UpdateClosestArtwork()
        {
            Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
            int count = Physics.OverlapSphereNonAlloc(
                origin,
                interactRange,
                _overlapResults,
                detectionMask,
                QueryTriggerInteraction.Collide);

            ArtworkInteractable closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider hit = _overlapResults[i];
                if (hit == null)
                    continue;

                ArtworkInteractable artwork = hit.GetComponentInParent<ArtworkInteractable>();
                if (artwork == null || !artwork.HasData)
                    continue;

                float distance = Vector3.Distance(origin, artwork.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = artwork;
                }
            }

            SetCurrentArtwork(closest);
        }

        private void SetCurrentArtwork(ArtworkInteractable artwork)
        {
            if (_currentArtwork == artwork)
                return;

            _currentArtwork = artwork;
            SetPromptVisible(_currentArtwork != null);
        }

        private void SetPromptVisible(bool visible)
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(visible);
        }

        private void SetPlayerControlEnabled(bool enabled)
        {
            if (firstPersonController != null)
                firstPersonController.enabled = enabled;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.25f);
            Gizmos.DrawWireSphere(origin, interactRange);
        }
#endif
    }
}
