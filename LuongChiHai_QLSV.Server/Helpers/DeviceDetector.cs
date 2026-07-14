namespace LuongChiHai_QLSV.Server.Helpers
{
    public class DeviceDetector
    {
        public static string GetDeviceFriendlyName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Thiết bị không xác định";

            string os = "Hệ điều hành ẩn danh";
            string browser = "Trình duyệt ẩn danh";

            // 1. Nhận diện Hệ điều hành (OS)
            if (userAgent.Contains("Windows")) os = "Windows";
            else if (userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS")) os = "macOS";
            else if (userAgent.Contains("iPhone")) os = "iPhone";
            else if (userAgent.Contains("iPad")) os = "iPad";
            else if (userAgent.Contains("Android")) os = "Android";
            else if (userAgent.Contains("Linux")) os = "Linux";

            // 2. Nhận diện Trình duyệt (Browser)
            if (userAgent.Contains("Edg/")) browser = "Edge";
            else if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg/")) browser = "Chrome";
            else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) browser = "Safari";
            else if (userAgent.Contains("Firefox")) browser = "Firefox";

            return $"{browser} ({os})";
        }
    }
}
