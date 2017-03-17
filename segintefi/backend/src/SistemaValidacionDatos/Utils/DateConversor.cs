namespace SVD.Utils
{
    using System;
    public class DateConversor
    {
        /// <summary>
        /// Convierte un valor de fecha DateTime a formato unixtime
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTime(DateTime x)
        {
            DateTimeOffset dto = new DateTimeOffset(x);

            return dto.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Convierte un valor de fecha en formato unixtime a DateTime
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTime(long unixTimeStamp)
        {
            //DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // con la zona horaria UTC
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
