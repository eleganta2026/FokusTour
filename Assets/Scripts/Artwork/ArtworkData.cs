using UnityEngine;

namespace FokusTour.Artwork
{
    [CreateAssetMenu(fileName = "ArtworkData", menuName = "FokusTour/Artwork Data")]
    public class ArtworkData : ScriptableObject
    {
        [SerializeField] private string title = "Judul Karya";
        [SerializeField] private string creatorName = "Nama Pembuat";
        [TextArea(3, 8)]
        [SerializeField] private string description = "Deskripsi karya.";
        [SerializeField] private Texture2D previewImage;

        public string Title => string.IsNullOrWhiteSpace(title) ? "Judul" : title;
        public string CreatorName => string.IsNullOrWhiteSpace(creatorName) ? "Nama Pembuat" : creatorName;
        public string Description => string.IsNullOrWhiteSpace(description) ? "Deskripsi" : description;
        public Texture2D PreviewImage => previewImage;
    }
}