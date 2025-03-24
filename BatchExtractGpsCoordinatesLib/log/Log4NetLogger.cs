using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository;
using System.Reflection;

namespace BatchExtractGpsCoordinatesLib.log;

public class Log4NetLogger : ILogger
{
    private readonly ILog _log;

    public Log4NetLogger(string logFilePath)
    {
        PatternLayout layout = new PatternLayout
        {
            ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
        };
        layout.ActivateOptions();

        RollingFileAppender fileAppender = new RollingFileAppender
        {
            Name = "RollingFileAppender",
            File = logFilePath,
            AppendToFile = true,
            RollingStyle = RollingFileAppender.RollingMode.Size,
            MaxFileSize = 10 * 1024 * 1024, // 10 MB
            MaxSizeRollBackups = 5,
            StaticLogFileName = true,
            Layout = layout,
            LockingModel = new FileAppender.MinimalLock()
        };
        fileAppender.ActivateOptions();

        ILoggerRepository repository = LogManager.GetRepository();
        log4net.Config.BasicConfigurator.Configure(repository, fileAppender);
        _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? throw new InvalidOperationException());
    }

    public void Log(LogEntry entry)
    {
        if (entry.Severity == LoggingEventType.Error)
        {
            _log.Error($"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
        }
        else if (entry.Severity == LoggingEventType.Warning)
        {
            _log.Warn($"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
        }
        else if (entry.Severity == LoggingEventType.Information)
        {
            _log.Info($"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
        }
        else if (entry.Severity == LoggingEventType.Debug)
        {
            _log.Debug($"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
        }
        else
        {
            _log.Debug($"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
        }
    }
}