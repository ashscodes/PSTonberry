using System;

namespace EasyPSD;

public class PsdReaderException : Exception
{
    public PsdReaderException() : base() { }

    public PsdReaderException(string message) : base(message) { }

    public PsdReaderException(string item, int errorIndex) : base(GetErrorMessage(item, errorIndex)) { }

    private static string GetErrorMessage(string item, int errorIndex)
        => string.Format(Resources.DataFileReadError, item, errorIndex);
}