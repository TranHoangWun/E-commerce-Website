using SV22T1080069.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080069.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly ProductDAL productDB;
        /// <summary>
        /// Ctor 
        /// </summary>
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// Giao tiếp với dữ liệu mặt hàng
        /// </summary>
        public static ProductDAL ProductDB { get { return productDB; } }        // => 

    }
}
