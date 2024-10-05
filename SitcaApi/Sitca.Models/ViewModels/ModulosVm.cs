using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class ModulosVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Transversal { get; set; }
        public string Nomenclatura { get; set; }
        public int Orden { get; set; }
        public string Tipologia { get; set; }

        public List<CuestionarioItemVm> Items { get; set; }

        public ResultadosModuloVm Resultados { get; set; }
    }

    [NotMapped]
    public class ResultadosModuloVm
    {
        public int TotalObligatorias { get; set; }
        public int TotalComplementarias { get; set; }

        public int ObligCumple { get; set; }
        public int ComplementCumple { get; set; }

        public decimal PorcObligCumple { get; set; }

        public decimal PorcComplementCumple { get; set; }

        public string ResultModulo { get; set; }
    }
}
