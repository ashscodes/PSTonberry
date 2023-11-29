using System;
using System.IO;
using System.Management.Automation.Language;

namespace PSDataFileHelper;

public sealed class DataFileReader
{
    public static DataFile ReadFile(string filePath)
    {
        var dataFile = new DataFile(filePath);
        var fileContent = File.ReadAllText(filePath);
        if (fileContent.TryReplaceDataSection(nameof(PrivateData), out string privateData, out string newFileContent))
        {

        }


        dataFile = new DataFile(filePath);
        return dataFile;
    }

    private static bool TryParseDataFileString(string input,
                                               string sectionName,
                                               out Token[] tokens,
                                               out Exception exception)
    {
        exception = null;
        var stringAst = Parser.ParseInput(input, out tokens, out ParseError[] errors);
        if (errors.Length > 0)
        {
            exception = new InvalidOperationException(string.Format(Resources.DataSectionInvalid, sectionName));
            return false;
        }

        return true;
    }
}