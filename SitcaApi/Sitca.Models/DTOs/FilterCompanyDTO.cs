namespace Sitca.Models.DTOs;

/// <summary>
/// Representa los criterios de filtrado para el reporte de empresas.
/// </summary>
/// <param name="Country">ID del país a filtrar. 0 para todos los países.</param>
/// <param name="Tipologia">ID de la tipología a filtrar. Null para todas.</param>
/// <param name="Estado">Estado de la empresa a filtrar. -1 para todos.</param>
/// <param name="Meses">Número de meses para filtrar. Null para no aplicar filtro.</param>
/// <param name="Certificacion">Estado de certificación a filtrar.</param>
/// <param name="Homologacion">Estado de homologación a filtrar.</param>
/// <param name="Lang">Idioma para las respuestas ("es" o "en").</param>
/// <param name="Activo">Estado de activación a filtrar.</param>
public record FilterCompanyDTO(
    string? Name = null,
    int? Country = null,
    int? Tipologia = null,
    int? Estado = null,
    int? Meses = null,
    string? Certificacion = null,
    string? Homologacion = null,
    string? Lang = "es",
    bool? Activo = null
)
{
    // Factory method para crear una instancia por defecto
    public static FilterCompanyDTO Default => new();

    // Método para crear una copia con país específico
    public FilterCompanyDTO WithCountry(int? newCountry) => this with { Country = newCountry };

    // Validar el filtro
    public bool IsValid()
    {
        if (Country < 0)
            return false;
        if (Tipologia < 0)
            return false;
        if (Meses < 0)
            return false;
        if (!string.IsNullOrEmpty(Lang) && Lang != "es" && Lang != "en")
            return false;

        return true;
    }
}
