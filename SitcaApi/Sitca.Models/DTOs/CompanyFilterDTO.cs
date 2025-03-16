namespace Sitca.Models.DTOs;

public class CompanyFilterDTO
{
    public string Name { get; set; } = null!;
    public int CountryId { get; set; }
    public int TypologyId { get; set; }
    public int DistinctiveId { get; set; }
    public int? StatusId { get; set; }
    public bool IsRecetification { get; set; } = false;

    // Paginación por bloques para el servidor
    public int BlockNumber { get; set; } = 1; // Número de bloque (1 = primeros 100, 2 = siguientes 100, etc.)
    public int BlockSize { get; set; } = 100; // Tamaño del bloque (predeterminado: 100)
}
