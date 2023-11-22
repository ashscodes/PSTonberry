using Tonberry.Core.Model;


namespace PSTonberry.Command;

public abstract class TonberryCommand<T, O> : BaseCommand
    where T : TonberryTask, new()
    where O : TonberryTaskOptions, new()
{
    protected T _task;

    internal TonberryCommand() : base() => _task = new T();

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
        if (_task is not TonberryInitTask)
        {
            _task.Config = GetTonberryConfig();
        }
    }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        _task.SetOptions(GetOptions());
        _task.SetDirectory(UserDirectory);
        Validate();
    }

    protected abstract O GetOptions();

    protected virtual void Validate() { }
}