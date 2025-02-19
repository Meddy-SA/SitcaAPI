namespace Sitca.Models.ViewModels;

// Para listar las compañías auditoras
public class CompAuditoraListVm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Pais { get; set; } = null!;
    public int PaisId { get; set; }
    public bool Status { get; set; }
    public string Representante { get; set; } = null!;
    public string NumeroCertificado { get; set; } = null!;
    public int TotalUsuarios { get; set; }
    public string? VencimientoConcesion { get; set; }
    public List<UsersListVm> Usuarios { get; set; } = [];
}

// Para detalles y edición de la compañía auditora
public class CompAuditoraDetailVm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Representante { get; set; } = null!;
    public string NumeroCertificado { get; set; } = null!;
    public string? FechaInicioConcesion { get; set; }
    public string? FechaFinConcesion { get; set; }
    public string Tipo { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public bool Status { get; set; }
    public bool Special { get; set; }
    public CommonVm Pais { get; set; } = null!;
    public List<UsersListVm> Auditores { get; set; } = [];
}

// Para crear una nueva compañía auditora
public class CompAuditoraCreateVm
{
    public string Name { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Representante { get; set; } = null!;
    public string NumeroCertificado { get; set; } = null!;
    public string? FechaInicioConcesion { get; set; }
    public string? FechaFinConcesion { get; set; }
    public string Tipo { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public int PaisId { get; set; }
}

// Para actualizar una compañía auditora
public class CompAuditoraUpdateVm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Representante { get; set; } = null!;
    public string NumeroCertificado { get; set; } = null!;
    public string? FechaInicioConcesion { get; set; }
    public string? FechaFinConcesion { get; set; }
    public string Tipo { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public bool Status { get; set; }
    public bool Special { get; set; }
    public int PaisId { get; set; }
}

// Para mostrar información resumida de la compañía auditora
public class CompAuditoraResumenVm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NumeroCertificado { get; set; } = null!;
    public int AuditoresActivos { get; set; }
    public int AuditoriasEnProceso { get; set; }
    public string Estado { get; set; } = null!;
}
