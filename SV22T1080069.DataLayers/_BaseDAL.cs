using Microsoft.Data.SqlClient;

namespace SV22T1080069.DataLayers
{
    /// <summary>
    /// Lớp cơ sở cha cho các lớp xử lý dữ liệu trên CSDL SQL Server 
    /// </summary>
    public abstract class BaseDAL   
    {
        protected string connectionString;
        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="connectionString">Chuỗi tham số kết nối đến CSDL</param>
        public BaseDAL(string connectionString)
        {
            // trùng tên lớp, không có kiểu dữ liệu trả về, không có từ khóa void -> đây là hàm khởi tạo (ctor)
            this.connectionString = connectionString;
        }
        /// <summary>
        /// Mở kết nối đến CSDL SQL Server
        /// </summary>
        /// <returns></returns>
        protected  SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = connectionString;
            connection.Open();
            return connection;
        }
        /// <summary>
        /// Mở kết nối đến CSDL SQL Server bất đồng bộ (asynchronous)
        /// </summary>
        /// <returns></returns>
        protected async Task<SqlConnection> OpenConnectionAsync()       // mở kết nối bất đồng bộ
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = connectionString;
            await connection.OpenAsync();                   // 
            return connection;
        }
    }
}
