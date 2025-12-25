using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    /// <summary>
    /// Người vận chuyển 
    /// </summary>
    public class Shipper
    {
        /// <summary>
        /// Mã người vận chuyển
        /// </summary>
        public int ShipperID { get; set; }
        /// <summary>
        /// Tên người vận chuyển
        /// </summary>
        public string ShipperName { get; set; } = "";
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; } = "";
    }
}
