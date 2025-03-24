using BatchExtractGpsCoordinatesLib.gps;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BatchExtractGpsCoordinatesLib;

public class ReadAllAsyncHandlerCommand : IListOfTasksToExecuteCommand
{
    public ChannelReader<LatLngFileNameModel>? GpsInfoChannelReader { get; set; }
    public ConcurrentQueue<Exception> Exceptions { get; set; } = new();

}

public class ReadAllAsyncHandler(
    Dictionary<IListOfTasksToExecuteBeforeReader, IListOfTasksToExecuteBeforeReaderCommand> listOfTasksToExecuteBeforeReadAllAsync
    , Dictionary<IListOfTasksToExecuteInReader, IListOfTasksToExecuteInReaderCommand> listOfTasksToExecuteInReadAllAsync
    , Dictionary<IListOfTasksToExecuteAfterReader, IListOfTasksToExecuteAfterReaderCommand> listOfTasksToExecuteAfterReadAllAsync
    )
    : IListOfTasksToExecute
{
    public async Task Execute(IListOfTasksToExecuteCommand command)
    {
        var reader = command.GpsInfoChannelReader;
        if (reader is not null)
        {
            foreach (var taskToExecuteBeforeReader in listOfTasksToExecuteBeforeReadAllAsync)
            {
                await taskToExecuteBeforeReader.Key.Execute(taskToExecuteBeforeReader.Value);
            }

            await foreach (var latLngFileName in reader.ReadAllAsync())
            {
                foreach (var taskToExecute in listOfTasksToExecuteInReadAllAsync)
                {
                    try
                    {
                        taskToExecute.Value.LatLngFileName = latLngFileName;
                        await taskToExecute.Key.Execute(taskToExecute.Value);
                    }
                    catch (Exception ex)
                    {
                        ((ReadAllAsyncHandlerCommand)command).Exceptions.Enqueue(ex);
                    }
                }
            }

            foreach (var taskToExecuteAfterReader in listOfTasksToExecuteAfterReadAllAsync)
            {
                await taskToExecuteAfterReader.Key.Execute(taskToExecuteAfterReader.Value);
            }
        }
    }
}