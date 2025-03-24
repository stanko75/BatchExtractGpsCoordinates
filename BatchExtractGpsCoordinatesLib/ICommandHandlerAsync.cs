namespace BatchExtractGpsCoordinatesLib;

public interface ICommandHandlerAsync<in TCommand>
{
    Task Execute(TCommand command);
}