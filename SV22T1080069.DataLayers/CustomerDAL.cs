using Dapper;
using SV22T1080069.DomainModels;
using System.Data;

namespace SV22T1080069.DataLayers
{
    public class CustomerDAL : BaseDAL
    {
        /// <summary>
        /// Ctor lớp truy cập dữ liệu bảng Khách hàng
        /// </summary>
        /// <Param name="connectionString"></Param>
        public CustomerDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trong mỗi trang (nếu pageSize = 0 thì không phân trang)</param>
        /// <param name="searchValue">Tên khách hàng cần tìm (rỗng nếu lấy toàn bộ)</param>
        /// <returns></returns>
        public async Task<IEnumerable<Customer>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")     // IEnumerable<Customer> trả về danh sách các Customer
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";                            //"%" + searchValue + "%"; 

            using (var connection = await OpenConnectionAsync())
            {
                //var sql = "SELECT * FROM Customers ORDER BY CustomerName";
                var sql = @"WITH cte AS
                                (
                                    SELECT    *,
                                            ROW_NUMBER() OVER(ORDER BY CustomerName) AS RowNumber
                                    FROM    Customers
                                    WHERE   CustomerName LIKE @searchValue OR ContactName LIKE @searchValue
                                )
                                SELECT * FROM cte
                                WHERE   (@PageSize = 0) OR
                                        (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                                ORDER BY RowNumber;";
                var parameters = new
                {
                    page = page,                        // trước là tên tham số, sau là giá trị truyền vào, tên tham số không @ (trong sql thôi)
                    pageSize = pageSize,                // tên tham số phải trùng trong sql
                    searchValue                         // viết gọn khi tên tham số và tên biến trùng nhau
                };

                // Thực thi câu lệnh SQL
                return await connection.QueryAsync<Customer>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                //return await connection.QueryAsync<Customer>(sql);      // <Customer> là kiểu dữ liệu trả về
            }
        }
        /// <summary>
        /// Đếm số khách hàng tìm được
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(String searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT COUNT(*) FROM Customers WHERE CustomerName LIKE @searchValue OR 
                                     ContactName LIKE @searchValue";
            var parameters = new { searchValue };
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Lấy thông tin của một khách hàng dựa vào mã khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = await OpenConnectionAsync();
            var sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var parameters = new { id };
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql: sql, param: parameters, commandType: CommandType.Text);
        }
        /// <summary>
        /// Thêm một khách hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Customer data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Customers 
                            (
                                CustomerName, ContactName, Province, Address, Phone, Email, IsLocked
                            )
                            VALUES(
                                @CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked
                            );
                            SELECT SCOPE_IDENTITY();";                              // trả về ID mới thêm vào
                //var parameters = new
                //{
                //    CustomerName = data.CustomerName ?? "",                               // 
                //    ContactName = data.ContactName ?? "",
                //    Province = data.Province ?? "",
                //    Address = data.Address ?? "",
                //    Phone = data.Phone ?? "",
                //    Email = data.Email ?? "",
                //    IsLocked = data.IsLocked
                //};
                // var paremeters = new (data) --> viết gọn khi tên biến và tên thuộc tính trùng nhau, nhưng chỉ áp dụng khi tất cả các thuộc tính đều cần dùng
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Cập nhật thông tin của một khách hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"UPDATE  Customers SET
                                CustomerName = @CustomerName,
                                ContactName = @ContactName,
                                Province = @Province,
                                Address = @Address,
                                Phone = @Phone,
                                Email = @Email,
                                IsLocked = @IsLocked
                        WHERE CustomerID = @CustomerID";
            //var parameters = new 
            //{ 
            //    CustomerName = data.CustomerName ?? "",
            //    ContactName = data.ContactName ?? "",
            //    Province = data.Province ?? "",
            //    Address = data.Address ?? "",
            //    Phone = data.Phone ?? "",
            //    Email = data.Email ?? "",
            //    IsLocked = data.IsLocked, 
            //    CustomerID = data.CustomerID
            //};
            return await connection.ExecuteAsync(sql: sql, param: data, commandType: CommandType.Text) > 0;       // trả về true nếu có ít nhất 1 dòng bị ảnh hưởng
        }
        /// <summary>
        /// Xóa một khách hàng dựa vào mã khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            using (var tran = connection.BeginTransaction())
            {
                try
                {
                    // 1. Xóa các giỏ hàng của khách
                    var sqlDeleteCarts = "DELETE FROM Carts WHERE CustomerID = @id";
                    await connection.ExecuteAsync(sqlDeleteCarts, new { id }, tran,
                                                  commandType: CommandType.Text);

                    // 2. Xóa khách hàng
                    var sqlDeleteCustomer = "DELETE FROM Customers WHERE CustomerID = @id";
                    var rows = await connection.ExecuteAsync(sqlDeleteCustomer, new { id }, tran,
                                                             commandType: CommandType.Text);

                    tran.Commit();
                    return rows > 0;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem khách hàng có đang được sử dụng hay không, đang có dữ liệu liên quan hay không 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> InUsed( int id)                 // thiếu async
        {
            using (var connection = await OpenConnectionAsync())
            {
                //var sql = "SELECT CASE WHEN EXISTS(SELECT * FROM Orders WHERE CustomerID = @id) THEN 1 ELSE 0 END";
                var sql = @"IF EXISTS(SELECT * FROM Orders WHERE CustomerID = @id)
                                      SELECT 1 
                                      ELSE SELECT 0;";
                var parameters = new { id };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }
        public async Task<int> CountAllAsync()
        {
            using var connection = await OpenConnectionAsync();
            var sql = "select count(*) from Customers";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

    }
}
