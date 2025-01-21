using System.Linq;
using FluentValidation;
using Sitca.Models.DTOs;

namespace Sitca.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDTO>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(x => x.Language == "en" ? "Email is required" : "Email es obligatorio")
            .EmailAddress()
            .WithMessage(x =>
                x.Language == "en"
                    ? "A valid email is required"
                    : "Ingrese un correo electrónico válido"
            );

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(x =>
                x.Language == "en" ? "Password is required" : "Contraseña es obligatoria"
            )
            .MinimumLength(6)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Password must be at least 6 characters"
                    : "La contraseña debe tener al menos 6 caracteres"
            );
        RuleFor(x => x.Empresa)
            .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Company Name is required"
                    : "El nombre de la empresa es obligatorio"
            )
            .MaximumLength(100)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Company Name must not exceed 50 characters"
                    : "El nombre de la compañia no debe exceder los 50 caracteres"
            );

        RuleFor(x => x.Tipologias)
            .NotNull()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Business types are required"
                    : "Las tipologías son obligatorias"
            )
            .Must(tipologias => tipologias.Any(t => t.isSelected))
            .WithMessage(x =>
                x.Language == "en"
                    ? "Please select at least one business type"
                    : "Por favor seleccione al menos una tipología"
            );

        RuleFor(x => x.Representante)
            .NotEmpty()
            .WithMessage(x =>
                x.Language == "en"
                    ? "Representative name is required"
                    : "El nombre del representante es obligatorio"
            )
            .MaximumLength(100)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Representative name must not exceed 100 characters"
                    : "El nombre del representante no debe exceder los 100 caracteres"
            );

        When(
            x => !string.IsNullOrEmpty(x.PhoneNumber),
            () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .Matches(@"^\+?[1-9][0-9]{7,14}$")
                    .WithMessage(x =>
                        x.Language == "en"
                            ? "Please enter a valid phone number"
                            : "Por favor ingrese un número de teléfono válido"
                    );
            }
        );

        RuleFor(x => x.Language)
            .NotEmpty()
            .WithMessage(x => x.Language == "en" ? "Language is required" : "Idioma es obligatorio")
            .Must(x => x == "en" || x == "es")
            .WithMessage(x =>
                x.Language == "en"
                    ? "Language must be 'en' or 'es'"
                    : "El idioma debe ser 'en' o 'es'"
            );

        RuleFor(x => x.CountryId)
            .NotEmpty()
            .WithMessage(x => x.Language == "en" ? "Country is required" : "País es obligatorio")
            .GreaterThan(0)
            .WithMessage(x =>
                x.Language == "en"
                    ? "Please select a valid country"
                    : "Por favor seleccione un país válido"
            );
    }
}
