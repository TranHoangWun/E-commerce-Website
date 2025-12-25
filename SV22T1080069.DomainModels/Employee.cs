using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    /// <summary>
    /// Nhân viên
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public int EmployeeId { get; set; }
        /// <summary>
        /// Tên nhân viên 
        /// </summary>
        public string FullName { get; set; } = "";
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime BirthDate { get; set; }
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
        /// Ảnh đại diện (URL hoặc tên file)
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Nhân viên có đang làm việc hay không 
        /// </summary>
        public bool IsWorking { get; set; } = true; 
        // Password và Rolename không cần thiết vì không hiển thị ra ngoài
    }
}
