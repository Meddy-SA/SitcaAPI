using FluentValidation;
using Sitca.Models.DTOs;

namespace Sitca.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDTO>
{
  public LoginDtoValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
        .WithMessage(x =>
            x.Language == "en"
              ? "Email is required"
              : "Email es obligatorio")
      .EmailAddress()
        .WithMessage(x =>
            x.Language == "en"
              ? "A valid email is required"
              : "Ingrese un correo electrónico válido");

    RuleFor(x => x.Password)
      .NotEmpty()
        .WithMessage(x =>
            x.Language == "en"
              ? "Password is required"
              : "Contraseña es obligatoria")
      .MinimumLength(8)
        .WithMessage(x =>
            x.Language == "en"
              ? "Length must be at least 8 characters"
              : "Debe tener al menos 8 caracteres");

    RuleFor(x => x.Language)
      .Must(x => string.IsNullOrEmpty(x) || x == "en" || x == "es")
      .WithMessage("Language must be either 'en' or 'es' if specified");
  }
}
