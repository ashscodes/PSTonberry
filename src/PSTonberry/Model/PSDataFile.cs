using System;
using System.IO;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFile : PSTokenizedLineCollection<ScriptBlockAst>
{
    public bool HasPrivateData => !HasError && PrivateData is not null;

    public FileInfo Module { get; set; }

    public PSPrivateData PrivateData { get; set; }

    public DirectoryInfo Root { get; set; }

    public PSDataFile(string filePath)
    {
        Module = new FileInfo(filePath);
        Root = Module.Directory;
        Init(filePath);
    }

    public void Write()
    {
        var tempPath = Path.GetTempFileName();
        using (var writer = new StreamWriter(tempPath))
        {
            writer.AutoFlush = true;
            foreach (var line in Lines)
            {
                // rewrite file.
            }
        }
    }

    private void Init(string filePath)
    {
        ParseError[] errors;
        Token[] tokens;
        var fileAst = Parser.ParseFile(filePath, out tokens, out errors);
        if (errors.Length > 0)
        {
            Error = new InvalidOperationException(string.Format(Resources.DataFileCouldNotBeParsed, filePath));
            return;
        }

        var dataFileStr = fileAst.ToString();
        if (TryParseSection(dataFileStr, Resources.PrivateData, out string privateDataStr, out string newDataFile))
        {
            PrivateData = new PSPrivateData(privateDataStr);
        }

        if (TryGetAstFromString(newDataFile, out ScriptBlockAst scriptBlockAst, out tokens))
        {
            Ast = scriptBlockAst;
            Lines = tokens.ToTokenizedLines();
        }
    }
}