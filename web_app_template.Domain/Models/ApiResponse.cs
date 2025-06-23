using web_app_template.Domain.Enums;

namespace web_app_template.Domain.Models
{
    public class ApiResponse<T>
    {
        public ResponseStatusCodes Message { get; set; }
        public T Data { get; set; }
    }
}
