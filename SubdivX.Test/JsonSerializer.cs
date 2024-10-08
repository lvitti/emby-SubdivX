using System;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;

namespace SubdivX.Test;

public class JsonSerializer: IJsonSerializer
{
    public void SerializeToStream(object obj, Stream stream)
    {
        throw new NotImplementedException();
    }

    public void SerializeToFile(object obj, string file)
    {
        throw new NotImplementedException();
    }

    public Task<object> DeserializeFromFileAsync(Type type, string file)
    {
        throw new NotImplementedException();
    }

    public T DeserializeFromFile<T>(string file) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<T> DeserializeFromFileAsync<T>(string file) where T : class
    {
        throw new NotImplementedException();
    }

    public T DeserializeFromStream<T>(Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<T> DeserializeFromStreamAsync<T>(Stream stream)
    {
        throw new NotImplementedException();
    }

    public T DeserializeFromString<T>(string text)
    {
        var data =  System.Text.Json.JsonSerializer.Deserialize<T>(text);
        return data;
    }

    public object DeserializeFromStream(Stream stream, Type type)
    {
        throw new NotImplementedException();
    }

    public Task<object> DeserializeFromStreamAsync(Stream stream, Type type)
    {
        throw new NotImplementedException();
    }

    public object DeserializeFromString(string json, Type type)
    {
        throw new NotImplementedException();
    }

    public string SerializeToString(object obj)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> SerializeToSpan(object obj)
    {
        throw new NotImplementedException();
    }

    public T DeserializeFromSpan<T>(ReadOnlySpan<char> text)
    {
        throw new NotImplementedException();
    }

    public object DeserializeFromSpan(ReadOnlySpan<char> json, Type type)
    {
        throw new NotImplementedException();
    }

    public object DeserializeFromBytes(ReadOnlySpan<byte> bytes, Type type)
    {
        throw new NotImplementedException();
    }

    public T DeserializeFromBytes<T>(ReadOnlySpan<byte> bytes)
    {
        throw new NotImplementedException();
    }
}