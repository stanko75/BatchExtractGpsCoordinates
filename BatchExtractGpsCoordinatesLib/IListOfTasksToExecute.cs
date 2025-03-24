using BatchExtractGpsCoordinatesLib.gps;
using System.Threading.Channels;

namespace BatchExtractGpsCoordinatesLib;

public interface IListOfTasksToExecuteCommand
{
    ChannelReader<LatLngFileNameModel>? GpsInfoChannelReader { get; set; }
}

public interface IListOfTasksToExecute
{
    public Task Execute(IListOfTasksToExecuteCommand command);
}