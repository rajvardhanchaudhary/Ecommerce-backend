using System.Text.Json.Serialization;

namespace EcommerceWeb.Api.Model.DTO
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        //public object? Data { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }
    }
    public class Enable2FARequestDto
    {
        public string? Issuer { get; set; } = "MyEcommerceApp"; // Optional override
    }

}
