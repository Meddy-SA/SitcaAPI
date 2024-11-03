using Microsoft.AspNetCore.Mvc;
using Sitca.DataAccess.Data.Repository.IRepository;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sitca.Controllers
{
  [Route("settings")]
  [ApiController]
  public class ValuesController : ControllerBase
  {
    private readonly IUnitOfWork _unitOfWork;

    public ValuesController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Route("tipologias")]
    public async Task<IActionResult> Tipologias(string lang = "es")
    {
      var result = await _unitOfWork.Tipologias.SelectList(lang);
      return Ok(result);
    }

    [HttpGet]
    [Route("estados")]
    public async Task<IActionResult> Estados(string lang = "es")
    {
      var result = await _unitOfWork.ProcesoCertificacion.GetStatusList(lang);
      return Ok(result);
    }

    [HttpGet]
    [Route("distintivos")]
    public async Task<IActionResult> GetDistintivos(string lang = "es")
    {
      var result = await _unitOfWork.ProcesoCertificacion.GetDistintivos(lang);
      return Ok(result);
    }

    // POST api/<ValuesController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<ValuesController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<ValuesController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
