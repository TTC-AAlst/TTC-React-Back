using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ttc.DataAccess.Services;
using Ttc.Model.Core;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/config")]
public class ConfigController
{
    #region Constructor
    private readonly ConfigService _service;
    private readonly TtcLogger _logger;
    private readonly IHubContext<TtcHub, ITtcHub> _hub;

    public ConfigController(ConfigService service, TtcLogger logger, IHubContext<TtcHub, ITtcHub> hub)
    {
        _service = service;
        _logger = logger;
        _hub = hub;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<Dictionary<string, string>> Get()
    {
        return await _service.Get();
    }

    [HttpPost]
    public async Task Post([FromBody] ConfigParam param)
    {
        await _service.Save(param.Key, param.Value);
        await _hub.Clients.All.BroadcastReload(Entities.Config, 0);
    }

    [HttpPost]
    [Route("Log")]
    [AllowAnonymous]
    public void Log([FromBody] ComponentError error)
    {
        string errMsg = error.Message + Environment.NewLine + error.Stack;
        errMsg += Environment.NewLine + Environment.NewLine + "Component Stack:" + Environment.NewLine + error.ComponentStack;
        _logger.Error(errMsg);
    }

    [HttpGet]
    [Route("Log/Get")]
    [AllowAnonymous]
    public string GetLogging()
    {
        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        _logger.Information($"Looking for last log dir in: {logDir}");
        string fileName = Directory
            .GetFiles(logDir, "*.txt")
            .OrderByDescending(x => x)
            .First();

        _logger.Information($"Current log file: {fileName}");
        return File.ReadAllText(fileName);
    }
}

public class ConfigParam
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";

    public override string ToString() => $"{Key} => {Value}";
}

public class ComponentError
{
    public string Message { get; set; } = "";
    public string Stack { get; set; } = "";
    public string ComponentStack { get; set; } = "";

    public override string ToString() => Message;
}
