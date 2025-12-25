using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Product}")]
    public class ProductController : Controller
    {
        private const int PAGESIZE = 20;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchCondition>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };

            }
            return View(condition);
            //return View();
        }
        /*public async Task<IActionResult> Search(PaginationSearchCondition condition)
        {
            var data = await ProductDataService.ProductDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await ProductDataService.ProductDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        } */
        public async Task<IActionResult> Search(ProductSearchCondition condition)
        {
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

            // Lưu vào session
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var model = new ProductEditModel()
            {
                ProductID = 0,
                Photo = "nophoto.png"
            }
            ;
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var product = await ProductDataService.ProductDB.GetAsync(id);
            if (product == null)
                return RedirectToAction("Index");
            var attributes = await ProductDataService.ProductDB.ListAttributesAsync(id);
            var Photos = await ProductDataService.ProductDB.ListPhotosAsync(id);
            var model = new ProductEditModel()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                CategoryID = product.CategoryID,
                SupplierID = product.SupplierID,
                Unit = product.Unit,
                Price = product.Price,
                Photo = product.Photo,
                IsSelling = product.IsSelling,
                Attributes = attributes,
                Photos = Photos 
            };
            //if (model == null)
            //    return RedirectToAction("Index");

            return View(model);
        }

        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await ProductDataService.ProductDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await ProductDataService.ProductDB.GetAsync(id);
                if (model == null)
                    return RedirectToAction("Index");
                return View(model);
            }

        }
        public async Task<IActionResult> SaveData(ProductEditModel model)
        {
            try
            {
                ViewBag.Title = model.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                // Kiếm tra tính đúng đắn của dữ liệu nhập vào
                if (string.IsNullOrWhiteSpace(model.ProductName))
                    ModelState.AddModelError(nameof(model.ProductName), "Tên mặt hàng không được để trống");
                if (string.IsNullOrEmpty(model.Unit))
                    ModelState.AddModelError(nameof(model.Unit), "Đơn vị tính không được để trống");
                if (string.IsNullOrEmpty(model.Price.ToString()) || model.Price < 0)
                    ModelState.AddModelError(nameof(model.Price), "Giá bán phải lớn hơn hoặc bằng 0");
                if (string.IsNullOrEmpty(model.CategoryID.ToString()) || model.CategoryID <= 0)
                    ModelState.AddModelError(nameof(model.CategoryID), "Vui lòng chọn loại hàng");
                if (string.IsNullOrEmpty(model.SupplierID.ToString()) || model.SupplierID <= 0)
                    ModelState.AddModelError(nameof(model.SupplierID), "Vui lòng chọn nhà cung cấp");
                if (string.IsNullOrEmpty(model.ProductDescription))
                    ModelState.AddModelError(nameof(model.ProductDescription), "Mô tả mặt hàng không được để trống");

                if (model.UploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadPhoto.CopyToAsync(stream);
                    }
                    model.Photo = fileName;
                }
                if (!ModelState.IsValid)
                {
                    //ViewBag.Title = model.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                    return View("Edit", model);
                }
                Product data = new Product()
                {
                    ProductID = model.ProductID,
                    ProductName = model.ProductName,
                    ProductDescription = model.ProductDescription,
                    CategoryID = model.CategoryID,
                    SupplierID = model.SupplierID,
                    Unit = model.Unit,
                    Price = model.Price,
                    Photo = model.Photo,
                    IsSelling = model.IsSelling
                };
                if (model.ProductID == 0)
                {
                    await ProductDataService.ProductDB.AddAsync(data);
                }
                else
                {
                    await ProductDataService.ProductDB.UpdateAsync(data);
                }
                //return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                //return View("Edit", model);
            }
            return RedirectToAction("Index");
        }

        /*public IActionResult Photo(int id, string method = "", int PhotoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    return View();
                case "edit":
                    ViewBag.Title = "Cập nhật ảnh cho mặt hàng";
                    return View();
                case "delete":
                    // TODO: Xóa ảnh - xóa trực tiếp, không cần confirm 
                    return RedirectToAction("Edit", new {id = id });
                default:
                    return RedirectToAction("Index");
            }
           
        }*/
        public async Task<IActionResult> Photo(int id = 0, string method = "", int photoid = 0)
        {

            switch (method.ToLower())
            {
                case "add":
                    ViewBag.Title = "Thêm Ảnh";
                    ViewBag.method = "add";
                    var newPhoto = new ProductPhotoEditModel()
                    {
                        ProductId = id,
                        Photo = "nophoto.png"
                    };
                    return View(newPhoto);
                case "edit":
                    ViewBag.Title = "Chỉnh Sửa Ảnh";
                    var photo = await ProductDataService.ProductDB.GetPhotoAsync(photoid); // cau nay se tra ve model cua no can phai doi qua modeledit
                    if (photo == null)
                        return RedirectToAction("Index");
                    if (photo.ProductId == 0)
                        photo.ProductId = id; // hoặc giá trị phù hợp
                    var model = new ProductPhotoEditModel()
                    {
                        PhotoId = photo.PhotoId,
                        ProductId = photo.ProductId,
                        Photo = photo.Photo,
                        Description = photo.Description,
                        DisplayOrder = photo.DisplayOrder,
                        IsHidden = photo.IsHidden
                    };
                    ViewBag.Method = "edit";

                    return View(model);

                case "delete":
                    //TODO:Xóa Ảnh(Xóa trực tiếp,không cần hỏi lại)
                    var att = await ProductDataService.ProductDB.DeletePhotoAsync(photoid);
                    return RedirectToAction("Edit", new { id }); // quay về danh sách
                default:
                    return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> Attribute(int id = 0, string method = "", int photoid = 0, int attributeID = 0)
        {
            switch (method.ToLower())
            {
                case "add":
                    ViewBag.Title = "Thêm Thuộc Tính Sản Phẩm";
                    var newAttr = new ProductAttribute()
                    {
                        ProductID = id
                    };
                    ViewBag.method = "add";
                    return View(newAttr);

                case "edit":
                    ViewBag.Title = "Chỉnh Sửa Thuộc Tính Sản Phẩm";
                    var attr = await ProductDataService.ProductDB.GetAttributeAsync(attributeID);
                    if (attr == null)
                        return RedirectToAction("Index");

                    // đảm bảo ProductID luôn có giá trị
                    if (attr.ProductID == 0)
                        attr.ProductID = id; // hoặc giá trị phù hợp

                    ViewBag.Method = "edit";
                    return View(attr);

                case "delete":
                    //TODO:Xóa Ảnh(Xóa trực tiếp,không cần hỏi lại)
                    var att = await ProductDataService.ProductDB.DeleteAttributeAsync(attributeID);
                    return RedirectToAction("Edit", new { id }); // quay về danh sách
                default:
                    return RedirectToAction("Index");
            }
        }
        // Attibute Save Data 
        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute attribute, string method)
        {
            try
            {
                ViewBag.Method = method;
                // Tiêu đề view
                ViewBag.Title = attribute.AttributeID == 0 ? "Thêm Thuộc Tính Sản Phẩm" : "Chỉnh Sửa Thuộc Tính Sản Phẩm";

                // Validate
                if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                    ModelState.AddModelError(nameof(attribute.AttributeName), "Tên thuộc tính không được để trống");

                if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
                    ModelState.AddModelError(nameof(attribute.AttributeValue), "Giá trị thuộc tính không được để trống");

                // Nếu có lỗi thì trả về view Attribute
                if (!ModelState.IsValid)
                {

                    return View("Attribute", attribute);
                }
                if (method == "add")
                    await ProductDataService.ProductDB.AddAttributeAsync(attribute);
                else if (method == "edit")
                    await ProductDataService.ProductDB.UpdateAttributeAsync(attribute);

                // Thêm mới hoặc cập nhật

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Attribute", attribute);
            }

            // Sau khi lưu xong, quay về trang edit sản phẩm chứa attribute
            return RedirectToAction("Edit", new { id = attribute.ProductID });
        }
        // Photo Save Data
        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhotoEditModel model, string method)
        {
            try
            {
                ViewBag.Method = method;
                ViewBag.Title = model.PhotoId == 0 ? "Thêm Ảnh Sản Phẩm" : "Chỉnh Sửa Ảnh Sản Phẩm";

                // Validate
                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError(nameof(model.Description), "Mô tả ảnh không được để trống");

                if (model.UploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "products", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadPhoto.CopyToAsync(stream);
                    }
                    model.Photo = fileName;
                }
                else if (string.IsNullOrWhiteSpace(model.Photo))
                {
                    model.Photo = "nophoto.png";
                }

                if (!ModelState.IsValid)
                {
                    return View("Photo", model);
                }

                if (method == "add")
                    await ProductDataService.ProductDB.AddPhotoAsync(model);
                else if (method == "edit")
                    await ProductDataService.ProductDB.UpdatePhotoAsync(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Photo", model);
            }

            return RedirectToAction("Edit", new { id = model.ProductId });
        }

        /*[HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhotoEditModel photo, string method)
        {
            try
            {
                ViewBag.Method = method;
                // Tiêu đề view
                ViewBag.Title = photo.PhotoId == 0 ? "Thêm Ảnh Sản Phẩm" : "Chỉnh Sửa Ảnh Sản Phẩm";
                // Validate
                // Nếu có lỗi thì trả về view Photo
                if (string.IsNullOrWhiteSpace(photo.Description))
                    ModelState.AddModelError(nameof(photo.Description), "Mô tả ảnh không được để trống");
                if (photo.UploadPhoto != null)
                {
                    //xu ly upload anh
                    string fileName = $"{DateTime.Now.Ticks}_{photo.UploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.UploadPhoto.CopyToAsync(stream);
                    }
                    photo.Photo = fileName;
                }
                else if (string.IsNullOrWhiteSpace(photo.Photo))
                {
                    photo.Photo = "nophoto.png";
                }
                if (!ModelState.IsValid)
                {
                    return View("Photo", photo);
                }
                if (method == "add")
                    await ProductDataService.ProductDB.AddPhotoAsync(photo);
                else if (method == "edit")
                    await ProductDataService.ProductDB.UpdatePhotoAsync(photo);
                // Thêm mới hoặc cập nhật
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Photo", photo);
            }
            // Sau khi lưu xong, quay về trang edit sản phẩm chứa photo
            return RedirectToAction("Edit", new { id = photo.ProductId });

        }*/
    }
}
