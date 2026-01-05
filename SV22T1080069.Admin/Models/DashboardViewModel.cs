using System;
using System.Collections.Generic;

namespace SV22T1080069.Admin.Models
{
    public class DashboardViewModel
    {
        // Tổng số khách hàng
        public int TotalCustomers { get; set; }

        // Tổng số đơn hàng
        public int TotalOrders { get; set; }

        // Doanh thu trong tháng (VND)
        public decimal RevenueThisMonth { get; set; }

        // Doanh thu trong ngày (VND)
        public decimal RevenueToday { get; set; }

        // Đơn hàng chờ xử lý
        public int PendingOrders { get; set; }

        // Dữ liệu vẽ biểu đồ doanh thu theo ngày
        public List<RevenuePoint> RevenueChart { get; set; } = new();
    }

    public class RevenuePoint
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
