using SV22T1080069.DomainModels;

namespace SV22T1080069.Shop.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new Product();
        public List<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public List<Product> RelatedProducts { get; set; } = new();   // mới
    }
}
