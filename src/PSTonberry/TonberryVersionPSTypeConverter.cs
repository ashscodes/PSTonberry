using System;
using System.Management.Automation;
using Tonberry.Core.Model;

namespace PSTonberry;

public class TonberryVersionPSTypeConverter : PSTypeConverter
{
    public override bool CanConvertFrom(object value, Type destination)
    {
        if (destination.GetType() != typeof(TonberryVersion))
        {
            return false;
        }

        if (value is string || value is Version)
        {
            return true;
        }

        return false;
    }

    public override bool CanConvertTo(object value, Type destination) => CanConvertFrom(value, destination);

    public override object ConvertFrom(object value, Type destination, IFormatProvider provider, bool ignoreCase)
    {
        if (TonberryVersion.TryParse(value.ToString(), out TonberryVersion tonberryVersion))
        {
            return tonberryVersion;
        }

        throw new InvalidCastException(
            string.Format(Resources.InvalidType, value.GetType().FullName, destination.FullName));
    }

    public override object ConvertTo(object value,
                                     Type destination,
                                     IFormatProvider provider,
                                     bool ignoreCase) => ConvertFrom(value, destination, provider, ignoreCase);
}