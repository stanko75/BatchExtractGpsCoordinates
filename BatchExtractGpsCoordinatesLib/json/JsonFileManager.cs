using System.Text;
using System.Text.Json;

namespace BatchExtractGpsCoordinatesLib.json;

public class JsonFileManager<T>(List<T>? jsonArray)
{
    private FileStream? _fileStream;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private List<T>? _jsonArray = jsonArray;

    public void OpenStream(string fileName)
    {
        _fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        _reader = new StreamReader(_fileStream, Encoding.UTF8);
        _writer = new StreamWriter(_fileStream, Encoding.UTF8) { AutoFlush = false };
        _jsonArray = ReadJson();
    }

    public void AddItem(T item)
    {
        if (_jsonArray == null)
            throw new InvalidOperationException("JSON array was not initialized!");
        _jsonArray.Add(item);
    }

    public List<T> ReadJson()
    {
        if (_reader == null || _fileStream == null)
            throw new InvalidOperationException("Stream is not open!");

        _fileStream.Seek(0, SeekOrigin.Begin);

        string json = _reader.ReadToEnd();
        return string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<T>>(json) ?? [];
    }

    public void WriteJson()
    {
        if (_writer == null || _fileStream == null)
            throw new InvalidOperationException("Stream is not open!");

        _fileStream.SetLength(0);

        string json = JsonSerializer.Serialize(_jsonArray, new JsonSerializerOptions { WriteIndented = true });
        _writer.Write(json);
    }

    public void CloseStream()
    {
        _writer?.Flush();
        _writer?.Dispose();
        _reader?.Dispose();
        _fileStream?.Dispose();

        _writer = null;
        _reader = null;
        _fileStream = null;
    }
}