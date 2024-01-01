namespace MediatR.Application.Exceptions
{
    public class ApplicationException
    {
        public string Message { get; private set; } = null!;
        public int StatusCode { get; set; }

        public ApplicationException(string message, int statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }
    }
}
