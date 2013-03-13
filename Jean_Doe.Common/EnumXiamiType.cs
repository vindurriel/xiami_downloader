using System;
using System.ComponentModel;
namespace Jean_Doe.Common
{
    [TypeConverter(typeof(EnumMusicType2StringConverter))]
    public enum EnumMusicType
    {
        [Description("专辑")]
        album = 1,
        [Description("艺术家")]
        artist = 2,
        [Description("精选集")]
        collect = 3,
        [Description("歌曲")]
        song = 4,
        [Description("所有")]
        any = 0,
    }
    public static class EnumHelper{
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

    class EnumMusicType2StringConverter : EnumConverter
    {
        public EnumMusicType2StringConverter()
            : base(typeof(EnumMusicType))
        { }

        public override object ConvertTo(ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value,
            System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((Enum)value).GetDescription(); // your code here
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
