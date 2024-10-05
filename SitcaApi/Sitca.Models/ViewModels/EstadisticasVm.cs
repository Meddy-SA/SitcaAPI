using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class EstadisticasVm
    {
        public IEnumerable<EstadisticaItemVm>  EmpresasPorPais { get; set; }
        public IEnumerable<EstadisticaItemVm> EmpresasPorTipologia { get; set; }
    }

    [NotMapped]
    public class EmpresasCalificadas
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Name { get; set; }        
        public string FechaDictamen { get; set; }
        public string NumeroDictamen { get; set; }
        public string Distintivo { get; set; }
        public bool Aprobado { get; set; }
        public string Observaciones { get; set; }
        public CommonUserVm Asesor { get; set; }
        public CommonUserVm Auditor { get; set; }

        public CommonVm Tipologia { get; set; }
    }

    [NotMapped]
    public class EstadisticaItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
