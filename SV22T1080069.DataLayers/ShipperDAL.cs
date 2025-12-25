using Dapper;
using System.Data;

namespace SV22T1080069.DataLayers
{
    public class ShipperDAL : BaseDAL
    {
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }
        public async Task<IEnumerable<DomainModels.Shipper>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                                (
                                    SELECT    *,
                                            ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
                                    FROM    Shippers
                                    WHERE   ShipperName LIKE @searchValue OR Phone LIKE @searchValue
                                )
                                SELECT * FROM cte
                                WHERE   (@PageSize = 0) OR
                                        (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                                ORDER BY RowNumber;";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue
                };
                return await connection.QueryAsync<DomainModels.Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Shippers
                            WHERE ShipperName LIKE @searchValue OR Phone LIKE @searchValue;";
                var parameters = new
                {
                    searchValue
                };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        public async Task<DomainModels.Shipper> GetAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Shippers
                            WHERE ShipperID = @shipperID;";
                var parameters = new
                {
                    shipperID
                };
                return await connection.QueryFirstOrDefaultAsync<DomainModels.Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        public async Task<int> AddAsync(DomainModels.Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Shippers (ShipperName, Phone)
                            VALUES (@ShipperName, @Phone);
                            SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    data.ShipperName,
                    data.Phone
                };
                var shipperID = await connection.ExecuteScalarAsync<decimal>(sql: sql, param: parameters, commandType: CommandType.Text);
                return (int)shipperID;
            }
        }
        public async Task<bool> UpdateAsync(DomainModels.Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Shippers
                            SET ShipperName = @ShipperName,
                                Phone = @Phone
                            WHERE ShipperID = @ShipperID;";
                var parameters = new
                {
                    data.ShipperID,
                    data.ShipperName,
                    data.Phone
                };
                var rowsAffected = await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        public async Task<bool> DeleteAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Shippers
                            WHERE ShipperID = @shipperID;";
                var parameters = new
                {
                    shipperID
                };
                var rowsAffected = await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        public async Task<bool> InUseAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT CASE WHEN EXISTS
                            (
                                SELECT * FROM Orders
                                WHERE ShipperID = @shipperID
                            )
                            THEN 1 ELSE 0 END;";
                var parameters = new
                {
                    shipperID
                };
                var inUse = await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                return inUse > 0;
            }
        }
    }
}
