using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class DateTimeUtility
    {

        public static DateTime ParseScrapedSteamDateTime(string data)
        {
            // TODO: Move to a constants file?
            string yearFormat = "MMM d, yyyy @ h:mmtt";
            string noYearFormat = "MMM d @ h:mmtt yyyy"; // Steam doesn't show year if it's current

            string stringToParse = data;
            string format;

            DateTime parsedDateTime;
            if (data.Contains(','))
            {
                format = yearFormat;
            }
            else
            {
                format = noYearFormat;
                stringToParse = $"{data} {DateTime.Now.Year}";
            }

            if (DateTime.TryParseExact(stringToParse, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return parsedDateTime;
            }
            else
            {
                throw new Exception("Workshop-style DateTime parsing error");
            }
        }

    }
}
