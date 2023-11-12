using System.Collections;
using System.Collections.Generic;
using Tonberry.Core.Model;

namespace PSTonberry.Model;

internal class PSTonberryData : IPSTonberryData
{
    private Hashtable _tonberryData;

    public PSTonberryData() : base() => _tonberryData = [];

    public PSTonberryData(TonberryConfiguration config) : this()
    {
        // To be completed
    }

    public void Add(string key, object value) => _tonberryData.Add(key, value);

    public void Clear() => _tonberryData.Clear();

    public void Remove(string key) => _tonberryData.Remove(key);

    public void Set(string key, object value) => _tonberryData[key] = value;

    internal TonberryConfiguration AsTonberryConfiguration()
    {
        return null;
    }
}

internal class PSTonberryProjectData : BaseData, IPSTonberryData
{
    private Hashtable _projectData;

    public PSTonberryProjectData() : base() => _projectData = [];

    public PSTonberryProjectData(TonberryProjectConfiguration config) : this()
    {
        // To be completed
    }

    public void Add(string key, object value)
    {
        throw new System.NotImplementedException();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new System.NotImplementedException();
    }

    public void Set(string key, object value)
    {
        throw new System.NotImplementedException();
    }

    internal static TonberryProjectConfiguration AsTonberryProjectConfiguration()
    {
        return null;
    }
}

internal abstract class BaseData
{
    public List<string> Exclusions { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public string ProjectFile { get; set; }

    public string ReleaseSha { get; set; }

    public string TagTemplate { get; set; }

    public TonberryVersion Version { get; set; }
}

internal interface IPSTonberryData
{
    void Add(string key, object value);

    void Clear();

    void Remove(string key);

    void Set(string key, object value);
}