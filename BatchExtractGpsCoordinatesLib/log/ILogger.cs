using Microsoft.IdentityModel.Abstractions;

namespace BatchExtractGpsCoordinatesLib.log;

public interface ILogger
{
    void Log(LogEntry entry);
}