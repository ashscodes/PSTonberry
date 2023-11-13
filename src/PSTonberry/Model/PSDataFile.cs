using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

public class PSDataFile : IPowerShellDataFile
{
    private Hashtable _dataFile;

    private FileInfo _module;

    private string _moduleName;

    private List<PSDataTokenCollection> _tokenCollections;

    public PSTonberryData Tonberry { get; set; }

    public PSDataFile() => _dataFile = [];

    public void GetTonberryData()
    {
        throw new System.NotImplementedException();
    }

    public void ReadDataFile(PSModuleInfo moduleInfo, DirectoryInfo directory)
    {
        _moduleName = moduleInfo.Name;
        if (TryGetPowerShellDataFile(_moduleName, directory))
        {
            var ast = Parser.ParseFile(_module.FullName, out Token[] tokens, out ParseError[] errors);
            if (errors.Length > 0)
            {
                throw new InvalidOperationException(string.Format(Resources.ModuleCouldNotBeParsed, _module.FullName));
            }

            ProcessTokens(tokens);
            var data = ast.Find(i => i is HashtableAst, false);
            if (data is null)
            {
                throw new InvalidOperationException(string.Format(Resources.ModuleDataNotHashtable, _module.FullName));
            }

            _dataFile = (Hashtable)data.SafeGetValue();
        }
    }

    public void SetTonberryData()
    {
        throw new System.NotImplementedException();
    }

    public void WriteDataFile()
    {
        throw new System.NotImplementedException();
    }

    private void ProcessTokens(Token[] tokens)
    {
        if (tokens is not null && tokens.Length != 0)
        {
            int indent = 0;
            int index = 0;
            int line = 1;
            var tokenCollection = new PSDataTokenCollection(line, indent);
            for (int i = 0; i < tokens.Length; i++)
            {
                tokenCollection.Add(tokens[i], index);
                index++;
                if (tokens[i].Kind is TokenKind.EndOfInput)
                {
                    _tokenCollections.Add(tokenCollection);
                }

                if (tokens[i].Kind is TokenKind.NewLine)
                {
                    _tokenCollections.Add(tokenCollection);
                    line++;
                    index = 0;
                    tokenCollection = new PSDataTokenCollection(line, indent);
                }

                if (tokens[i].Kind is TokenKind.AtCurly)
                {
                    indent += 2;
                }

                if (tokens[i].Kind is TokenKind.RCurly)
                {
                    indent -= 2;
                }
            }
        }
    }

    private bool TryGetPowerShellDataFile(string name, DirectoryInfo directory)
    {
        var moduleFileName = name + ".psd1";
        var files = directory.GetFiles(moduleFileName, SearchOption.TopDirectoryOnly);
        if (files is null || files.Length == 0)
        {
            throw new FileNotFoundException(string.Format(Resources.ModuleNotFound, moduleFileName));
        }

        if (files.Length == 1)
        {
            _module = files[0];
            return true;
        }

        foreach (var file in files)
        {
            if (file.Directory.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                _module = file;
                return true;
            }
        }

        return false;
    }
}

internal interface IPowerShellDataFile
{
    void GetTonberryData();

    void ReadDataFile(PSModuleInfo moduleInfo, DirectoryInfo directory);

    void SetTonberryData();

    void WriteDataFile();
}