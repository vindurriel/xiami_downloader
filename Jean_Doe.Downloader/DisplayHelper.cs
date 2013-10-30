namespace Jean_Doe.Downloader
{
    public static class DisplayHelper
    {
        public static string ToReadableString(this long size)
        {
            if (size / 1000000L > 0L)
            {
                return (size / 10000L / 100.0).ToString() + "M";
            }
            if (size / 1000L > 0L)
            {
                return (size / 10L / 100.0).ToString() + "K";
            }
            return "< 1k";
        }
    }
}
