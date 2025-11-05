using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AssociationPortal.Data;
using System.Text;
using Microsoft.AspNetCore.Hosting; 

var builder = WebApplication.CreateBuilder(args);

//  Add services to the container
builder.Services.AddControllersWithViews();

//  Thêm DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Lấy JWT settings từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

//  Cấu hình Authentication + JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

//  Thêm Authorization để phân quyền
builder.Services.AddAuthorization();

builder.Services.AddScoped<PermissionService>();

// ĐĂNG KÝ HttpClient
builder.Services.AddHttpClient();

// ĐĂNG KÝ IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

var app = builder.Build();

//  Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//  Kích hoạt Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

//  Các route
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


//app.MapControllers(); // Để test API


app.Run();
