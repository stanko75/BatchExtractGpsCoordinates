using BatchExtractGpsCoordinatesLib.gps;

namespace BatchExtractGpsCoordinatesLib.json;

public class WriteGpsToJsonCommand : IListOfTasksToExecuteInReaderCommand
{
    public string? FilePath { get; set; }
    public LatLngFileNameModel? LatLngFileName { get; set; }
    public JsonFileManager<LatLngFileNameModel>? JsonFileManager { get; set; }
}

public class WriteGpsToJson : IListOfTasksToExecuteInReader
{
    public Task Execute(IListOfTasksToExecuteInReaderCommand command)
    {
        WriteGpsToJsonCommand writeGpsToJsonCommand = (WriteGpsToJsonCommand)command;
        var filePath = writeGpsToJsonCommand.FilePath;
        if (filePath != null && command.LatLngFileName != null)
        {
            if (command.LatLngFileName.FileName is not null 
                && command.LatLngFileName.Latitude is not null 
                && command.LatLngFileName.Longitude is not null)
            {
                writeGpsToJsonCommand.JsonFileManager?.AddItem(command.LatLngFileName);
            }
        }

        return Task.CompletedTask;
    }
}