using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class FiltroEmpresaReporteVm
    {
        public int? country { get; set; }
        public int? tipologia { get; set; }
        public int? estado { get; set; }
        public int? meses { get; set; }
        public string? certificacion { get; set; }
        public string? homologacion { get; set; }
        public string? lang { get; set; }
        public bool? activo { get; set; }
    }
}
