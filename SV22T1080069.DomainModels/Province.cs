using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    public class Province            // sử dụng internal để chỉ có thể truy cập trong cùng assembly, sử dụng public để truy cập từ bên ngoài assembly hiện tại là 3 
    {
        /// <summary>
        /// Tên tỉnh thành 
        /// </summary>
        public string ProvinceName { get; set; } = ""; 

    }
}
