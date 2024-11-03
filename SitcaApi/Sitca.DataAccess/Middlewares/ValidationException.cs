using System;

namespace Sitca.DataAccess.Middlewares;

public class ValidationException : Exception
{
  public ValidationException(string message) : base(message) { }
}
