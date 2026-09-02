using FokusTour.Api;
using UnityEngine;

namespace FokusTour.Artwork
{
    /// <summary>
    /// Runtime artwork data loaded from API.
    /// </summary>
    public class ArtworkItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatorName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Texture2D PreviewTexture { get; set; }

        public bool HasMetadata =>
            !string.IsNullOrWhiteSpace(Title) ||
            !string.IsNullOrWhiteSpace(CreatorName) ||
            !string.IsNullOrWhiteSpace(Description);

        public static ArtworkItem FromDto(ArtworkDto dto)
        {
            if (dto == null)
                return null;

            return new ArtworkItem
            {
                Id = dto.id,
                Title = dto.title,
                CreatorName = dto.creator_name,
                Description = dto.description,
                ImageUrl = dto.image_url,
            };
        }
    }
}
