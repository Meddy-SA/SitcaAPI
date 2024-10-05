using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class CertificacionVm
    {
        public int Id { get; set; }
        public string Asesor { get; set; }

        public int EmpresaId { get; set; }

        public int Empresa { get; set; }
    }

    public class CertificacionStatusVm
    {
        public int CertificacionId { get; set; }
        public string Status { get; set; }
    }

    public class SaveCalificacionVm
    {
        public int idProceso { get; set; }
        public int? distintivoId { get; set; }
        public string Observaciones { get; set; }
        public string Dictamen { get; set; }
        public bool aprobado { get; set; }
    }

    public class CambioAuditor
    {
        public int idProceso { get; set; }
        public string userId { get; set; }
        public bool auditor { get; set; }
        public string motivo { get; set; }
    }

    public class CertificacionDetailsVm
    {
        public int Id { get; set; }

        public EmpresaVm Empresa { get; set; }

        public CommonUserVm Asesor { get; set; }

        public CommonUserVm Auditor { get; set; }

        public CommonUserVm Generador { get; set; }

        public bool Recertificacion { get; set; }
        public string FechaInicio { get; set; }

        public string FechaFin { get; set; }

        public string Status { get; set; }
        public string TipologiaName { get; set; }

        public string Resultado { get; set; }

        public string FechaVencimiento { get; set; }
        public string Expediente { get; set; }

        public bool alertaVencimiento { get; set; }
    }

    [NotMapped]
    public class CuestionarioCreateVm
    {
        public int Id { get; set; }

        public int TipologiaId { get; set; }
        public int EmpresaId { get; set; }
        public int CertificacionId { get; set; }
        public string AsesorId { get; set; }

        public string AuditorId { get; set; }

        public bool Prueba { get; set; }
    }
       
    public class CuestionarioDetailsMinVm
    {
        public int Id { get; set; }

        public CommonVm Tipologia { get; set; }
        public CommonVm Empresa { get; set; }

        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string FechaEvaluacion { get; set; }

        public CommonUserVm Asesor { get; set; }
        public bool Prueba { get; set; }

        public int IdCertificacion { get; set; }
    }

    [NotMapped]
    public class CuestionarioDetailsVm
    {
        public int Id { get; set; }

        public CommonVm Tipologia { get; set; }
        public CommonVm Empresa { get; set; }

        public CommonUserVm Asesor { get; set; }

        public CommonUserVm Auditor { get; set; }
        public bool Prueba { get; set; }

        public List<ModulosVm> Modulos { get; set; }

        public string path { get; set; }
        public string Expediente { get; set; }

        public string FechaFinalizacion { get; set; }

        public bool Recertificacion { get; set; }

        public string Lang { get; set; }

        public int Pais { get; set; }
    }

    [NotMapped]
    public class CuestionarioNoCumpleVm
    {
        public int Id { get; set; }

        public CommonVm Tipologia { get; set; }
        public CommonVm Empresa { get; set; }

        public CommonUserVm Asesor { get; set; }

        public CommonUserVm Auditor { get; set; }
        public bool Prueba { get; set; }
        
        public List<CuestionarioItemVm> Preguntas { get; set; }

        public string path { get; set; }
        public string Expediente { get; set; }

        public string FechaFinalizacion { get; set; }

        public string Lenguage { get; set; }
    }

    [NotMapped]
    public class CuestionarioItemVm
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Nomenclatura { get; set; }

        public int Order { get; set; }
        public string Type { get; set; }

        public int? Result { get; set; }

        public bool NoAplica { get; set; }
        public bool Obligatoria { get; set; }

        public int CuestionarioId { get; set; }
        public int? IdRespuesta { get; set; }
        public List<Archivo> Archivos { get; set; }

        public string Observacion { get; set; }

        public bool TieneArchivos { get; set; }
        public bool TieneObs { get; set; }
    }

    [NotMapped]
    public class HistorialVm
    {
        public string Fecha { get; set; }
        public int Cantidad { get; set; }
        public int Archivos { get; set; }
        public int Porcentaje { get; set; }
        public int PorcentajeAcumulado { get; set; }
    }
}
