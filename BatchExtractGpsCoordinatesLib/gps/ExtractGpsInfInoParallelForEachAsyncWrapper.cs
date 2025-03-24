using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BatchExtractGpsCoordinatesLib.gps;

public class ExtractGpsInfoParallelForEachAsyncWrapperCommand
{
    public string? FolderName { get; set; }
    public Channel<LatLngFileNameModel>? GpsInfoChannel { get; set; }
    public ConcurrentQueue<Exception> Exceptions { get; set; } = new();
}

public class ExtractGpsInfoParallelForEachAsyncWrapper(ExtractGpsInfoParallelForEachAsync extractGpsInfoParallelForEachAsync
    , Dictionary<IListOfTasksToExecute, IListOfTasksToExecuteCommand> listOfTasksToExecuteBeforeStartForEach) : ICommandHandlerAsync<ExtractGpsInfoParallelForEachAsyncWrapperCommand>
{
    public async Task Execute(ExtractGpsInfoParallelForEachAsyncWrapperCommand command)
    {
        var tasksToExecuteBeforeStartForEach = new List<Task>();
        try
        {
            foreach (var taskToExecuteBeforeStartForEach in listOfTasksToExecuteBeforeStartForEach)
            {
                if (command.GpsInfoChannel != null)
                {
                    taskToExecuteBeforeStartForEach.Value.GpsInfoChannelReader = command.GpsInfoChannel;
                }

                tasksToExecuteBeforeStartForEach.Add(
                    taskToExecuteBeforeStartForEach.Key.Execute(taskToExecuteBeforeStartForEach.Value));
            }

            var extractGpsInfoParallelForEachAsyncCommand = new ExtractGpsInfoParallelForEachAsyncCommand
            {
                FolderName = command.FolderName
                , GpsInfoChannel = command.GpsInfoChannel
            };
            await extractGpsInfoParallelForEachAsync.Execute(extractGpsInfoParallelForEachAsyncCommand);
        }
        catch (Exception e)
        {
            command.Exceptions.Enqueue(e);
        }
        finally
        {
            await Task.WhenAll(tasksToExecuteBeforeStartForEach);
        }
    }
}