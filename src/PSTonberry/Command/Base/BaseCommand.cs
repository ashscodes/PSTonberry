using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSTonberry.Model;
using Tonberry.Core;
using SMA = System.Management.Automation;

namespace PSTonberry.Command;

public abstract class BaseCommand : PSCmdlet
{
    private bool _allowNonExistentPaths;

    private string[] _pathParameters = ["LiteralPath", "OutFile", "Path"];

    private bool _suppressWildcardExpression;

    protected HashSet<FileSystemInfo> _discoveredPaths;

    protected string[] _paths;

    internal SwitchParameter AllowNonExistentPaths
    {
        get => _allowNonExistentPaths;
        set => _allowNonExistentPaths = value;
    }

    internal SwitchParameter SuppressWildcardExpression
    {
        get => _suppressWildcardExpression;
        set => _suppressWildcardExpression = value;
    }

    internal TonberryRoot TonberryRoot { get; private set; }

    internal DirectoryInfo UserDirectory { get; set; }

    internal BaseCommand() : base() { }

    private bool TryGetFileSystemInfo(string path, out FileSystemInfo fileSystemInfo)
    {
        fileSystemInfo = null;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!path.StartsWith(@"\\?\") && path.Length > 260)
        {
            return false;
        }

        if (Path.Exists(path))
        {
            var fileAttributes = File.GetAttributes(path);
            fileSystemInfo = fileAttributes.HasFlag(FileAttributes.Directory) ? new DirectoryInfo(path) : new FileInfo(path);
            return true;
        }

        var directoryPath = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directoryPath))
        {
            fileSystemInfo = new FileInfo(path);
            return true;
        }
        else if (directoryPath == ".")
        {
            fileSystemInfo = path == "." ? UserDirectory : new FileInfo(path);
            return true;
        }
        else
        {
            fileSystemInfo = directoryPath == path ? new DirectoryInfo(directoryPath) : new FileInfo(path);
            return true;
        }
    }

    private bool FindConfigurationFile(out FileInfo file, string moduleName)
    {
        file = null;
        var config = string.IsNullOrEmpty(moduleName) ? Resources.TonberryConfigurationFile : moduleName + ".psd1";
        foreach (var item in TonberryRoot.Directory.GetFiles(config, SearchOption.AllDirectories))
        {
            file = item;
            return true;
        }

        return false;
    }

    private void GetTonberryRoot()
    {
        var rootDirectory = SessionState.PSVariable.GetValue(Resources.TonberryRoot);
        if (rootDirectory is not null)
        {
            if (rootDirectory is TonberryRoot tonberryRoot)
            {
                TonberryRoot = tonberryRoot;
            }
        }

        TonberryRoot = new TonberryRoot(string.Empty, SessionState.Path.CurrentFileSystemLocation.Path);
    }

    protected PSTonberryConfiguration GetTonberryConfig()
    {
        var config = (PSTonberryConfiguration)SessionState.PSVariable.GetValue(Resources.TonberryConfig);
        if (config is null)
        {
            try
            {
                GetTonberryRoot();
                if (FindConfigurationFile(out FileInfo dataFileInfo, TonberryRoot.ModuleName))
                {
                    var dataFile = new PSDataFile(dataFileInfo.FullName);
                    config = dataFile.GetConfiguration(TonberryRoot.Directory);
                }
                else
                {
                    config = (PSTonberryConfiguration)TonberryRoot.Directory.ReadConfig();
                }

                SetTonberryConfig(config);
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(exception);
            }
        }

        return config;
    }

    protected override void BeginProcessing() => base.BeginProcessing();

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        if (_pathParameters.Any(p => MyInvocation.BoundParameters.ContainsKey(p)))
        {
            ResolveParameterPaths();
        }
    }

    protected void SetTonberryConfig(PSTonberryConfiguration config)
        => SessionState.PSVariable.Set(new PSVariable(Resources.TonberryConfig, config, ScopedItemOptions.None));

    protected virtual void ThrowTerminatingError(Exception exception, string id = "TONBERRY_ERROR")
    {
        ThrowTerminatingError(new ErrorRecord(exception,
                                              id,
                                              ErrorCategory.OperationStopped,
                                              MyInvocation.MyCommand.Name));
    }


    protected internal void ResolveParameterPaths()
    {
        _discoveredPaths = new HashSet<FileSystemInfo>();
        foreach (string path in _paths)
        {
            bool unknownPath = false;
            ErrorRecord pathNotFound = null;
            try
            {
                var pathsInfo = SessionState.Path.GetResolvedPSPathFromPSPath(path);
                unknownPath = pathsInfo.Count == 0;
                foreach (var pathInfo in pathsInfo)
                {
                    if (TryGetFileSystemInfo(pathInfo.ProviderPath, out FileSystemInfo fileSystemInfo))
                    {
                        _discoveredPaths.Add(fileSystemInfo);
                    }
                }
            }
            catch (PSNotSupportedException notSupported)
            {
                WriteError(new ErrorRecord(notSupported.ErrorRecord, notSupported));
            }
            catch (SMA.DriveNotFoundException driveNotFound)
            {
                WriteError(new ErrorRecord(driveNotFound.ErrorRecord, driveNotFound));
            }
            catch (ProviderNotFoundException providerNotFound)
            {
                WriteError(new ErrorRecord(providerNotFound.ErrorRecord, providerNotFound));
            }
            catch (ItemNotFoundException itemNotFound)
            {
                unknownPath = true;
                pathNotFound = new ErrorRecord(itemNotFound.ErrorRecord, itemNotFound);
            }

            if (unknownPath)
            {
                if (AllowNonExistentPaths
                    && (SuppressWildcardExpression || WildcardPattern.ContainsWildcardCharacters(path)))
                {
                    var unresolved = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path,
                                                                                           out ProviderInfo provider,
                                                                                           out PSDriveInfo driveInfo);

                    if (TryGetFileSystemInfo(unresolved, out FileSystemInfo fileSystemInfo))
                    {
                        _discoveredPaths.Add(fileSystemInfo);
                    }
                }
                else
                {
                    if (pathNotFound == null)
                    {
                        var exception = new ItemNotFoundException(string.Format(Resources.PathNotFound, path));
                        pathNotFound = new ErrorRecord(exception, "ItemNotFound", ErrorCategory.ObjectNotFound, path);
                    }

                    WriteError(pathNotFound);
                }
            }
        }
    }
}