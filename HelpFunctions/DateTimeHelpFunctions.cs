using System;
using System.Collections.Generic;
using System.Text;

namespace WikiHistory.HelpFunctions
{
  static class DateTimeHelpFunctions
  {
    public static string DateTimeToString(DateTime dt)
    {
      return dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") +
        " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
    }

    public static string DaysAgo(DateTime dt)
    {
      DateTime todayEvening = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
      TimeSpan ts = todayEvening.Subtract(dt);
      if (ts.Days == 0)
        return "сегодня";
      else if (ts.Days == 1)
        return "вчера";
      else if (ts.Days < 365)
        return ts.Days.ToString() + " дней назад";
      else
        return (ts.Days / 365).ToString() + " лет и " + (ts.Days % 365).ToString() + " дней назад";
    }
  }
}
