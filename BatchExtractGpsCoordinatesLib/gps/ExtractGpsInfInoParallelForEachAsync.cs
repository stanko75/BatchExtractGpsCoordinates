using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BatchExtractGpsCoordinatesLib.gps;

public class ExtractGpsInfoParallelForEachAsyncCommand
{
    public string? FolderName { get; set; }
    public Channel<LatLngFileNameModel>? GpsInfoChannel { get; set; } = Channel.CreateUnbounded<LatLngFileNameModel>();
}

public class ExtractGpsInfoParallelForEachAsync : ICommandHandlerAsync<ExtractGpsInfoParallelForEachAsyncCommand>
{
    private readonly ConcurrentQueue<Exception> _exceptions = new();

    public async Task Execute(ExtractGpsInfoParallelForEachAsyncCommand command)
    {
        if (Directory.Exists(command.FolderName))
        {
            var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp"
            };

            await Parallel.ForEachAsync(
                EnumerateFilesSafe(command.FolderName), async (imageFileName, ct) =>
                {
                    if (imageExtensions.Contains(Path.GetExtension(imageFileName).ToLower()))
                    {
                        var extractGpsInfoFromImageCommand = new ExtractGpsInfoFromImageCommand
                        {
                            ImageFileNameToReadGpsFrom = imageFileName
                        };

                        if (command.GpsInfoChannel != null)
                            await command.GpsInfoChannel.Writer.WriteAsync(
                                ExtractGpsInfoFromImage(extractGpsInfoFromImageCommand), ct);
                    }
                });

            command.GpsInfoChannel?.Writer.Complete();

            if (!_exceptions.IsEmpty)
            {
                throw new AggregateException("Error in der Parallel.ForEachAsync", _exceptions);
            }
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory {command.FolderName} not found.");
        }
    }

    private LatLngFileNameModel ExtractGpsInfoFromImage(
        ExtractGpsInfoFromImageCommand extractGpsInfoFromImageCommand)
    {
        LatLngFileNameModel latLngFileNameModel = new LatLngFileNameModel();
        ExtractGpsInfoFromImage extractGpsInfoFromImage = new ExtractGpsInfoFromImage();

        try
        {
            extractGpsInfoFromImage.Execute(extractGpsInfoFromImageCommand);
            if (extractGpsInfoFromImageCommand.LatLngModel != null)
            {
                latLngFileNameModel.FileName = extractGpsInfoFromImageCommand.ImageFileNameToReadGpsFrom;
                latLngFileNameModel.Latitude = extractGpsInfoFromImageCommand.LatLngModel.Latitude;
                latLngFileNameModel.Longitude = extractGpsInfoFromImageCommand.LatLngModel.Longitude;
            }
            else
            {
                latLngFileNameModel.FileName = null;
                latLngFileNameModel.Latitude = null;
                latLngFileNameModel.Longitude = null;
                throw new Exception($"No GPS info found in {extractGpsInfoFromImageCommand.ImageFileNameToReadGpsFrom}");
            }
        }
        catch (Exception e)
        {
            _exceptions.Enqueue(e);
        }

        return latLngFileNameModel;
    }

    private static IEnumerable<string> EnumerateFilesSafe(string root)
    {
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(root);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var file in files)
        {
            yield return file;
        }

        IEnumerable<string> directories;
        try
        {
            directories = Directory.EnumerateDirectories(root);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var dir in directories)
        {
            foreach (var file in EnumerateFilesSafe(dir))
            {
                yield return file;
            }
        }
    }
}