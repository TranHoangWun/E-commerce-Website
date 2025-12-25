using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using SV22T1080069.BusinessLayers;

namespace SV22T1080069.Admin
{
    public static class SelectListHelper
    {
        /// <summary>
        /// Danh sách các tỉnh thành dùng cho thẻ select
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<SelectListItem>> Provinces()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Chọn Tỉnh/Thành --" });
            foreach (var item in await CommonDataService.ProvinceDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.ProvinceName, Text = item.ProvinceName });
            }
            return list;
        }
        public static async Task<IEnumerable<SelectListItem>> Categories()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Chọn Loại hàng --" });
            foreach (var item in await CommonDataService.CategoryDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.CategoryID.ToString(), Text = item.CategoryName });
            }
            return list;
        }
        public static async Task<IEnumerable<SelectListItem>> Suppliers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Chọn Nhà cung cấp --" });
            foreach (var item in await CommonDataService.SupplierDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.SupplierID.ToString(), Text = item.SupplierName });
            }
            return list;
        }
        public static async Task<IEnumerable<SelectListItem>> Customers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Chọn Khách hàng --" });
            foreach (var item in await CommonDataService.CustomerDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.CustomerID.ToString(), Text = item.CustomerName });
            }
            return list;
        }
    }
}
