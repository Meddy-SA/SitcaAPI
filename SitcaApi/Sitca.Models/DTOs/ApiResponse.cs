namespace Sitca.Models.DTOs;

public class ApiResponseDto<T>
{
  public bool IsSuccess { get; set; }
  public T? Data { get; set; }
  public string? Message { get; set; }
  public IEnumerable<string>? Errors { get; set; }

  public static ApiResponseDto<T> Success(T data, string? message = null) =>
      new() { IsSuccess = true, Data = data, Message = message };

  public static ApiResponseDto<T> Failure(string message, IEnumerable<string>? errors = null) =>
      new() { IsSuccess = false, Message = message, Errors = errors };
}
