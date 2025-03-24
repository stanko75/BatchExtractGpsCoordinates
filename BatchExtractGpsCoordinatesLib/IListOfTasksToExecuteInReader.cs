using BatchExtractGpsCoordinatesLib.gps;

namespace BatchExtractGpsCoordinatesLib;

public interface IListOfTasksToExecuteInReaderCommand
{
    LatLngFileNameModel? LatLngFileName { get; set; }
}

public interface IListOfTasksToExecuteInReader
{
    public Task Execute(IListOfTasksToExecuteInReaderCommand command);
}