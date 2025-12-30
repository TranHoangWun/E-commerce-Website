using Dapper;
using SV22T1080069.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DataLayers
{
    public class ProductDAL : BaseDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng mỗi trang (0 thì không phân trang)</param>
        /// <param name="searchValue">Giá trị tìm kiếm (rỗng lấy toàn bộ)</param>
        /// <param name="categoryID">Mã loại hàng (chọn loại hàng = lấy tất cả)</param>
        /// <param name="supplierID">Mã mặt hàng</param>
        /// <param name="minPrice">Giá min</param>
        /// <param name="maxPrice">Giá max</param>
        /// <returns></returns>
        public async Task<IEnumerable<DomainModels.Product>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0,
                                                                        decimal minPrice = 0, decimal maxPrice = 0)
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";

            using var connection = await OpenConnectionAsync();
            var sql = @"with cte as
                        (
                            select  *,
                                    row_number() over(order by ProductName) as RowNumber
                            from    Products 
                            where   (ProductName like @SearchValue)
                                and (@CategoryID = 0 or CategoryID = @CategoryID)
                                and (@SupplierID = 0 or SupplierId = @SupplierID)
                                and (Price >= @MinPrice)
                                and (@MaxPrice <= 0 or Price <= @MaxPrice)
                        )
                        select * from cte 
                        where   (@PageSize = 0) 
                            or (RowNumber between (@Page - 1)*@PageSize + 1 and @Page * @PageSize)";
            var parameters = new
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                SupplierID = supplierID,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };
            return await connection.QueryAsync<Product>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Danh sách mặt hàng có sắp xếp theo yêu cầu
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <param name="categoryID"></param>
        /// <param name="supplierID"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> ListSortedAsync(
                    int page = 1, int pageSize = 0,
                    string searchValue = "",
                    int categoryID = 0, int supplierID = 0,
                    decimal minPrice = 0, decimal maxPrice = 0,
                    string sortBy = ""
                            )
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";

            var orderClause = sortBy switch
            {
                "name_asc" => "ProductName ASC",
                "name_desc" => "ProductName DESC",
                "price_asc" => "Price ASC",
                "price_desc" => "Price DESC",
                _ => "ProductName ASC"
            };

            using var connection = await OpenConnectionAsync();

            var sql = $@"
    WITH cte AS (
        SELECT *,
               ROW_NUMBER() OVER(ORDER BY {orderClause}) AS RowNumber
        FROM Products
        WHERE (ProductName LIKE @SearchValue)
          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
          AND (@SupplierID = 0 OR SupplierId = @SupplierID)
          AND (Price >= @MinPrice)
          AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
    )
    SELECT *
    FROM cte
    WHERE (@PageSize = 0)
       OR (RowNumber BETWEEN (@Page - 1)*@PageSize + 1 AND @Page * @PageSize);";

            var parameters = new
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                SupplierID = supplierID,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return await connection.QueryAsync<Product>(sql, parameters);
        }

        /// <summary>
        /// Đếm số lượng
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(string searchValue = "", int categoryID = 0, int supplierID = 0,
                                  decimal minPrice = 0, decimal maxPrice = 0)
        {
            using var connection = await OpenConnectionAsync();

            searchValue = $"%{searchValue}%";

            var sql = @"
        SELECT COUNT(*)
        FROM Products
        WHERE (ProductName LIKE @SearchValue)
          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
          AND (@SupplierID = 0 OR SupplierId = @SupplierID)
          AND (Price >= @MinPrice)
          AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
    ";

            var parameters = new
            {
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                SupplierID = supplierID,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }

        /// <summary>
        /// Lấy thông tin của một sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<DomainModels.Product?> GetAsync(int productId)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Select * From Products Where ProductID = @productId;";
            var parameters = new
            {
                productId
            };
            return await connection.QueryFirstOrDefaultAsync<DomainModels.Product>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Thêm một sản phẩm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(DomainModels.Product data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        INSERT INTO Products
            (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
        VALUES
            (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);

        SELECT SCOPE_IDENTITY();
    ";

            var parameters = new
            {
                data.ProductName,
                data.ProductDescription,
                data.SupplierID,
                data.CategoryID,
                data.Unit,
                data.Price,
                data.Photo,
                data.IsSelling
            };

            var productId = await connection.ExecuteScalarAsync<decimal>(sql, parameters);
            return (int)productId;
        }
        public async Task<bool> UpdateAsync(DomainModels.Product data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        UPDATE Products
        SET 
            ProductName = @ProductName,
            ProductDescription = @ProductDescription,
            SupplierID = @SupplierID,
            CategoryID = @CategoryID,
            Unit = @Unit,
            Price = @Price,
            Photo = @Photo,
            IsSelling = @IsSelling
        WHERE ProductID = @ProductID;
    ";

            var parameters = new
            {
                data.ProductID,
                data.ProductName,
                data.ProductDescription,
                data.SupplierID,
                data.CategoryID,
                data.Unit,
                data.Price,
                data.Photo,
                data.IsSelling
            };

            var rows = await connection.ExecuteAsync(sql, parameters);
            return rows > 0;
        }
        public async Task<bool> DeleteAsync(int productId)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        DELETE FROM Products
        WHERE ProductID = @ProductID;
    ";

            var parameters = new
            {
                ProductID = productId
            };

            var rows = await connection.ExecuteAsync(sql, parameters);
            return rows > 0;
        }
        public async Task<bool> InUsedAsync(int productId)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM OrderDetails WHERE ProductID = @ProductID) 
            THEN 1 ELSE 0 END;
    ";  

            var parameters = new
            {
                ProductID = productId
            };

            var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
            return result == 1;
        }
        /// <summary>
        /// Lấy danh sách thuộc tính của mặt hàng
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProductAttribute>> ListAttributesAsync(int productId)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
        FROM ProductAttributes
        WHERE ProductID = @productId
        ORDER BY DisplayOrder ASC";

            return await connection.QueryAsync<ProductAttribute>(sql, new { productId });
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy thông tin một thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ProductAttribute?> GetAttributeAsync(long AttributeID)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
        FROM ProductAttributes
        WHERE AttributeID = @AttributeID";

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { AttributeID });
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Add atribute for product
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
        VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);

        SELECT CAST(SCOPE_IDENTITY() AS bigint);
    ";

            return await connection.ExecuteScalarAsync<long>(sql, data);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Cập nhật thuộc tính của mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        UPDATE ProductAttributes
        SET 
            ProductID = @ProductID,
            AttributeName = @AttributeName,
            AttributeValue = @AttributeValue,
            DisplayOrder = @DisplayOrder
        WHERE AttributeID = @AttributeID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Xóa thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = await OpenConnectionAsync();
            //                                              .result
            string sql = @"
        DELETE FROM ProductAttributes 
        WHERE AttributeID = @attributeID";

            int rows = connection.Execute(sql, new { attributeID });
            return rows > 0;
           // throw new NotImplementedException();
        }
        /// <summary>
        /// Danh sách ảnh của mặt hàng
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProductPhoto>> ListPhotosAsync(int productId)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        SELECT *
        FROM ProductPhotos
        WHERE ProductID = @ProductId 
        ORDER BY DisplayOrder ASC";

            return await connection.QueryAsync<ProductPhoto>(sql, new { productId });
        }
        /// <summary>
        /// Lấy thông tin một ảnh của mặt hàng
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public async Task<ProductPhoto?> GetPhotoAsync(long photoId)
        {
            using var connection = await OpenConnectionAsync();     //PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden
            string sql = @" 
        SELECT *                                                                                    
        FROM ProductPhotos
        WHERE PhotoID = @photoId";

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoId });
        }
        /// <summary>
        /// Thêm ảnh cho mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
        VALUES (@ProductId, @Photo, @Description, @DisplayOrder, @IsHidden);

        SELECT CAST(SCOPE_IDENTITY() AS bigint);
    ";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }
        /// <summary>
        /// Cập nhật ảnh của mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        UPDATE ProductPhotos
        SET 
            ProductID = @ProductId,
            Photo = @Photo,
            Description = @Description,
            DisplayOrder = @DisplayOrder,
            IsHidden = @IsHidden
        WHERE PhotoID = @PhotoId";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }
        /// <summary>
        /// Xóa ảnh của mặt hàng
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePhotoAsync(long photoId)
        {
            using var connection = await OpenConnectionAsync();
            string sql = @"
        DELETE FROM ProductPhotos
        WHERE PhotoID = @photoId";

            int rows = await connection.ExecuteAsync(sql, new { photoId });
            return rows > 0;
        }

        public async Task<bool> ChangePhotoAsync(int productId, string newPhoto)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        UPDATE Products
        SET Photo = @Photo
        WHERE ProductID = @ProductID;
    ";

            var parameters = new
            {
                ProductID = productId,
                Photo = newPhoto
            };

            var rows = await connection.ExecuteAsync(sql, parameters);
            return rows > 0;
        }
        /// <summary>
        /// Lấy giá nhỏ nhất trong bảng Products
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetMinPriceAsync()
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT ISNULL(MIN(Price), 0) FROM Products;";
            return await connection.ExecuteScalarAsync<decimal>(sql, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Lấy giá lớn nhất trong bảng Products
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetMaxPriceAsync()
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT ISNULL(MAX(Price), 0) FROM Products;";
            return await connection.ExecuteScalarAsync<decimal>(sql, commandType: System.Data.CommandType.Text);
        }
        public async Task<IEnumerable<Product>> TopProductsByPriceAsync(int top = 4)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        SELECT TOP (@Top) *
        FROM Products
        ORDER BY Price DESC;";          // TOP N + ORDER BY [web:49]

            return await connection.QueryAsync<Product>(sql, new { Top = top });
        }

    }
}
