namespace SV22T1080069.Admin.Models
{
    /// <summary>
    /// Đầu vào sử dụng cho các chức năng tìm kiếm có phân trang dữ liệu 
    /// </summary>
    public class PaginationSearchCondition
    {
        /// <summary>
        /// Trang cần hiển thị
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// Số dòng hiển thị trên mỗi trang
        /// </summary>
        public int PageSize { get; set; }
        //public int PageCount { get; set; }
        /// <summary>
        /// Giá trị cần tìm kiếm
        /// </summary>
        public string SearchValue { get; set; } = "";                       //string.Empty;

    }
    /// <summary>
    /// Đầu vào tìm kiếm hàng hóa có phân trang dữ liệu đối với mặt hàng 
    /// </summary>
    public class ProductSearchCondition : PaginationSearchCondition         // kế thừa từ PaginationSearchCondition
    {
        /// <summary>
        /// Mã loại hàng cần tìm
        /// </summary>
        public int CategoryID { get; set; } = 0;
        /// <summary>
        /// Mã nhà cung cấp cần tìm
        /// </summary>
        public int SupplierID { get; set; } = 0;
        /// <summary>
        /// Mã mặt hàng cần tìm
        /// </summary>
        public int ProductID { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
    //public class OrderSearchCondition : PaginationSearchCondition
    //{
    //    public int StatusID { get; set; }
    //    public DateTime? FromTime { get; set; }
    //    public DateTime? ToTime { get; set; }


    //}
    public class OrderSearchCondition : PaginationSearchCondition
    {
        public int StatusID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DateRange { get; set; }


        //TODO: Tìm kiếm theo ngày lập hàng
        //public string DateRange { get; set; } = "";

        //public DateTime FromDate
        //{
        //    get
        //    {
        //        string[] values = DateRange.Split('-');
        //        DateTime d = DateTime.Parse(values[0].Trim());
        //        return d;
        //    }
        //}

        //public DateTime ToDate
        //{
        //    get
        //    {
        //        string[] values = DateRange.Split('-');
        //        DateTime d = DateTime.Parse(values[1].Trim());
        //        return d;
        //    }
        //}
    }
}
