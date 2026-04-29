
namespace MadnClient;

internal static class ClientTime
{
    public static string FormatLocal(DateTime? utcTime)
    {
        if (!utcTime.HasValue)
        {
            return "-";
        }

        return ToLocal(utcTime.Value).ToString("g");
    }

    public static DateTime ToLocal(DateTime utcTime)
    {
        // Server sends UTC; if the JSON deserializer leaves the Kind unspecified,
        // treat it as UTC so the client still displays the correct local time.
        if (utcTime.Kind == DateTimeKind.Unspecified)
        {
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        }

        return utcTime.ToLocalTime();
    }
}

