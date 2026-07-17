using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using LuongChiHai_QLSV.Server.Services; // IMPORT: Đảm bảo import namespace chứa BusinessException và NotFoundException

namespace LuongChiHai_QLSV.Server.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Định nghĩa cấu trúc lỗi theo chuẩn ProblemDetails mặc định (Lỗi hệ thống 500)
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Title = "Server Error",
                Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                Instance = httpContext.Request.Path
            };

            // Biến cờ kiểm tra xem đây là lỗi nghiệp vụ được kiểm soát (Client Error) hay lỗi hệ thống đột xuất
            bool isClientError = false;

            // 2. Tùy biến Status Code dựa trên loại Exception[cite: 10]
            if (exception is KeyNotFoundException)
            {
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Not Found";
                problemDetails.Detail = exception.Message;
                isClientError = true;
            }
            else if (exception is BusinessException) // BỔ SUNG: Bắt lỗi nghiệp vụ cụ thể[cite: 3]
            {
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation"; // Hoặc "Bad Request"
                problemDetails.Detail = exception.Message;
                isClientError = true;
            }
            else if (exception is UnauthorizedAccessException)
            {
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";
                isClientError = true;

                // Kiểm tra câu thông báo của Exception[cite: 10]
                if (string.IsNullOrEmpty(exception.Message) || exception.Message.Contains("Attempted to perform"))
                {
                    problemDetails.Detail = "Bạn không có quyền truy cập vào tài nguyên này.";
                }
                else
                {
                    problemDetails.Detail = exception.Message;
                }
            }

            // 3. Phân chia cấp độ Ghi Log (Logging) thông minh
            if (isClientError)
            {
                // Lỗi nghiệp vụ do thao tác người dùng: Chỉ ghi Log cảnh báo (LogWarning) để tránh spam log lỗi nặng
                _logger.LogWarning("Cảnh báo nghiệp vụ/yêu cầu không hợp lệ: {Message}", exception.Message);
            }
            else
            {
                // Lỗi sập hệ thống (mất kết nối DB, NullReferenceException,...): Log lỗi chi tiết kèm StackTrace[cite: 10]
                _logger.LogError(exception, "Một lỗi hệ thống nghiêm trọng đã xảy ra: {Message}", exception.Message);
            }

            // 4. Thiết lập HttpContext và trả về định dạng chuẩn RFC7807[cite: 10]
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";
            // Ghi đè dữ liệu trả về cho client dạng JSON[cite: 10]
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}