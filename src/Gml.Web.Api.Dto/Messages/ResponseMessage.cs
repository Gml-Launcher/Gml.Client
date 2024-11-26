using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;

namespace Gml.Web.Api.Dto.Messages;

public class ResponseMessage
{
    public string Status { get; set; } = null!;
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public static ResponseMessage Create(string message, HttpStatusCode statusCode)
    {
        return new ResponseMessage
        {
            Message = message,
            Status = statusCode.ToString(),
            StatusCode = (int)statusCode
        };
    }

    public static ResponseMessage<T> Create<T>(T content, string? message, HttpStatusCode statusCode)
    {
        return new ResponseMessage<T>
        {
            Message = message ?? string.Empty,
            Status = statusCode.ToString(),
            StatusCode = (int)statusCode,
            Data = content
        };
    }

    public static object? Create(List<ValidationFailure> resultErrors, string message, HttpStatusCode statusCode)
    {
        return new ResponseMessage
        {
            Message = message,
            Status = statusCode.ToString(),
            StatusCode = (int)statusCode,
            Errors = resultErrors.Select(c => c.ErrorMessage)
        };
    }
}

public class ResponseMessage<T> : ResponseMessage
{
    public T? Data { get; set; }
}
