using LiteCommerce.DataLayers;
using SV22T1080069.DataLayers;
using SV22T1080069.DomainModels;

namespace SV22T1080069.BusinessLayers
{
    public static class UserAccountService
    {
        private static readonly EmployeeUserAccountDAL employeeUserAccountDB;
        private static readonly CustomerUserAccountDAL customerUserAccountDB;

        static UserAccountService()
        {
            employeeUserAccountDB = new EmployeeUserAccountDAL(Configuration.ConnectionString);
            customerUserAccountDB = new CustomerUserAccountDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Giao tiếp với dữ liệu tài khoản của nhân viên
        /// </summary>
        public static EmployeeUserAccountDAL EmployeeUserAccountDB => employeeUserAccountDB;

        /// <summary>
        /// Giao tiếp với dữ liệu tài khoản của khách hàng
        /// </summary>
        public static CustomerUserAccountDAL CustomerUserAccountDB => customerUserAccountDB;
    }

}
