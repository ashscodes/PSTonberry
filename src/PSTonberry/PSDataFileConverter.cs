using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using PSTonberry.Model;

namespace PSTonberry;

internal static class PSDataFileConverter
{
    public static bool TryReadDataFile(PSModuleInfo moduleInfo, DirectoryInfo directory, out PSDataFile dataFile)
    {
        var moduleName = moduleInfo.Name;
        if (TryGetPowerShellDataFile(moduleName, directory, out string filePath))
        {
            PSDataFileAst<HashtableAst> dataFileAst;
            try
            {
                dataFileAst = FindAst<HashtableAst>(filePath, Resources.ModuleCouldNotBeParsed, filePath, true);
                dataFileAst.TryFindAst();
                if (dataFileAst.Ast is null)
                {
                    throw new InvalidOperationException(string.Format(Resources.ModuleDataNotHashtable, filePath));
                }
            }
            catch
            {
                dataFile = new PSDataFile(moduleName, directory);
                return false;
            }

            if (dataFileAst.TryParseSection(Resources.TonberryData, out string tonberrySection))
            {
                var tonberryAst = FindAst<HashtableAst>(filePath, Resources.TonberryDataCouldNotBeParsed, filePath);
                dataFile = new PSDataFile(dataFileAst, tonberryAst, filePath, moduleName, directory);
            }

            dataFile = new PSDataFile(dataFileAst, filePath, moduleName, directory);
            return true;
        }

        dataFile = new PSDataFile(moduleName, directory);
        return false;
    }

    public static void WriteDataFile(PSDataFile dataFile) { }

    private static PSDataFileAst<T> FindAst<T>(string input,
                                               string errorMessage,
                                               string filePath,
                                               bool isFile = false) where T : Ast
    {
        ParseError[] errors;
        Token[] tokens;
        ScriptBlockAst ast = isFile
            ? Parser.ParseFile(input, out tokens, out errors)
            : Parser.ParseInput(input, out tokens, out errors);

        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Format(errorMessage, filePath));
        }

        return new PSDataFileAst<T>(ast, tokens);
    }

    private static bool TryGetPowerShellDataFile(string moduleName, DirectoryInfo directory, out string path)
    {
        path = string.Empty;
        var moduleFileName = moduleName + ".psd1";
        var files = directory.GetFiles(moduleFileName, SearchOption.TopDirectoryOnly);
        if (files is null || files.Length == 0)
        {
            throw new FileNotFoundException(string.Format(Resources.ModuleNotFound, moduleFileName));
        }

        if (files.Length == 1)
        {
            path = files[0].FullName;
            return true;
        }

        foreach (var file in files)
        {
            if (file.Directory.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                path = file.FullName;
                return true;
            }
        }

        return false;
    }

    private static void WritePSDataFileToken(PSDataFileToken dataFileToken, StreamWriter writer) { }
}