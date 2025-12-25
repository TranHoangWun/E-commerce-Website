using SV22T1080069.DataLayers.SQLServer;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DataLayers;

namespace SV22T1080069.BusinessLayers
{
    /// <summary>
    /// Các chức năng tác nghiệp liên quan đến đơn hàng
    /// </summary>
    public class OrderDataService
    {
        private static readonly OrderDAL orderDB;
        /// <summary>
        /// 
        /// </summary>
        static OrderDataService()
        {
            orderDB = new OrderDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        public static OrderDAL OrderDB => orderDB;
    }
    public class ShipperDataService
    {
        private static readonly ShipperDAL shipperDB;
        /// <summary>
        /// 
        /// </summary>
        static ShipperDataService()
        {
            shipperDB = new ShipperDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// 
        /// </summary>
        public static ShipperDAL ShipperDB => shipperDB;
    }
}