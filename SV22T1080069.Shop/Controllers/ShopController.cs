using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using SV22T1080069.Shop.Models;

namespace SV22T1080069.Shop.Controllers
{
    public class ShopController : Controller
    {
        private const int PAGESIZE = 12;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";
        /*public async Task<IActionResult> Index(ProductSearchCondition condition)
        {
            if (condition.Page <= 0) condition.Page = 1;
            if (condition.PageSize <= 0) condition.PageSize = 12;

            var data = await ProductDataService.ProductDB.ListAsync(
                page: condition.Page,
                pageSize: condition.PageSize,
                searchValue: condition.SearchValue,
                categoryID: condition.CategoryID,
                supplierID: condition.SupplierID,
                minPrice: condition.MinPrice,
                maxPrice: condition.MaxPrice
            );

            var rowCount = await ProductDataService.ProductDB.CountAsync(
                searchValue: condition.SearchValue,
                categoryID: condition.CategoryID,
                supplierID: condition.SupplierID,
                minPrice: condition.MinPrice,
                maxPrice: condition.MaxPrice
            );

            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            return View(model);        // Views/Shop/Index.cshtml dùng model này
        } */
        /// <summary>
        /// Đưa dữ liệu ra trang tìm kiếm sản phẩm với phân trang
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(ProductSearchCondition condition)
        {
            if (condition.Page <= 0) condition.Page = 1;
            if (condition.PageSize <= 0) condition.PageSize = 12;

            // Lấy danh mục
            var categories = await SelectListHelper.Categories();

            // Lấy min/max giá toàn bộ
            var minPriceAll = await ProductDataService.ProductDB.GetMinPriceAsync();
            var maxPriceAll = await ProductDataService.ProductDB.GetMaxPriceAsync();

            //var data = await ProductDataService.ProductDB.ListAsync()
            var data = await ProductDataService.ProductDB.ListSortedAsync(
             page: condition.Page,
             pageSize: condition.PageSize,
             searchValue: condition.SearchValue,
             categoryID: condition.CategoryID,
             supplierID: condition.SupplierID,
             minPrice: condition.MinPrice,
             maxPrice: condition.MaxPrice,
             sortBy: condition.SortBy   // CHỈ THÊM DÒNG NÀY
            );
            var rowCount = await ProductDataService.ProductDB.CountForShopAsync(
                searchValue: condition.SearchValue,
                categoryID: condition.CategoryID,
                supplierID: condition.SupplierID,
                minPrice: condition.MinPrice,
                maxPrice: condition.MaxPrice
            );

            var model = new ProductSearchResult
            {
                // Thuộc tính PaginationSearchResult<Product>
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data,

                // Thuộc tính riêng
                SearchInput = condition,
                Categories = categories,
                MinPriceAll = minPriceAll,
                MaxPriceAll = maxPriceAll
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Filter(ProductSearchCondition condition)
        {
            if (condition.Page <= 0) condition.Page = 1;
            if (condition.PageSize <= 0) condition.PageSize = 12;

            var categories = await SelectListHelper.Categories();
            var minPriceAll = await ProductDataService.ProductDB.GetMinPriceAsync();
            var maxPriceAll = await ProductDataService.ProductDB.GetMaxPriceAsync();

            var data = await ProductDataService.ProductDB.ListSortedAsync(
                condition.Page, condition.PageSize,
                condition.SearchValue, condition.CategoryID, condition.SupplierID,
                condition.MinPrice, condition.MaxPrice, condition.SortBy
            );

            var rowCount = await ProductDataService.ProductDB.CountForShopAsync(
                condition.SearchValue, condition.CategoryID, condition.SupplierID,
                condition.MinPrice, condition.MaxPrice
            );

            var model = new ProductSearchResult
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data,
                SearchInput = condition,
                Categories = categories,
                MinPriceAll = minPriceAll,  
                MaxPriceAll = maxPriceAll
            };

            return PartialView("_ProductList", model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await ProductDataService.ProductDB.GetAsync(id);
            if (product == null)
                return NotFound();
            var photos = (await ProductDataService.ProductDB.ListPhotosAsync(id)).ToList();
            var attributes = await ProductDataService.ProductDB
                         .ListAttributesAsync(product.ProductID);

            if (!string.IsNullOrEmpty(product.Photo))
            {
                photos.Insert(0, new ProductPhoto
                {
                    Photo = product.Photo,
                    Description = product.ProductName,
                    ProductId = product.ProductID,
                    DisplayOrder = 0
                });
            }
            // Lấy tối đa 4 sản phẩm cùng danh mục, khác ProductID hiện tại
            var related = (await ProductDataService.ProductDB.ListSortedAsync(
                               page: 1,
                               pageSize: 10,
                               searchValue: "",
                               categoryID: product.CategoryID,
                               supplierID: 0,
                               minPrice: 0,
                               maxPrice: 0,
                               sortBy: ""        // hoặc "newest", tuỳ bạn
                           ))
                           .Where(p => p.ProductID != product.ProductID)
                           .Take(8)
                           .ToList();
            // ListSortedAsync + Where/Take là cách đơn giản để lấy sản phẩm liên quan theo danh mục. [web:151][web:146]

            var model = new ProductDetailViewModel
            {
                Product = product,
                Photos = photos,
                Attributes = attributes?.ToList() ?? new List<ProductAttribute>(),
                RelatedProducts = related
            };

            return View(model); 
        }

    }
}
