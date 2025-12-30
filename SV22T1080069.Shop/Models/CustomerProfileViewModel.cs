using SV22T1080069.DomainModels;

namespace SV22T1080069.Shop.Models
{
    public class CustomerProfileViewModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Province { get; set; } = "";
        public string Photo { get; set; } = "";        // tên file avatar
        public IFormFile? UploadPhoto { get; set; }    // file upload từ form
        public List<OrderProfileItemViewModel> Orders { get; set; } = new();
    }

    public class OrderProfileItemViewModel
    {
        public Order Order { get; set; } = new Order();
        public List<OrderDetail> Items { get; set; } = new List<OrderDetail>();
        // Tổng tiền của đơn
        public decimal Total => Items.Sum(x => x.TotalPrice);
        // Đơn được phép hủy nếu đang mới hoặc đã chấp nhận
        public bool CanCancel =>
            Order != null &&
            (Order.Status == Constants.ORDER_INIT ||
             Order.Status == Constants.ORDER_ACCEPTED);
    }
}
