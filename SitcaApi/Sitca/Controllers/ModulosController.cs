using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public ModulosController(
            IUnitOfWork unitOfWork,
            IConfiguration config
        )
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

       
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public string Details(int id)
        {
            var res = _unitOfWork.Modulo.Details(id);
            return JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        
        public string Get()
        {
            var res = _unitOfWork.Modulo.GetList(0);
            return JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
    
        }

        [HttpPost("Edit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Modulo data)
        {

            var result = await _unitOfWork.Modulo.Edit(data);
            return Ok(JsonConvert.SerializeObject(result));
        }

        [HttpPost("SavePregunta")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SavePregunta()
        {
            //var result = await _unitOfWork.Pregunta.GetAll();
            return BadRequest();
        }
    }
}
