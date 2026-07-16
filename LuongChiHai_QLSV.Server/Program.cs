using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Helpers;
using LuongChiHai_QLSV.Server.Interfaces;
using LuongChiHai_QLSV.Server.Middlewares; // 🔥 THÊM VÀO: Namespace chứa file GlobalExceptionHandler của bạn
using LuongChiHai_QLSV.Server.Security;
using LuongChiHai_QLSV.Server.Services;
using LuongChiHai_QLSV.Server.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy cấu hình từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "Key_Chua_Chay_Mac_Dinh_Sieu_Dai_Cho_Hai_2026_!";

// 2. Đăng ký dịch vụ Authentication dùng JWT
// 1.Đăng ký Dynamic Policy Provider (Dạng Singleton)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// 2. Đăng ký Authorization Handler (Phải là Scoped vì nó gọi vào DbContext)
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<TokenService>();

// ============================================================================
// 🔥 THÊM VÀO Ở ĐÂY: ĐĂNG KÝ DỊCH VỤ BẮT LỖI TẬP TRUNG (BEFORE BUILD)
// ============================================================================
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// ============================================================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // 🔥 XÓA BỎ 5 PHÚT ÂN HẠN MẶC ĐỊNH
        ClockSkew = TimeSpan.Zero
    };

    // ============================================================================
    // 🔥 THÊM CẤU HÌNH BẮT LỖI 401 / 403 CHO JWT TẠI ĐÂY
    // ============================================================================
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Bỏ qua hành vi mặc định (tránh gửi trùng tiêu đề WWW-Authenticate mặc định)
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";

            // Tạo phản hồi Problem Details chuẩn RFC 7807 giống hệt GlobalExceptionHandler
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Title = "Unauthorized",
                Detail = "Yêu cầu không hợp lệ. Bạn cần cung cấp JWT Token hợp lệ để truy cập tài nguyên này.",
                Instance = context.Request.Path
            };

            // Nếu trong quá trình xác thực có lỗi cụ thể (ví dụ: Token hết hạn, Token sai định dạng...)
            if (!string.IsNullOrEmpty(context.ErrorDescription))
            {
                problemDetails.Detail = $"{problemDetails.Detail} Chi tiết lỗi: {context.ErrorDescription}";
            }

            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        },

        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Title = "Forbidden",
                Detail = "Tài khoản của bạn không có quyền truy cập vào tài nguyên này (Forbidden).",
                Instance = context.Request.Path
            };

            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }
    };
});

// Add services to the container.
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 1. Định nghĩa cơ chế bảo mật JWT (Bearer Token)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer' [khoảng trắng] rồi điền JWT Token của bạn.\n\nVí dụ: Bearer eyJhbGciOi..."
    });

    // 2. Áp dụng cấu hình bảo mật vào Swagger để hiển thị ô khóa ở các endpoint
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Phải trùng với tên Định nghĩa ở bước 1
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ============================================================================
// 🔥 THÊM VÀO Ở ĐÂY: SỬ DỤNG EXCEPTION HANDLER MIDDLEWARE (NGAY SAU BUILD)
// Phải đặt ở đầu Pipeline để bắt được tất cả các lỗi xảy ra ở phía sau (Auth, Routing, Controllers...)
// ============================================================================
app.UseExceptionHandler();
// ============================================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();