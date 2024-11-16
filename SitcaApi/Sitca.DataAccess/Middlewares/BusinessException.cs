using System;

namespace Sitca.DataAccess.Middlewares;

public class BusinessException : Exception
{
  public BusinessException(string message, Exception innerException)
      : base(message, innerException) { }
}
