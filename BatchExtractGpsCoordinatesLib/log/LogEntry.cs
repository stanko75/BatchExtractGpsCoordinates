namespace BatchExtractGpsCoordinatesLib.log;

public struct LogEntry
{
    public LoggingEventType Severity { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    public LogEntry(LoggingEventType severity, string msg, Exception? ex = null)
    {
        if (msg is null) throw new ArgumentNullException("msg");
        if (msg == string.Empty) throw new ArgumentException("empty", "msg");

        Severity = severity;
        Message = msg;
        Exception = ex;
    }
}