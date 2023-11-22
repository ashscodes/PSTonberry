using System.IO;

namespace PSTonberry.Model;

internal class PSTonberryData : PSHashtableData
{
    public PSTonberryData(string tonberryData) => Init(tonberryData);

    public override void Write(StreamWriter writer)
    {
        throw new System.NotImplementedException();
    }
}