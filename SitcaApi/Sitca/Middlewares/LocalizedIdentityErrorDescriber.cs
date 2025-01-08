using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Sitca.Middlewares;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly string _language;

    public LocalizedIdentityErrorDescriber(IHttpContextAccessor httpContextAccessor)
    {
        _language =
            httpContextAccessor
                .HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "Language")
                ?.Value ?? "es";
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = _language switch
            {
                "en" => $"Password must be at least {length} characters",
                "es" => $"La contraseña debe tener al menos {length} caracteres",
                _ => $"La contraseña debe tener al menos {length} caracteres",
            },
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = _language switch
            {
                "en" => "Password must contain a special character",
                "es" => "La contraseña debe contener al menos un caracter especial",
                _ => "La contraseña debe contener al menos un caracter especial",
            },
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _language switch
            {
                "en" => "Password must contain at least one digit ('0'-'9')",
                "es" => "La contraseña debe incluir al menos un dígito ('0'-'9')",
                _ => "La contraseña debe incluir al menos un dígito ('0'-'9')",
            },
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = _language switch
            {
                "en" => $"Email '{email}' is already registered",
                "es" => $"El email '{email}' ya está registrado",
                _ => $"El email '{email}' ya está registrado",
            },
        };
    }

    public override IdentityError InvalidEmail(string email = null)
    {
        return new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = _language switch
            {
                "en" => "Invalid email format",
                "es" => "Formato de email inválido",
                _ => "Formato de email inválido",
            },
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = _language switch
            {
                "en" => $"Username '{userName}' is already taken",
                "es" => $"El nombre de usuario '{userName}' ya está en uso",
                _ => $"El nombre de usuario '{userName}' ya está en uso",
            },
        };
    }
}
