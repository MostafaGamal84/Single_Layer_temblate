using API.Entities.Auctions;
using DTOs;

namespace API.DTOs.ItemPhotoDto
{
    public class ItemPhotoAddDto : BaseDto
    {
        public string FileBase64 { get; set; }
    }
}