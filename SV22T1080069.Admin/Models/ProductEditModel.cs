using SV22T1080069.DomainModels;

namespace SV22T1080069.Admin.Models
{
    /// <summary>
    /// ViewModel dùng cho chức năng bổ sung/cập nhật Product
    /// </summary>
    public class ProductEditModel : Product
    {
        public IFormFile? UploadPhoto { get; set; }
        public IEnumerable<ProductAttribute>? Attributes { get; set; }
        public IEnumerable<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
    }
}
