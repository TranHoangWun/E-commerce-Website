using SV22T1080069.DomainModels;
namespace SV22T1080069.Admin.Models
{
    public class ProductPhotoEditModel : ProductPhoto
    {
        public IFormFile? UploadPhoto { get; set; }
    }
}
