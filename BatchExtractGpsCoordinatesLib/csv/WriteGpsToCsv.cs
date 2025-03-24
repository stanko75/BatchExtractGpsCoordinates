using System.Text;
using BatchExtractGpsCoordinatesLib.gps;

namespace BatchExtractGpsCoordinatesLib.csv;

public class WriteGpsToCsvCommand : IListOfTasksToExecuteInReaderCommand
{
    public string? FilePath { get; set; }
    public LatLngFileNameModel? LatLngFileName { get; set; }
}

public class WriteGpsToCsv : IListOfTasksToExecuteInReader
{
    public async Task Execute(IListOfTasksToExecuteInReaderCommand command)
    {
        var filePath = ((WriteGpsToCsvCommand)command).FilePath;
        if (filePath != null && command.LatLngFileName != null)
        {
            await using var writer = new StreamWriter(filePath, append: true, Encoding.UTF8);
            if (command.LatLngFileName.FileName is not null && command.LatLngFileName.Latitude is not null && command.LatLngFileName.Longitude is not null)
            {
                string content =
                    $"{command.LatLngFileName.FileName};{command.LatLngFileName.Latitude};{command.LatLngFileName.Longitude}";
                await writer.WriteLineAsync(content);
            }
        }
    }
}