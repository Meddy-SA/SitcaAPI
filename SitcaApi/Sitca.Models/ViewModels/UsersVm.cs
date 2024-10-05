using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    public class UsersVm
    {
    }

    public class UsersListVm
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PaisId { get; set; }
        public string Rol { get; set; }
        public string Pais { get; set; }
        public string Codigo { get; set; }

        public string Direccion { get; set; }

        public string NumeroCarnet { get; set; }

        public string FechaIngreso { get; set; }

        public string HojaDeVida { get; set; }

        public string DocumentoAcreditacion { get; set; }

        public string Departamento { get; set; }

        public string VencimientoCarnet { get; set; }

        public DateTime AvisoVencimientoCarnet { get; set; }

        public string Ciudad { get; set; }

        public string DocumentoIdentidad { get; set; }


        public string Profesion { get; set; }


        public string Nacionalidad { get; set; }
        public string RutaPdf { get; set; }

        public int? CompAuditoraId { get; set; }
        public bool CanDeactivate { get; set; }
        public bool Active { get; set; }
        public bool Notificaciones { get; set; }
        public string Lang { get; set; }
    }

    public class NotificacionVm
    {
        public Notificacion Data { get; set; }
        public List<UsersListVm> Users { get; set; }
    }

    public class NotificacionSigleVm
    {
        public Notificacion Data { get; set; }
        public UsersListVm User { get; set; }
    }
}
