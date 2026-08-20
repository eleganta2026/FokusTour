using System;
using FokusTour.Artwork;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FokusTour.UI
{
    /// <summary>
    /// Simple mobile panel that shows artwork information (TextMeshPro).
    /// </summary>
    public class ArtworkInfoUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI creatorText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private Button closeButton;

        private Action _onClosed;
        private ArtworkInfoPanelLayout _layout;

        public bool IsOpen { get; private set; }

        public GameObject PanelRoot => panelRoot;
        public TextMeshProUGUI TitleText => titleText;
        public TextMeshProUGUI CreatorText => creatorText;
        public TextMeshProUGUI DescriptionText => descriptionText;
        public RawImage PreviewImage => previewImage;
        public Button CloseButton => closeButton;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            _layout = GetComponent<ArtworkInfoPanelLayout>();
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                Close();
        }

        public void Open(ArtworkData data, Action onClosed = null)
        {
            if (data == null)
                return;

            _onClosed = onClosed;

            if (titleText != null)
                titleText.text = data.Title;

            if (creatorText != null)
                creatorText.text = $"Fotografer: {data.CreatorName}";

            if (descriptionText != null)
                descriptionText.text = data.Description;

            if (previewImage != null)
            {
                bool hasImage = data.PreviewImage != null;
                previewImage.gameObject.SetActive(hasImage);
                if (hasImage)
                    previewImage.texture = data.PreviewImage;
            }

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (_layout == null)
                _layout = GetComponent<ArtworkInfoPanelLayout>();

            _layout?.Apply();
            IsOpen = true;
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            HideImmediate();

            Action callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }

        private void HideImmediate()
        {
            IsOpen = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            _layout?.SetDimmerVisible(false);
        }
    }
}
