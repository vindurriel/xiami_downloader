using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.Common
{
    public static class EnumHelper
    {
        public static string GetDescription(this Enum en)
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }
    }

    class Enum2StringConverter<T> : EnumConverter
    {
        public Enum2StringConverter():base(typeof(T))
        { }
        public override object ConvertTo(ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value,
            System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((Enum)value).GetDescription();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
