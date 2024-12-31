using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models.DTOs;
using Sitca.DataAccess.Middlewares;
using Sitca.Models;
using System;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository;

public class CompañiasAuditorasRepository : Repository<CompAuditoras>, ICompañiasAuditorasRepository
{
    private readonly ApplicationDbContext _db;

    public CompañiasAuditorasRepository(ApplicationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Result<CompAuditoras>> SaveAsync(CompAuditoras data)
    {
        //agregar tipologia
        if (data == null)
        {
            return Result<CompAuditoras>.Failure("Los datos de la compañía auditora no pueden ser nulos.");
        }

        try
        {
            await ValidateCompanyDataAsync(data);

            if (data.Id > 0)
            {
                return await UpdateExistingCompanyAsync(data);
            }

            return await CreateNewCompanyAsync(data);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<CompAuditoras>.Failure("La compañía auditora ha sido modificada por otro usuario. Por favor, refresque los datos e intente nuevamente.");
        }
        catch (Exception ex)
        {
            // Aquí podrías usar ILogger para registrar la excepción
            return Result<CompAuditoras>.Failure($"Error al guardar la compañía auditora: {ex.Message}");
        }
    }

    private async Task ValidateCompanyDataAsync(CompAuditoras data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            throw new ValidationException("El nombre de la compañía es requerido.");

        if (string.IsNullOrWhiteSpace(data.Email) || !IsValidEmail(data.Email))
            throw new ValidationException("El email de la compañía no es válido.");

        var existingCompany = await _db.CompAuditoras
            .AnyAsync(c => c.Email == data.Email && c.Id != data.Id);

        if (existingCompany)
            throw new ValidationException("Ya existe una compañía con este email.");
    }

    private async Task<Result<CompAuditoras>> UpdateExistingCompanyAsync(CompAuditoras data)
    {
        var company = await _db.CompAuditoras
            .FindAsync(data.Id);

        if (company == null)
            return Result<CompAuditoras>.Failure("Compañía auditora no encontrada.");

        // Utilizar AutoMapper aquí sería una mejor opción
        UpdateCompanyProperties(company, data);

        await _db.SaveChangesAsync();
        return Result<CompAuditoras>.Success(data);
    }

    private async Task<Result<CompAuditoras>> CreateNewCompanyAsync(CompAuditoras data)
    {
        var newCompany = new CompAuditoras
        {
            Direccion = data.Direccion,
            Email = data.Email,
            FechaInicioConcesion = data.FechaInicioConcesion,
            FechaFinConcesion = data.FechaFinConcesion,
            Representante = data.Representante,
            NumeroCertificado = data.NumeroCertificado,
            Tipo = data.Tipo,
            Name = data.Name,
            PaisId = data.PaisId,
            Telefono = data.Telefono,
            Status = true
        };

        var addResult = await _db.CompAuditoras.AddAsync(newCompany);
        var result = await _db.SaveChangesAsync();
        newCompany.Id = addResult.Entity.Id;
        return Result<CompAuditoras>.Success(newCompany);
    }

    private void UpdateCompanyProperties(CompAuditoras existing, CompAuditoras updated)
    {
        existing.Name = updated.Name;
        existing.PaisId = updated.PaisId;
        existing.Telefono = updated.Telefono;
        existing.Email = updated.Email;
        existing.Direccion = updated.Direccion;
        existing.FechaFinConcesion = updated.FechaFinConcesion;
        existing.FechaInicioConcesion = updated.FechaInicioConcesion;
        existing.Tipo = updated.Tipo;
        existing.Representante = updated.Representante;
        existing.NumeroCertificado = updated.NumeroCertificado;
        existing.Status = updated.Status;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

