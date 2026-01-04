    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.DomainModels
{
    public class Cart
    {
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class CartItemDb   // để phân biệt với ViewModel CartItem ở Shop.Models
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";

    }
}
