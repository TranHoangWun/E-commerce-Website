using Dapper;
using SV22T1080069.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DataLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến tài khoản của khách hàng
    /// </summary>
    public class CustomerUserAccountDAL : BaseDAL
    {
        public CustomerUserAccountDAL(string connectionString) : base(connectionString)
        {
        }
        public async Task<UserAccount?> AuthenticateCustomerAsync(string email, string password)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"
            SELECT  CustomerID      AS UserID,        -- map vào UserAccount.UserID
                    CustomerName    AS FullName,     -- map vào UserAccount.FullName
                    Email           AS UserName,     -- map vào UserAccount.UserName
                    Email           AS Email,
                    Photo           AS Photo,       -- lấy trực tiếp từ DB
                    'customer'      AS RoleNames
            FROM    Customers
            WHERE   Email = @email AND Password = @password
              AND   IsLocked = 0";                   // tránh đăng nhập tài khoản bị khóa

            var parameters = new { email, password };

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql: sql,
                param: parameters,
                commandType: System.Data.CommandType.Text
            );
        }
        public async Task<bool> ChangePassword(string userID, string oldPassword, string newPassword)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"UPDATE	Customers 
                        SET		Password = @newPassword 
                        WHERE	CustomerID = @userID AND Password = @oldPassword";
            var parameters = new { userID, oldPassword, newPassword };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        public async Task<bool> CreateAsync(string fullName, string email, string phone, string address, string province, string password)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"
        INSERT INTO Customers
            (CustomerName, ContactName, Email, Phone, Address, Province, Password, IsLocked)
        VALUES
            (@fullName, @contactName, @Email, @Phone, @Address, @Province, @Password, 0)";

            var parameters = new
            {
                fullName,
                contactName = fullName,   // gán ContactName = fullName
                email,
                phone,
                address,
                province,
                password
            };

            return (await connection.ExecuteAsync(
                sql: sql,
                param: parameters,
                commandType: CommandType.Text)) > 0;
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            using var connection = await OpenConnectionAsync();

            var sql = @"SELECT  CustomerID,
                    CustomerName,
                    ContactName,
                    Province,
                    Address,
                    Phone,
                    Email,
                    Password,
                    Photo
            FROM    Customers
            WHERE   Email = @email";

            var parameters = new { email };

            return await connection.QueryFirstOrDefaultAsync<Customer>(
                sql: sql,
                param: parameters,
                commandType: CommandType.Text
            );
        }
        public async Task<bool> UpdateProfileAsync(  int customerId,string customerName,string contactName, string phone,string address,
                        string province, string photoFileName)
                            {
                                using var connection = await OpenConnectionAsync();
                                var sql = @"
                    UPDATE Customers
                    SET    CustomerName = @customerName,
                           ContactName  = @contactName,
                           Phone        = @phone,
                           Address      = @address,
                           Province     = @province,
                           Photo        = @photo
                    WHERE  CustomerID   = @customerId";

                                var parameters = new
                                {
                                    customerId,
                                    customerName,
                                    contactName,
                                    phone,
                                    address,
                                    province,
                                    photo = photoFileName
                                };

                                return (await connection.ExecuteAsync(sql, parameters)) > 0;
                            }
    }
}
