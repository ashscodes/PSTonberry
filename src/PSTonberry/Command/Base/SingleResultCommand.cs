using Tonberry.Core;
using Tonberry.Core.Model;

namespace PSTonberry.Command;

public abstract class SingleResultCommand<T, O, R> : TonberryCommand<T, O>
    where T : TonberrySingleTask, new()
    where O : TonberryTaskOptions, new()
    where R : TonberryResult, new()
{
    internal R _result;

    internal SingleResultCommand() : base() { }

    protected override void BeginProcessing() => base.BeginProcessing();

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        _result = (R)_task.Invoke();
    }
}