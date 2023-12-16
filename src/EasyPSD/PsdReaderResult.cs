using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class PsdReaderResult
{
    public PsdFile DataFile { get; private set; }

    public List<Exception> Errors { get; private set; }

    public Token[] Tokens { get; private set; }

    public bool HasErrors => Errors is not null && Errors.Count > 0;

    internal PsdReaderResult(PsdFile dataFile, List<Exception> errors, Token[] tokens = null)
    {
        DataFile = dataFile;
        Errors = errors;
        Tokens = tokens;
    }
}