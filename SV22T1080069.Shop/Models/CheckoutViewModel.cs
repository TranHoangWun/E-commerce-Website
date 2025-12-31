namespace SV22T1080069.Shop.Models
{
    public class CheckoutViewModel
    {
        // Thông tin khách tại thời điểm đặt hàng
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Email { get; set; } = "";      // readonly trên view
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Province { get; set; } = "";

        // Giỏ hàng
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(x => x.Total);

        // Thanh toán
        public string PaymentMethod { get; set; } = "COD"; // COD, ZALOPAY, VNPAY...
    }

}
