using FluentValidation;
using Sitca.Models.DTOs;

namespace Sitca.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDTO>
{
  public RegisterDtoValidator()
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
                    : "Ingrese un correo electrónico válido");

    RuleFor(x => x.Password)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password is required"
                    : "Contraseña es obligatoria")
        .MinimumLength(8)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must be at least 8 characters"
                    : "La contraseña debe tener al menos 8 caracteres")
        .Matches("[A-Z]")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must contain at least one uppercase letter"
                    : "La contraseña debe contener al menos una letra mayúscula")
        .Matches("[a-z]")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must contain at least one lowercase letter"
                    : "La contraseña debe contener al menos una letra minúscula")
        .Matches("[0-9]")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must contain at least one number"
                    : "La contraseña debe contener al menos un número")
        .Matches("[^a-zA-Z0-9]")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must contain at least one special character"
                    : "La contraseña debe contener al menos un carácter especial");

    RuleFor(x => x.ConfirmPassword)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Confirm Password is required"
                    : "Confirmar contraseña es obligatorio")
        .Equal(x => x.Password)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Passwords must match"
                    : "Las contraseñas deben coincidir");

    RuleFor(x => x.FirstName)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "First Name is required"
                    : "Nombre es obligatorio")
        .MaximumLength(50)
            .WithMessage(x =>
                x.Language == "en"
                    ? "First Name must not exceed 50 characters"
                    : "El nombre no debe exceder los 50 caracteres");

    RuleFor(x => x.LastName)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Last Name is required"
                    : "Apellido es obligatorio")
        .MaximumLength(50)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Last Name must not exceed 50 characters"
                    : "El apellido no debe exceder los 50 caracteres");

    RuleFor(x => x.PhoneNumber)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Phone Number is required"
                    : "Número de teléfono es obligatorio")
        .Matches(@"^\+?[1-9][0-9]{7,14}$")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Please enter a valid phone number"
                    : "Por favor ingrese un número de teléfono válido");

    RuleFor(x => x.Language)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Language is required"
                    : "Idioma es obligatorio")
        .Must(x => x == "en" || x == "es")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Language must be 'en' or 'es'"
                    : "El idioma debe ser 'en' o 'es'");

    RuleFor(x => x.CountryId)
        .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Country is required"
                    : "País es obligatorio")
        .GreaterThan(0)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Please select a valid country"
                    : "Por favor seleccione un país válido");

    RuleFor(x => x.AcceptTerms)
        .Equal(true)
            .WithMessage(x =>
                x.Language == "en"
                    ? "You must accept the terms and conditions"
                    : "Debe aceptar los términos y condiciones");
  }
}
