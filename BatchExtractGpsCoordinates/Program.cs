using BatchExtractGpsCoordinatesLib;
using BatchExtractGpsCoordinatesLib.csv;
using BatchExtractGpsCoordinatesLib.gps;
using System.Threading.Channels;
using BatchExtractGpsCoordinatesLib.db;
using BatchExtractGpsCoordinatesLib.json;
using BatchExtractGpsCoordinatesLib.log;

var startTime = DateTime.Now;
Console.WriteLine($"Start: {startTime:HH:mm:ss.fff}");
try
{
    Dictionary<string, string> ParseArguments(string[] args) =>
        args.Where(arg => arg.Contains(":"))
            .Select(arg => arg.Split([':'], 2))
            .ToDictionary(parts => parts[0], parts => parts[1]);

    var logFilePath = Path.Combine(Environment.CurrentDirectory, AppDomain.CurrentDomain.FriendlyName + ".log");
    ILogger logger = new Log4NetLogger(logFilePath);

    var parameters = ParseArguments(args);
    if (!parameters.TryGetValue("folder", out string? folderPath))
    {
        if (folderPath is null)
        {
            throw new ArgumentException("folder should be present");
        }
    }
    else
    {
        var listOfTasksToExecuteInReadAllAsync = new Dictionary<IListOfTasksToExecuteInReader, IListOfTasksToExecuteInReaderCommand>();
        var listOfTasksToExecuteAfterReadAllAsync = new Dictionary<IListOfTasksToExecuteAfterReader, IListOfTasksToExecuteAfterReaderCommand>();
        var listOfTasksToExecuteBeforeReadAllAsync = new Dictionary<IListOfTasksToExecuteBeforeReader, IListOfTasksToExecuteBeforeReaderCommand>();

        if (parameters.TryGetValue("connectionString", out string? connectionString))
        {
            string tableName = parameters.GetValueOrDefault("tableName", "GpsInfo");
            CreateTableForDbTasksToExecuteBeforeRead(connectionString, tableName, listOfTasksToExecuteBeforeReadAllAsync, logger);

            AddTasksForDb
            (
                connectionString
                , tableName
                , listOfTasksToExecuteInReadAllAsync
            );
        }

        if (parameters.TryGetValue("csvFile", out string? csvFile))
        {
            AddTasksForCsvFile
            (
                csvFile
                , listOfTasksToExecuteInReadAllAsync
            );
        }

        if (parameters.TryGetValue("jsonFile", out string? jsonFile))
        {
            var jsonArray = new List<LatLngFileNameModel>();
            var jsonFileManager = new JsonFileManager<LatLngFileNameModel>(jsonArray);

            AddTasksForJsonFile
            (
                jsonFile
                , jsonFileManager
                , listOfTasksToExecuteBeforeReadAllAsync
                , listOfTasksToExecuteInReadAllAsync
                , listOfTasksToExecuteAfterReadAllAsync
            );
        }

        if (connectionString is null && csvFile is null && jsonFile is null)
        {
            throw new ArgumentException("At least one parameter—connectionString, csvFile, or jsonFile—must be present.");
        }

        var readAllAsyncHandlerCommand = new ReadAllAsyncHandlerCommand();
        var readAllAsyncHandler = new ReadAllAsyncHandler(
            listOfTasksToExecuteBeforeReadAllAsync
            , listOfTasksToExecuteInReadAllAsync
            , listOfTasksToExecuteAfterReadAllAsync
            );
        var listOfTasksToExecuteBeforeStartForEach =
            new Dictionary<IListOfTasksToExecute, IListOfTasksToExecuteCommand>
            {
                {
                    readAllAsyncHandler, readAllAsyncHandlerCommand
                }
            };

        var extractGpsInfoParallelForEachAsync = new ExtractGpsInfoParallelForEachAsync();
        var extractGpsInfoParallelForEachAsyncWrapper =
            new ExtractGpsInfoParallelForEachAsyncWrapper(extractGpsInfoParallelForEachAsync,
                listOfTasksToExecuteBeforeStartForEach);

        var extractGpsInfoParallelForEachAsyncWrapperCommand = new ExtractGpsInfoParallelForEachAsyncWrapperCommand
        {
            FolderName = folderPath,
            GpsInfoChannel = Channel.CreateUnbounded<LatLngFileNameModel>()
        };
        try
        {
            await extractGpsInfoParallelForEachAsyncWrapper.Execute(
                extractGpsInfoParallelForEachAsyncWrapperCommand);
        }
        catch (AggregateException aex)
        {
            logger.Log(aex);
        }
        catch (Exception e)
        {
            logger.Log(e);
            Console.WriteLine(e);
        }

        if (!extractGpsInfoParallelForEachAsyncWrapperCommand.Exceptions.IsEmpty)
        {
            foreach (var exception in extractGpsInfoParallelForEachAsyncWrapperCommand.Exceptions)
            {
                logger.Log(exception);
            }
        }

        if (!readAllAsyncHandlerCommand.Exceptions.IsEmpty)
        {
            foreach (var exception in readAllAsyncHandlerCommand.Exceptions)
            {
                logger.Log(exception);
            }
        }

    }
}
finally
{
    var endTime = DateTime.Now;
    Console.WriteLine($"End: {endTime:HH:mm:ss.fff}, last: {(endTime - startTime):hh\\:mm\\:ss\\.fff}");
}

void AddTasksForCsvFile(
    string csvFile
    , Dictionary<IListOfTasksToExecuteInReader, IListOfTasksToExecuteInReaderCommand> listOfTasksToExecuteInReadAllAsync)
{
    WriteGpsToCsv writeGpsToCsv = new WriteGpsToCsv();
    WriteGpsToCsvCommand writeGpsToCsvCommand = new WriteGpsToCsvCommand
    {
        FilePath = csvFile
    };

    listOfTasksToExecuteInReadAllAsync.Add(writeGpsToCsv, writeGpsToCsvCommand);
}

void CreateTableForDbTasksToExecuteBeforeRead(
    string connectionString
    , string tableName
    , Dictionary<IListOfTasksToExecuteBeforeReader, IListOfTasksToExecuteBeforeReaderCommand> listOfTasksToExecuteBeforeReadAllAsync
    , ILogger logger
    )
{
    CreateTableIfNotExists createTableIfNotExists = new CreateTableIfNotExists(logger);
    CreateTableIfNotExistsCommand createTableIfNotExistsCommand = new CreateTableIfNotExistsCommand
    {
        ConnectionString = connectionString
        , TableName = tableName
    };
    listOfTasksToExecuteBeforeReadAllAsync.Add(createTableIfNotExists, createTableIfNotExistsCommand);
}

void AddTasksForDb(string connectionString
    , string tableName
    , Dictionary<IListOfTasksToExecuteInReader, IListOfTasksToExecuteInReaderCommand> listOfTasksToExecuteInReadAllAsync
    )
{
    WriteGpsToDb writeGpsToDb = new WriteGpsToDb();
    WriteGpsToDbCommand writeGpsToDbCommand = new WriteGpsToDbCommand
    {
        ConnectionString = connectionString
        , TableName = tableName
    };

    listOfTasksToExecuteInReadAllAsync.Add(writeGpsToDb, writeGpsToDbCommand);
}

void AddTasksForJsonFile(
    string jsonFile
    , JsonFileManager<LatLngFileNameModel> jsonFileManager
    , Dictionary<IListOfTasksToExecuteBeforeReader, IListOfTasksToExecuteBeforeReaderCommand> listOfTasksToExecuteBeforeReadAllAsync
    , Dictionary<IListOfTasksToExecuteInReader, IListOfTasksToExecuteInReaderCommand> listOfTasksToExecuteInReadAllAsync
    , Dictionary<IListOfTasksToExecuteAfterReader, IListOfTasksToExecuteAfterReaderCommand> listOfTasksToExecuteAfterReadAllAsync)
{

    OpenJsonStreamCommand openJsonStreamCommand = new OpenJsonStreamCommand
    {
        FileName = jsonFile
        , JsonFileManager = jsonFileManager
    };
    OpenJsonStream openJsonStream = new OpenJsonStream();
    listOfTasksToExecuteBeforeReadAllAsync.Add(openJsonStream, openJsonStreamCommand);

    WriteGpsToJsonCommand writeGpsToCsvCommand = new WriteGpsToJsonCommand
    {
        FilePath = jsonFile
        , JsonFileManager = jsonFileManager
    };
    WriteGpsToJson writeGpsToCsv = new WriteGpsToJson();
    listOfTasksToExecuteInReadAllAsync.Add(writeGpsToCsv, writeGpsToCsvCommand);

    CloseJsonStreamCommand closeJsonStreamCommand = new CloseJsonStreamCommand
    {
        JsonFileManager = jsonFileManager
    };
    CloseJsonStream closeJsonStream = new CloseJsonStream();
    listOfTasksToExecuteAfterReadAllAsync.Add(closeJsonStream, closeJsonStreamCommand);
}