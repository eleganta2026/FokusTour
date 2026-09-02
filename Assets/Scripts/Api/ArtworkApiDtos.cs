using System;

namespace FokusTour.Api
{
    [Serializable]
    public class ArtworkListResponse
    {
        public bool success;
        public int count;
        public ArtworkDto[] data;
    }

    [Serializable]
    public class ArtworkDto
    {
        public int id;
        public string title;
        public string creator_name;
        public string description;
        public string image_url;
        public int is_active;
    }
}
