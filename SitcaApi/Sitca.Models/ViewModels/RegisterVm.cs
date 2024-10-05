using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class RegisterVm
    {
        public string email { get; set; }
        public string empresa { get; set; }
        public string password { get; set; }
        public int country { get; set; }
        public string representante { get; set; }
        public string language { get; set; }
        public List<CommonVm> tipologias { get; set; }
    }

    [NotMapped]
    public class RegisterStaffVm
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Country { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public string PhoneNumber { get; set; }

        public string Codigo { get; set; }
        
        public string Direccion { get; set; }
       
        public string NumeroCarnet { get; set; }

        public DateTime VencimientoCarnet { get; set; }
     
        public string FechaIngreso { get; set; }
       
        public string HojaDeVida { get; set; }
  
        public string DocumentoAcreditacion { get; set; }
       
        public string Departamento { get; set; }
   
        public string Ciudad { get; set; }

        
        public string DocumentoIdentidad { get; set; }

        
        public string Profesion { get; set; }

        
        public string Nacionalidad { get; set; }

        public int CompAuditoraId { get; set; }
        public bool Active { get; set; }

        public bool Notificaciones { get; set; }
    }

}
