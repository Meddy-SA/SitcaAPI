using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Extensions;
using Sitca.Models.Constants;
using Sitca.Models.ViewModels;

namespace Sitca.Controllers
{
    [Route("settings")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(IUnitOfWork unitOfWork, ILogger<ValuesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("tipologias")]
        public async Task<ActionResult<List<CommonVm>>> Tipologias(
            string lang = LanguageCodes.Spanish
        )
        {
            try
            {
                var result = await _unitOfWork.Tipologias.SelectList(lang);
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                return this.HandleError(ex, "Tipologias", lang);
            }
        }

        [HttpGet("estados")]
        public async Task<ActionResult<List<CommonVm>>> Estados(string lang = LanguageCodes.Spanish)
        {
            try
            {
                var result = await _unitOfWork.ProcesoCertificacion.GetStatusList(lang);
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                return this.HandleError(ex, "Estados", lang);
            }
        }

        [HttpGet("distintivos")]
        public async Task<ActionResult<List<CommonVm>>> GetDistintivos(
            string lang = LanguageCodes.Spanish
        )
        {
            try
            {
                var result = await _unitOfWork.ProcesoCertificacion.GetDistintivos(lang);
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                return this.HandleError(ex, "Distintivos", lang);
            }
        }

        private ActionResult<List<CommonVm>> HandleError(
            Exception ex,
            string operation,
            string lang
        )
        {
            _logger.LogError(ex, "Error {Operation} with language {Lang}", operation, lang);
            return this.HandleResponse<List<CommonVm>>(
                null,
                false,
                StatusCodes.Status500InternalServerError
            );
        }
    }
}
