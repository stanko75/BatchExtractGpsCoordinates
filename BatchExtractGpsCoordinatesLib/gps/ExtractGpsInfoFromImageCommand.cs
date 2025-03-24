namespace BatchExtractGpsCoordinatesLib.gps;

public class ExtractGpsInfoFromImageCommand
{
    public string? ImageFileNameToReadGpsFrom { get; set; }
    public LatLngModel? LatLngModel { get; set; }
}