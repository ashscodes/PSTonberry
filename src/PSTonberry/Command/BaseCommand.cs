using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSTonberry.Model;
using Tonberry.Core;
using Tonberry.Core.Model;
using SMA = System.Management.Automation;

namespace PSTonberry.Command;

public abstract class BaseCommand<T, O> : PSCmdlet
    where T : TonberryTask, new()
    where O : TonberryTaskOptions, new()
{
    protected const string TonberryConfig = "global:TonberryConfig";

    protected const string TonberryRoot = "global:TonberryRoot";

    private bool _allowNonExistentPaths;

    private string[] _pathParameters = ["LiteralPath", "OutFile", "Path"];

    private SearchOption _searchOption;

    private bool _suppressWildcardExpression;

    protected HashSet<FileSystemInfo> _discoveredPaths;

    protected string[] _paths;

    protected T _task;

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

    internal DirectoryInfo UserDirectory { get; set; }

    internal BaseCommand() : base() => _task = new T();

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

    private void ResolveParameterPaths()
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

    protected void CreateDirectory(FileSystemInfo fileSystemInfo)
    {
        if (fileSystemInfo is FileInfo fileInfo)
        {
            Directory.CreateDirectory(fileInfo.Directory.FullName);
        }

        Directory.CreateDirectory(fileSystemInfo.FullName);
    }

    protected virtual void ThrowTerminatingError(Exception exception, string id = "TONBERRY_ERROR")
    {
        ThrowTerminatingError(new ErrorRecord(exception,
                                              id,
                                              ErrorCategory.OperationStopped,
                                              MyInvocation.MyCommand.Name));
    }

    protected virtual void Validate() { }

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
        GetTonberryConfig();
    }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        if (_pathParameters.Any(p => MyInvocation.BoundParameters.ContainsKey(p)))
        {
            ResolveParameterPaths();
        }

        _task.SetOptions(GetOptions());
        _task.SetDirectory(UserDirectory);
    }

    protected abstract O GetOptions();

    protected virtual void GetTonberryConfig()
    {
        PSTonberryConfiguration config = (PSTonberryConfiguration)SessionState.PSVariable.GetValue(TonberryConfig);
        if (config is null)
        {
            GetTonberryRoot();
            if (_task is not TonberryInitTask)
            {
                try
                {
                    if (PSDataFileConverter.TryReadDataFile(SessionState.Module,
                                                            UserDirectory,
                                                            out PSDataFile dataFile))
                    {
                        config = new PSTonberryConfiguration(dataFile);
                    }
                    else
                    {
                        config = (PSTonberryConfiguration)UserDirectory.ReadConfig();
                    }
                }
                catch (Exception exception)
                {
                    ThrowTerminatingError(exception);
                }
            }
        }

        _task.Config = config;
    }

    protected virtual void GetTonberryRoot()
    {
        var tonberryRoot = SessionState.PSVariable.GetValue(TonberryRoot);
        if (tonberryRoot is not null)
        {
            if (tonberryRoot is string stringRoot)
            {
                UserDirectory = new DirectoryInfo(stringRoot);
                return;
            }

            if (tonberryRoot is DirectoryInfo dirRoot)
            {
                UserDirectory = dirRoot;
                return;
            }
        }

        UserDirectory = new DirectoryInfo(SessionState.Path.CurrentFileSystemLocation.Path);
    }

    protected void SetTonberryConfig(PSTonberryConfiguration config)
        => SessionState.PSVariable.Set(new PSVariable(TonberryConfig, config, ScopedItemOptions.None));
}