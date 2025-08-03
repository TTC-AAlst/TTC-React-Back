using Serilog;

namespace Ttc.Model.Core;

public class TtcLogger
{
    private readonly ILogger _logger;

    public TtcLogger(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    public void Information(string messageTemplate, object propertyValue)
    {
        _logger.Information(messageTemplate, propertyValue);
    }

    public void Information(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(messageTemplate, propertyValues);
    }

    public void Error(Exception exception, string messageTemplate, object propertyValue)
    {
        _logger.Error(exception, messageTemplate, propertyValue);
    }
}