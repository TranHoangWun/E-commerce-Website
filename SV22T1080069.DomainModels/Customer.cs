using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
     /// <summary>
     ///  Khách hàng 
     /// </summary>
    public class Customer
    {
        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int CustomerID { get; set; }         // thuộc tính get: chỉ đọc, set: chỉ ghi
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; } = "";
        /// <summary>
        /// Tên giao dịch
        /// </summary>
        public string ContactName { get; set; } = "";
        /// <summary>
        /// Tên tỉnh thành  
        /// </summary>
        public string Province { get; set; } = "";
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; } = "";
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; } = ""; 
        /// <summary>
        /// Email 
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// Tài khoản khách có bị khóa hay không 
        /// </summary>
        public bool IsLocked    { get; set; }
        // mật khẩu không cần thiết vì không hiển thị ra ngoài
    }
}
