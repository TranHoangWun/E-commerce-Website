using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    /// <summary>
    /// Ảnh của mặt hàng
    /// </summary>
    public class ProductPhoto
    {
        /// <summary>
        /// Mã ảnh  
        /// </summary>
        public long PhotoId { get; set; } 
        /// <summary>
        /// Mã mặt hàng 
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = ""; 
        /// <summary>
        /// Mô tả ảnh 
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// Display Order - thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// IsHidden - Ảnh có bị ẩn hay không 
        /// </summary>
        public bool IsHidden { get; set; }  


    }
}
