using System.Text.Json.Serialization;

namespace ShopAPI.Response
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public string? CorrelationId { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully.")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailureResponse(string message, List<string> errors = null, string? correlationId = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                CorrelationId = correlationId
            };
        }
    }
}
