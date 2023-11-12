using System;
using System.Management.Automation;
using Tonberry.Core.Model;

namespace PSTonberry.Command;

[Alias(new[] { "tonberry-init" })]
[Cmdlet(VerbsCommon.New, "TonberryConfiguration", DefaultParameterSetName = "Init")]
[OutputType(typeof(void))]
public sealed class NewTonberryConfiguration : SingleResultCommand<TonberryInitTask, TonberryInitOptions, TonberryInitResult>, ITonberryInitOptions
{
    [Parameter(Mandatory = false, Position = 1)]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; }

    [Parameter(Mandatory = false, Position = 0)]
    [ValidateNotNullOrEmpty]
    public Uri Repository { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Show { get; set; }

    [Parameter(Mandatory = false)]
    public TonberryVersion Version { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        if (Show.IsPresent)
        {
            _result.Open();
        }
    }

    protected override TonberryInitOptions GetOptions() => new((ITonberryInitOptions)this);

    protected override void Validate() => base.Validate();

    [Parameter(DontShow = true)]
    bool ITonberryInitOptions.Open
    {
        get => Show.IsPresent && Show.ToBool();
        set => Show = value;
    }
}