using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    /// <summary>
    /// Mặt hàng
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string ProductName { get; set; } = "";
        /// <summary>
        /// Mô tả mặt hàng
        /// </summary>
        public string ProductDescription { get; set; } ="";
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// Mã loại hàng
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// Đơn vị tính (gói, hộp, lon, chai, kg, thùng, ...)
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Photo URL
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Có đang được bán hay không  
        /// </summary>
        public bool IsSelling { get; set; }

    }
}
