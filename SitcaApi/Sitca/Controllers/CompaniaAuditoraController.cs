using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Sitca.Extensions;

namespace Sitca.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CompaniaAuditoraController : ControllerBase
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;


    public CompaniaAuditoraController(
        IUnitOfWork unitOfWork,
        IConfiguration config,
        UserManager<ApplicationUser> userManager
    )
    {
      _unitOfWork = unitOfWork;
      _config = config;
      _userManager = userManager;
    }

    [HttpGet]
    [Authorize]
    [Route("Get")]
    public IActionResult Get(int? idEmpresa)
    {
      var res = _unitOfWork.CompañiasAuditoras.Get(idEmpresa ?? 0);

      return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [HttpGet]
    [Authorize]
    [Route("GetPersonal")]
    public async Task<IActionResult> GetPersonal(int? idPais, int idEmpresa)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (!User.IsInRole("Admin"))
      {
        idPais = appUser.PaisId ?? 0;
      }

      var res = await _unitOfWork.Users.GetPersonal(idPais ?? 0, idEmpresa);

      return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [HttpGet]
    [Authorize]
    [Route("List")]
    public async Task<IActionResult> List(int? idPais, bool special = true)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (!User.IsInRole("Admin"))
      {
        idPais = appUser.PaisId ?? 0;
      }

      var res = _unitOfWork.CompañiasAuditoras.GetAll(x => x.PaisId == idPais || idPais == 0, null, "Pais");

      if (!special)
      {
        //quitar las compañias auditoras especiales -institucional, -independiente
        res = res.Where(x => !x.Special);
      }

      return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [HttpPost]
    [Authorize]
    [Route("Save")]
    public async Task<IActionResult> Save(CompAuditoras data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (!User.IsInRole("Admin"))
      {
        data.PaisId = appUser.PaisId ?? 0;
      }

      var res = await _unitOfWork.CompañiasAuditoras.SaveAsync(data);

      return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }
  }
}
