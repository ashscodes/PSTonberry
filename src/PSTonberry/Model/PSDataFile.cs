using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFile : IPowerShellDataFile
{
    private Hashtable _dataFile;

    private FileInfo _module;

    private string _moduleName;

    private Token[] _tokens;

    public PSDataFile() => _dataFile = [];

    public PSTonberryData GetTonberryData()
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

            _tokens = tokens;
            var data = ast.Find(i => i is HashtableAst, false);
            if (data is not null)
            {
                _dataFile = (Hashtable)data.SafeGetValue();
                return;
            }

            throw new InvalidOperationException(string.Format(Resources.ModuleDataNotHashtable, _module.FullName));
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
    PSTonberryData GetTonberryData();

    void ReadDataFile(PSModuleInfo moduleInfo, DirectoryInfo directory);

    void SetTonberryData();

    void WriteDataFile();
}