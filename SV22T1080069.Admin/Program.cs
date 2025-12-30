// đây là hàm main 
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using SV22T1080069.Admin;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();              // đăng ký dịch vụ truy cập HttpContext, lấy đường dẫn vật lý wwwroot
builder.Services.AddControllersWithViews()
                .AddMvcOptions(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });
// Cấu hình xác thực sử dụng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1080069.Admin";
                    option.LoginPath = "/Account/Login";                     // đường dẫn đến trang đăng nhập 
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(30);
                    option.SlidingExpiration = true;
                });
builder.Services.AddDistributedMemoryCache(); // Cần thiết cho Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // Đặt thời gian, FromMinutes(60)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")  // định nghĩa quy tắc Route , tham số có "?" có cũng được không cũng được 
    .WithStaticAssets();
// /{id?}/{subid?} 

// cấu hình định dạng dữ liệu 
var cultureInfo = new CultureInfo("vi-VN");
//var localizationOptions = new RequestLocalizationOptions
//{
//    DefaultRequestCulture = new RequestCulture(cultureInfo),
//    SupportedCultures = new[] { cultureInfo },
//    SupportedUICultures = new[] { cultureInfo }
//};

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// khỏi tạo cấu hình cho ApplicationContext
ApplicationContext.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>(),
    app.Services.GetRequiredService<IWebHostEnvironment>(),
    app.Configuration);

// Khởi tạo cấu hình cho tầng tác nghiệp - Business Layers
//string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB") ?? throw new Exception("ConnectionString error");
string connectionString = builder.Configuration.GetConnectionString("EcommerceTest") ?? throw new Exception("ConnectionString error");
SV22T1080069.BusinessLayers.Configuration.Initialize(connectionString);
app.Run();
