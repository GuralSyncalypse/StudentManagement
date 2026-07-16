using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            // 1. Log lại lỗi chi tiết ở server
            _logger.LogError(exception, "Một lỗi hệ thống đã xảy ra: {Message}", exception.Message);

            // 2. Định nghĩa cấu trúc lỗi theo chuẩn ProblemDetails
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Title = "Server Error",
                Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                Instance = httpContext.Request.Path
            };

            // Tùy biến Status Code dựa trên loại Exception (Ví dụ: KeyNotFoundException -> 404)
            if (exception is KeyNotFoundException)
            {
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Not Found";
                problemDetails.Detail = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";

                // 💡 GIẢI PHÁP: Kiểm tra câu thông báo của Exception
                // Nếu exception không có message tự thiết lập, .NET sẽ tự sinh ra câu mặc định bắt đầu bằng "Attempted to..."
                if (string.IsNullOrEmpty(exception.Message) || exception.Message.Contains("Attempted to perform"))
                {
                    problemDetails.Detail = "Bạn không có quyền truy cập vào tài nguyên này.";
                }
                else
                {
                    problemDetails.Detail = exception.Message;
                }
            }

            // 3. Thiết lập HttpContext
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            // 4. Ghi đè dữ liệu trả về cho client dạng JSON
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Trả về true để báo hiệu rằng lỗi đã được xử lý xong, 
            // không cần chuyển tiếp sang handler khác.
            return true;
        }
    }
}
