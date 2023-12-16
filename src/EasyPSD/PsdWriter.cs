using System;
using System.IO;

namespace EasyPSD;

public sealed class PsdWriter : IDisposable
{
    private int _indentation;

    private string _targetPath;

    private string _tempPath;

    private StreamWriter _writer;

    public PsdWriter(string filePath)
    {
        _indentation = 0;
        _targetPath = filePath;
        _tempPath = Path.GetTempFileName();
        _writer = new StreamWriter(_tempPath);
        _writer.AutoFlush = true;
    }

    public void Dispose()
    {
        if (_writer is not null)
        {
            _writer.Close();
            _writer.Dispose();
        }
    }

    public void WriteFile(PsdFile psdFile) { }

    private string GetIndent()
        => _indentation != 0 ? new string(' ', _indentation) : string.Empty;
}