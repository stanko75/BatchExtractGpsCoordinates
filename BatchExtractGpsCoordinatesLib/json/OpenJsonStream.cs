using BatchExtractGpsCoordinatesLib.gps;

namespace BatchExtractGpsCoordinatesLib.json;

public class OpenJsonStreamCommand : IListOfTasksToExecuteBeforeReaderCommand
{
    public string? FileName { get; set; }
    public JsonFileManager<LatLngFileNameModel>? JsonFileManager { get; set; }
}

public class OpenJsonStream : IListOfTasksToExecuteBeforeReader
{
    public Task Execute(IListOfTasksToExecuteBeforeReaderCommand command)
    {
        try
        {
            var openJsonStreamCommand = (OpenJsonStreamCommand)command;
            if (openJsonStreamCommand.FileName != null)
            {
                openJsonStreamCommand.JsonFileManager?.OpenStream(openJsonStreamCommand.FileName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }
}