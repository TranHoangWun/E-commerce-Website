using SV22T1080069.DataLayers;
using SV22T1080069.DomainModels;          // CartItemDb
namespace SV22T1080069.BusinessLayers
{
    public static class CartDataService
    {
        private static readonly CartDAL cartDB;

        static CartDataService()
        {
            cartDB = new CartDAL(Configuration.ConnectionString);
        }
        // 1. Thêm vào giỏ
        public static async Task AddToCartAsync(int customerId, int productId, int quantity, decimal unitPrice)
        {
            await cartDB.AddToCartAsync(customerId, productId, quantity, unitPrice);
        }

        // 2. Lấy giỏ để hiển thị (ViewModel)
        public static async Task<IList<CartItemDb>> ListCartAsync(int customerId)
        {
            var itemsDb = await cartDB.GetCartItemsAsync(customerId);
            return itemsDb.ToList();
        }

        // 3. Cập nhật số lượng
        public static Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
            => cartDB.UpdateQuantityAsync(cartItemId, quantity);

        // 4. Xóa 1 dòng
        public static Task<bool> RemoveItemAsync(int cartItemId)
            => cartDB.RemoveItemAsync(cartItemId);

        // 5. Xóa toàn bộ giỏ
        public static Task ClearCartAsync(int customerId)
            => cartDB.ClearCartAsync(customerId);
    }
}
