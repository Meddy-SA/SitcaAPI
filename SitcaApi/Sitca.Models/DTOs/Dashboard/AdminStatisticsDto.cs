namespace Sitca.Models.DTOs.Dashboard
{
    public class AdminStatisticsDto
    {
        public int TotalEmpresas { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalInProcess { get; set; }
        public int TotalPending { get; set; }
        public List<CountryStatDto> EmpresasPorPais { get; set; } = new();
        public List<TypologyStatDto> EmpresasPorTipologia { get; set; } = new();
    }

    public class CountryStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int CountryId { get; set; }
    }

    public class TypologyStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TipologiaId { get; set; }
    }
}