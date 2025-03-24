using BatchExtractGpsCoordinatesLib.gps;

namespace BatchExtractGpsCoordinatesLib.json;

public class CloseJsonStreamCommand: IListOfTasksToExecuteAfterReaderCommand
{
    public JsonFileManager<LatLngFileNameModel>? JsonFileManager { get; set; }
}

public class CloseJsonStream : IListOfTasksToExecuteAfterReader
{
    public Task Execute(IListOfTasksToExecuteAfterReaderCommand command)
    {
        CloseJsonStreamCommand closeJsonStreamCommand = (CloseJsonStreamCommand)command;
        closeJsonStreamCommand.JsonFileManager?.WriteJson();
        closeJsonStreamCommand.JsonFileManager?.CloseStream();
        return Task.CompletedTask;
    }
}