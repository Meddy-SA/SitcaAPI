using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AuthenticationRepository> _logger;

    public AuthenticationRepository(
        ApplicationDbContext db,
        ILogger<AuthenticationRepository> logger
    )
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<List<Pais>>> GetCountries()
    {
        try
        {
            var paises = await _db.Pais.AsNoTracking().ToListAsync();

            return Result<List<Pais>>.Success(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el pais");
            return Result<List<Pais>>.Failure("Error al obtener el pais");
        }
    }

    public async Task<Result<Pais>> GetCountry(int paisId)
    {
        try
        {
            var pais = await _db.Pais.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paisId);

            return Result<Pais>.Success(pais);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el pais");
            return Result<Pais>.Failure("Error al obtener el pais");
        }
    }
}
