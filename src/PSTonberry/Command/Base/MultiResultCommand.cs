using Tonberry.Core;
using Tonberry.Core.Model;

namespace PSTonberry.Command;

public abstract class MultiResultCommand<T, O, R> : TonberryCommand<T, O>
    where T : TonberryMultiTask, new()
    where O : TonberryTaskOptions, new()
    where R : TonberryResultCollection, new()
{
    internal R _results;

    internal MultiResultCommand() : base() { }

    protected override void BeginProcessing() => base.BeginProcessing();

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        _results = (R)_task.Invoke();
    }
}