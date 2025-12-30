using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using SV22T1080069.Shop;
using SV22T1080069.Shop.Models;

[Authorize]
public class CartController : Controller
{
    private bool TryGetCustomerId(out int customerId)
    {
        customerId = 0;

        var user = User.GetUserData();    // hàm extension bạn đang dùng trong Account
        if (user == null) return false;

        return int.TryParse(user.UserId, out customerId);
    }

    public async Task<IActionResult> Add(int id, int quantity = 1)
    {
        if (!TryGetCustomerId(out var customerId))
            return RedirectToAction("Login", "Account");

        var product = await ProductDataService.ProductDB.GetAsync(id);
        if (product == null) return NotFound();

        await CartDataService.AddToCartAsync(customerId, id, quantity, product.Price);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        if (!TryGetCustomerId(out var customerId))
            return RedirectToAction("Login", "Account");

        var itemsDb = await CartDataService.ListCartAsync(customerId);

        var model = itemsDb.Select(x => new CartItem
        {
            CartItemID = x.CartItemID,
            ProductID = x.ProductID,
            ProductName = x.ProductName,
            Photo = x.Photo,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        return View(model);                  // View dùng List<CartItem>
    }
    /*[HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        await CartDataService.UpdateQuantityAsync(cartItemId, quantity);
        return RedirectToAction("Index");
    }
    }*/
    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        await CartDataService.UpdateQuantityAsync(cartItemId, quantity);

        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var itemsDb = await CartDataService.ListCartAsync(customerId);

        var model = itemsDb.Select(x => new CartItem
        {
            CartItemID = x.CartItemID,
            ProductID = x.ProductID,
            ProductName = x.ProductName,
            Photo = x.Photo,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        return PartialView("_CartTable", model); // _CartTable dùng List<CartItem>
    }
    /*[HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        await CartDataService.UpdateQuantityAsync(cartItemId, quantity);

        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var itemsDb = await CartDataService.ListCartAsync(customerId);
        var model = itemsDb.Select(x => new CartItem
        {
            CartItemID = x.CartItemID,
            ProductID = x.ProductID,
            ProductName = x.ProductName,
            Photo = x.Photo,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        var totalQty = model.Sum(x => x.Quantity);

        return Json(new
        {
            html = RenderViewToString("_CartTable", model), // hàm helper render partial thành string
            totalQuantity = totalQty
        });
    }
    */
    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        await CartDataService.RemoveItemAsync(cartItemId);

        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var itemsDb = await CartDataService.ListCartAsync(customerId);

        var model = itemsDb.Select(x => new CartItem
        {
            CartItemID = x.CartItemID,
            ProductID = x.ProductID,
            ProductName = x.ProductName,
            Photo = x.Photo,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        return PartialView("_CartTable", model);   // _CartTable: @model List<CartItem>
    }

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        await CartDataService.ClearCartAsync(customerId);

        var itemsDb = await CartDataService.ListCartAsync(customerId);

        var model = itemsDb.Select(x => new CartItem
        {
            CartItemID = x.CartItemID,
            ProductID = x.ProductID,
            ProductName = x.ProductName,
            Photo = x.Photo,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        return PartialView("_CartTable", model);
    }
    [HttpPost]
    public async Task<IActionResult> AddAjax(int id, int quantity = 1)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var product = await ProductDataService.ProductDB.GetAsync(id);
        if (product == null) return NotFound();

        await CartDataService.AddToCartAsync(customerId, id, quantity, product.Price);

        // Lấy lại tổng số lượng trong giỏ
        var itemsDb = await CartDataService.ListCartAsync(customerId);
        int totalQty = itemsDb.Sum(x => x.Quantity);

        return Json(new { success = true, totalQuantity = totalQty });
    }

}
