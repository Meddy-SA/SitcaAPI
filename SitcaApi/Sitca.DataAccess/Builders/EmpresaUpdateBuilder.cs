using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sitca.Models;
using Sitca.Models.ViewModels;
using Utilities;

namespace Sitca.DataAccess.Builders;

public class EmpresaUpdateBuilder
{
  private readonly EmpresaUpdateVm _vm;
  private readonly Empresa _empresa;
  private readonly ApplicationUser _user;

  public EmpresaUpdateBuilder(Empresa empresa, ApplicationUser user)
  {
    _empresa = empresa;
    _user = user;
    _vm = new EmpresaUpdateVm
    {
      Language = user.Lenguage,
      MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture(user.Lenguage))
    };
  }

  public EmpresaUpdateBuilder WithBasicInfo()
  {
    _vm.Id = _empresa.Id;
    _vm.Nombre = _empresa.Nombre;
    _vm.ResultadoSugerido = _empresa.ResultadoSugerido;
    _vm.Estado = _empresa.Estado ?? 0;
    return this;
  }

  public EmpresaUpdateBuilder WithLocation()
  {
    _vm.Pais = new CommonVm
    {
      id = _empresa.Pais.Id,
      name = _empresa.Pais.Name
    };
    _vm.Ciudad = _empresa.Ciudad;
    _vm.Direccion = _empresa.Direccion;
    return this;
  }

  public EmpresaUpdateBuilder WithContactInfo()
  {
    _vm.CargoRepresentante = _empresa.CargoRepresentante;
    _vm.Telefono = _empresa.Telefono;
    _vm.Responsable = _empresa.NombreRepresentante;
    _vm.Website = _empresa.WebSite;
    _vm.Email = _empresa.Email;
    _vm.IdNacionalRepresentante = _empresa.IdNacional;
    return this;
  }

  public EmpresaUpdateBuilder WithTipologias(List<Tipologia> allTipologias)
  {
    _vm.Tipologias = allTipologias
      .Select(x =>
        new CommonVm
        {
          name = _user.Lenguage == "es" ? x.Name : x.NameEnglish,
          id = x.Id,
          isSelected = _empresa.Tipologias
            .Any(z =>
                z.IdTipologia == x.Id)
        }).ToList();
    return this;
  }

  public EmpresaUpdateBuilder WithArchivos()
  {
    _vm.Archivos = _empresa.Archivos?
        .Where(s => s.Activo)
        .Select(z => new ArchivoVm
        {
          Id = z.Id,
          Nombre = z.Nombre,
          Ruta = z.Ruta,
          Tipo = z.Tipo,
          Cargador = $"{z.UsuarioCarga.FirstName} {z.UsuarioCarga.LastName}",
          FechaCarga = z.FechaCarga.ToUtc(),
          Propio = z.UsuarioCargaId == _user.Id
        }).ToList();
    return this;
  }

  public EmpresaUpdateVm Build() => _vm;
}
