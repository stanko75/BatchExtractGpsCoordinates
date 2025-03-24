namespace BatchExtractGpsCoordinatesLib;

public interface IListOfTasksToExecuteAfterReaderCommand;

public interface IListOfTasksToExecuteAfterReader
{
    public Task Execute(IListOfTasksToExecuteAfterReaderCommand command);
}