using System;
using System.IO;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFile : PSTokenCollection<ScriptBlockAst>
{
    public bool HasPrivateData => !HasError && PrivateData is not null;

    public FileInfo Path { get; set; }

    public PSPrivateData PrivateData { get; set; }

    public DirectoryInfo Root { get; set; }

    public PSDataFile(string filePath)
    {
        Path = new FileInfo(filePath);
        Root = Path.Directory;
        Init(filePath);
    }

    public override void Write(StreamWriter writer)
    {
        throw new NotImplementedException();
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
        if (TryParseSection(dataFileStr, Resources.PrivateData, out string privateDataStr))
        {
            PrivateData = new PSPrivateData(privateDataStr);
            dataFileStr = dataFileStr.Replace(privateDataStr, null);
        }

        if (TryGetAstFromString(dataFileStr, out ScriptBlockAst scriptBlockAst, out tokens))
        {
            Ast = scriptBlockAst;
            Lines = tokens.ToTokenizedLines();
        }
    }
}