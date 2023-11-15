using PSTonberry.Model;

namespace PSTonberry;

internal static class ConfigurationExtensions
{
    public static void GetDataFileValues(this PSTonberryConfiguration config)
    {
        if (config.Type == PSTonberryConfigType.DataFile && config.DataFile.IsTonberryEnabled)
        {

        }
    }
}