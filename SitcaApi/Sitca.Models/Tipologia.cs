using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class Tipologia
  {
    [Key]
    public int Id { get; set; }

    [StringLength(75)]
    public string Name { get; set; } = null!;
    [StringLength(75)]
    public string NameEnglish { get; set; } = null!;

    [Required]
    public bool Active { get; set; }

    public ICollection<TipologiasEmpresa> Empresas { get; set; } = [];
    public ICollection<Cuestionario> Cuestionarios { get; set; } = [];
    public ICollection<Modulo> Modulos { get; set; } = [];
  }

  /* 
   * Alojamiento
   * Restaurantes
   * Operadoras de Turismo
   * Empresas de Transporte y Rent a car
   * Actividades Temáticas
   */
}
