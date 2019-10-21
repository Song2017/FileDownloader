using Common;
using Common.Utils;
using Services.Business;
using Services.Infrastructure;

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using VASD.Resources;
using System.Reflection;

namespace VASD.Controllers
{
    [Route("datacenter")]
    [ApiController]
    public class RepairController : ControllerBase
    {
        private readonly IValveService _valveService;
        private readonly IReportService _reportService;
        private readonly ILogger _logger;
        private readonly ICustomeCache _cache;
        private readonly IStringLocalizer _localizer;

        public RepairController(IValveService valveService, ILoggerFactory logger,
            IReportService reportService, ICustomeCache cache, IStringLocalizerFactory factory)
        {
            _valveService = valveService;
            _reportService = reportService;
            _logger = logger.CreateLogger<RepairController>();
            _cache = cache;

            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("SharedResource", assemblyName.Name);

        }

        /// <summary>
        /// Get valve files name
        /// </summary>
        /// <param name="authentication"></param>
        /// <param name="owner"></param>
        /// <param name="plant"></param>
        /// <param name="valvetype"></param>
        /// <param name="filetype"></param>
        /// <param name="tagnumber"></param>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        [HttpGet("file")]
        [Authorize]
        public ActionResult Get([FromHeader(Name = "Authorization")]string authentication,
            string owner, string plant, string valvetype, string filetype,
            string tagnumber = "", string serialnumber = "")
        {
            // validate filter parameters
            var message = string.Empty;
            var token = authentication.GetToken();

            var filters = new List<string>() {owner, plant, valvetype, filetype, tagnumber, serialnumber};

            var valves = _valveService.ValidateValve(token.GetTokenName(TokenConstants.TokenName),
                filters, ref message);
            if (valves == null)
            {
                _logger.LogInformation($"Get Repair History Failed: {message}-{string.Join(',', filters)}");
                return NotFound(message);
            }

            var file = _cache.GetStringAsync(_cache.GetCacheKey(valves[0]));
            if (file.Result != null)
                return Ok(file.Result);

            // generate valve files
            valves = _valveService.GetValves(valves);
            string fileName;
            switch (message)
            {
                case AppConstants.Xml:
                    fileName = _valveService.CreateValvesFile(valves);
                    break;
                case AppConstants.Docx:
                    fileName = _reportService.CreateWordFiles(valves, _localizer);
                    break;
                default:
                    fileName = _valveService.CreateValvesFile(valves);
                    break;
            }

            if (fileName == null)
                return new ObjectResult("Create Valve Files Failed!") { StatusCode = 500 };

            _cache.SetStringAsync(_cache.GetCacheKey(valves[0]), fileName,
                new DistributedCacheEntryOptions() { AbsoluteExpiration = token.ValidTo });

            return Ok(fileName);
        }

    }
}
