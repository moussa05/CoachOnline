using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Statics
{
    public static class ConvertTime
    {
        public static DateTime FromUnixTimestamp(int unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;



        }

        public static DateTime FromUnixTimestamp(long unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;


        }

        public static DateTime FromUnixTimestamp(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;


        }

        public static int ToUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime -
                 new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;

        }

        public static long ToUnixTimestampLong(DateTime dateTime)
        {
            return (long)(dateTime -
                 new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;

        }
    }
}
