using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
    public class Pais
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string Name { get; set; } = null!;

        [Required]
        public bool Active { get; set; }

        public ICollection<Empresa> Empresas { get; set; } = [];
        public ICollection<ApplicationUser> Users { get; set; } = [];
    }
}
