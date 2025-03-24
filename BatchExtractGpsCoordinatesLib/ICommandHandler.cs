namespace BatchExtractGpsCoordinatesLib;

public interface ICommandHandler<in TCommand>
{
    void Execute(TCommand command);
}