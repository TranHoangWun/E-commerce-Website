using Dapper;
using SV22T1080069.DomainModels;

namespace SV22T1080069.DataLayers
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu liên quan đến tỉnh thành 
    /// </summary>
    public class ProvinceDAL : BaseDAL
    {
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Lấy danh sách các tỉnh thành
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Province>> ListAsync()
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "SELECT * FROM Provinces ORDER BY ProvinceName";
                return await connection.QueryAsync<Province>(sql);
            }
        }

    }
}
