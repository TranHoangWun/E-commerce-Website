using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1080069.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DataLayers
{
    public class CartDAL : BaseDAL
    {
        public CartDAL(string connectionString) : base(connectionString)
        {
        }

        /* =============================
           Lấy CartID theo CustomerID
           ============================= */
        public async Task<int?> GetCartIdAsync(int customerId)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            SELECT CartID
            FROM Carts
            WHERE CustomerID = @customerId";

            var parameters = new
            {
                customerId
            };

            return await connection.ExecuteScalarAsync<int?>(sql, parameters);
        }

        /* =============================
           Tạo Cart mới
           ============================= */
        public async Task<int> CreateCartAsync(int customerId)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            INSERT INTO Carts(CustomerID)
            VALUES (@customerId);
            SELECT SCOPE_IDENTITY();";

            var parameters = new
            {
                customerId
            };

            var id = await connection.ExecuteScalarAsync<decimal>(sql, parameters);
            return (int)id;

        }

        /* =============================
           Lấy danh sách sản phẩm trong giỏ
           ============================= */
        public async Task<IEnumerable<CartItemDb>> GetCartItemsAsync(int customerId)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            SELECT ci.CartItemID,
                   ci.CartID,
                   ci.ProductID,
                   ci.Quantity,
                   ci.UnitPrice,
                    p.ProductName,
                    p.Photo
            FROM   CartItems ci
                   JOIN Carts c ON ci.CartID = c.CartID
                    JOIN Products p ON ci.ProductID = p.ProductID
            WHERE  c.CustomerID = @customerId";

            var parameters = new { customerId };

            return await connection.QueryAsync<CartItemDb>(sql, parameters);
        }

        /* =============================
           Thêm sản phẩm vào giỏ
           (đã có thì tăng số lượng)
           ============================= */
        public async Task AddToCartAsync(
            int customerId,
            int productId,
            int quantity,
            decimal unitPrice)
        {
            using var connection = await OpenConnectionAsync();

            var cartId = await GetCartIdAsync(customerId)
                         ?? await CreateCartAsync(customerId);

            var sql = @"
            MERGE CartItems AS target
            USING (SELECT @cartId AS CartID, @productId AS ProductID) AS source
            ON target.CartID = source.CartID 
               AND target.ProductID = source.ProductID
            WHEN MATCHED THEN
                UPDATE SET Quantity = Quantity + @quantity
            WHEN NOT MATCHED THEN
                INSERT (CartID, ProductID, Quantity, UnitPrice)
                VALUES (@cartId, @productId, @quantity, @unitPrice);";

            var parameters = new
            {
                cartId,
                productId,
                quantity,
                unitPrice
            };

            await connection.ExecuteAsync(sql, parameters);
        }

        /* =============================
           Cập nhật số lượng
           ============================= */
        public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            UPDATE CartItems
            SET Quantity = @quantity
            WHERE CartItemID = @cartItemId";

            var parameters = new
            {
                cartItemId,
                quantity
            };

            int result = await connection.ExecuteAsync(sql, parameters);
            return result > 0;
        }

        /* =============================
           Xóa 1 sản phẩm khỏi giỏ
           ============================= */
        public async Task<bool> RemoveItemAsync(int cartItemId)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            DELETE FROM CartItems
            WHERE CartItemID = @cartItemId";

            var parameters = new
            {
                cartItemId
            };

            int result = await connection.ExecuteAsync(sql, parameters);
            return result > 0;
        }

        /* =============================
           Xóa toàn bộ giỏ hàng
           (sau khi checkout)
           ============================= */
        public async Task ClearCartAsync(int customerId)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            DELETE ci
            FROM CartItems ci
            JOIN Carts c ON ci.CartID = c.CartID
            WHERE c.CustomerID = @customerId";

            var parameters = new
            {
                customerId
            };

            await connection.ExecuteAsync(sql, parameters);
        }
    }

}
