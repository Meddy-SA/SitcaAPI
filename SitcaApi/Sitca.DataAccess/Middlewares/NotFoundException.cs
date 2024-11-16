using System;

namespace Sitca.DataAccess.Middlewares;

public class NotFoundException : Exception
{
  public NotFoundException(string message) : base(message) { }
}
