namespace BatchExtractGpsCoordinatesLib;

public interface IListOfTasksToExecuteBeforeReaderCommand;

public interface IListOfTasksToExecuteBeforeReader
{
    public Task Execute(IListOfTasksToExecuteBeforeReaderCommand command);
}