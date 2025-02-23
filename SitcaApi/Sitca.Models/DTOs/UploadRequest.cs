using Microsoft.AspNetCore.Http;
using Sitca.Models.Enums;

namespace Sitca.Models.DTOs;

public class UploadRequest
{
    public IFormFile File { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public bool IsCompanyFile { get; set; }
    public FileCompany FileType { get; set; }
    public string DocumentType { get; set; } = null!;
    public int EmpresaId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public IList<string> Roles { get; set; } = [];
    public string? idPregunta { get; set; }
    public string? idRespuesta { get; set; }
}
