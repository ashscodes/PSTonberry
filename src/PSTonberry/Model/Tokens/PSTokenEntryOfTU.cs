using System;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenEntry<T> : PSTokenEntry
{
    internal T InitialValue { get; }

    internal abstract Func<T, int> Compare { get; }

    public PSTokenEntry() { }

    internal PSTokenEntry(Token token, int index, T value) : base(token, index)
    {
        InitialValue = value;
        TrackChanges = true;
    }

    public override bool IsModified() => Compare(InitialValue) != 0;

    public abstract void SetValue(T value);
}