using System.Collections;
using System.Collections.Generic;
using Tonberry.Core.Model;

namespace PSTonberry.Model;

public class PSTonberryData : IPSTonberryData
{
    private Hashtable _tonberryData;

    public List<string> Exclusions { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public string ProjectFile { get; set; }

    public string ReleaseSha { get; set; }

    public string TagTemplate { get; set; }

    public TonberryVersion Version { get; set; }

    public PSTonberryData() => _tonberryData = [];

    public PSTonberryData(TonberryConfiguration config) : this()
    {
        // To be completed
    }

    void IPSTonberryData.Add(string key, object value) => _tonberryData?.Add(key, value);

    void IPSTonberryData.Clear() => _tonberryData?.Clear();

    void IPSTonberryData.Remove(string key) => _tonberryData?.Remove(key);

    void IPSTonberryData.Set(string key, object value)
    {
        if (_tonberryData.ContainsKey(key))
        {
            _tonberryData[key] = value;
        }
    }
}

internal class PSTonberryProjectData : IPSTonberryData
{
    private Hashtable _projectData;

    public List<string> Exclusions { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public string ProjectFile { get; set; }

    public string ReleaseSha { get; set; }

    public string TagTemplate { get; set; }

    public TonberryVersion Version { get; set; }

    public PSTonberryProjectData() => _projectData = [];

    public PSTonberryProjectData(TonberryProjectConfiguration config) : this()
    {
        // To be completed
    }

    void IPSTonberryData.Add(string key, object value) => _projectData?.Add(key, value);

    void IPSTonberryData.Clear() => _projectData?.Clear();

    void IPSTonberryData.Remove(string key) => _projectData?.Remove(key);

    void IPSTonberryData.Set(string key, object value)
    {
        if (_projectData.ContainsKey(key))
        {
            _projectData[key] = value;
        }
    }
}

internal interface IPSTonberryData
{
    List<string> Exclusions { get; set; }

    string Language { get; set; }

    string Name { get; set; }

    string ProjectFile { get; set; }

    string ReleaseSha { get; set; }

    string TagTemplate { get; set; }

    TonberryVersion Version { get; set; }

    void Add(string key, object value);

    void Clear();

    void Remove(string key);

    void Set(string key, object value);
}