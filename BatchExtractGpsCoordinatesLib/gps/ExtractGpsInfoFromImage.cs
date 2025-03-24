﻿using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace BatchExtractGpsCoordinatesLib.gps;

public class ExtractGpsInfoFromImage : ICommandHandler<ExtractGpsInfoFromImageCommand>
{
    public void Execute(ExtractGpsInfoFromImageCommand command)
    {
        if (command.ImageFileNameToReadGpsFrom is not null)
        {
            IReadOnlyList<MetadataExtractor.Directory> directories =
                ImageMetadataReader.ReadMetadata(command.ImageFileNameToReadGpsFrom);
            GpsDirectory? gps = directories.OfType<GpsDirectory>().FirstOrDefault();
            GeoLocation? location = gps?.GetGeoLocation();

            if (location is null || location.Latitude == 0 || location.Longitude == 0)
            {
                throw new Exception($"Cannot extract GPS info from image: {command.ImageFileNameToReadGpsFrom}!");
            }

            command.LatLngModel = new LatLngModel
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
        else
        {
            throw new NullReferenceException("ImageFileNameToReadGpsFrom is null!");
        }
    }
}