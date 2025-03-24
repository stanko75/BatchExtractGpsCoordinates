using BatchExtractGpsCoordinatesLib.log;
using Microsoft.Data.SqlClient;

namespace BatchExtractGpsCoordinatesLib.db;

public class CreateTableIfNotExistsCommand : IListOfTasksToExecuteBeforeReaderCommand
{
    public string? TableName { get; set; }
    public string? ConnectionString { get; set; }
}

public class CreateTableIfNotExists(ILogger logger) : IListOfTasksToExecuteBeforeReader
{
    public async Task Execute(IListOfTasksToExecuteBeforeReaderCommand command)
    {
        CreateTableIfNotExistsCommand createTableIfNotExistsCommand = (CreateTableIfNotExistsCommand)command;
        string sqlCreateTable = $"""
                                     CREATE TABLE {createTableIfNotExistsCommand.TableName} (
                                         [FileName] [nvarchar](max) NULL,
                                         [Latitude] [float] NULL,
                                         [Longitude] [float] NULL
                                     );
                                 """;

        await using SqlConnection connection = new SqlConnection(createTableIfNotExistsCommand.ConnectionString);
        await connection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlCreateTable, connection);
        sqlCommand.CommandTimeout = 3600;
        try
        {
            await sqlCommand.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 2714)
        {
            logger.Log(new LogEntry(LoggingEventType.Information, $"Table '{createTableIfNotExistsCommand.TableName}' already exists."));
        }
    }
}