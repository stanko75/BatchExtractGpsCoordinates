using BatchExtractGpsCoordinatesLib.gps;
using Microsoft.Data.SqlClient;

namespace BatchExtractGpsCoordinatesLib.db;

public class WriteGpsToDbCommand : IListOfTasksToExecuteInReaderCommand
{
    public LatLngFileNameModel? LatLngFileName { get; set; }
    public string? ConnectionString { get; set; }
    public string? TableName { get; set; }
}

public class WriteGpsToDb : IListOfTasksToExecuteInReader
{
    public async Task Execute(IListOfTasksToExecuteInReaderCommand command)
    {
        if (command.LatLngFileName is not null 
            && command.LatLngFileName.FileName is not null
            && command.LatLngFileName.Latitude is not null
            && command.LatLngFileName.Longitude is not null)
        {
            string query =
                $"INSERT INTO [{((WriteGpsToDbCommand)command).TableName}] ([FileName], [Latitude], [Longitude]) VALUES (@fileName, @latitude, @longitude);";

            await using SqlConnection connection =
                new SqlConnection(((WriteGpsToDbCommand)command).ConnectionString);
            await connection.OpenAsync();

            await using SqlCommand sqlCommand = new SqlCommand(query, connection);
            sqlCommand.CommandTimeout = 3600;
            sqlCommand.Parameters.AddWithValue("@fileName",
                command.LatLngFileName.FileName);
            sqlCommand.Parameters.AddWithValue("@latitude", command.LatLngFileName.Latitude);
            sqlCommand.Parameters.AddWithValue("@longitude", command.LatLngFileName.Longitude);
            await sqlCommand.ExecuteNonQueryAsync();
        }
    }
}