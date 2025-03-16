using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace MiuLibrary.Constants
{
    public class Utils
    {
        public static string ProjectDir(string path)
        {
            return $"{Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."))}/{path}";
        }
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 00, 000);

        public static void WriteLog(string log)
        {
            File.AppendAllText("log.txt", log + Environment.NewLine);
        }
        public static bool _checkEmpty(object obj)
        {
            return obj == null || string.IsNullOrEmpty(obj.ToString());
        }
        public static int RandomNumber(int max)
        {
            var random = new MyRandom();
            return random.NextInt(max);
        }
        public static int RandomNumber(int min, int max)
        {
            var random = new MyRandom();
            if (min <= max) return random.NextInt(min, max);
            (min, max) = (max, min);
            return random.NextInt(min, max);
        }
        public static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public static DateTime TimeNow()
        {
            return (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(CurrentTimeMillis().ToString()));
        }

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.Now - Jan1st1970).TotalMilliseconds;
        }

        public static int CurrentTimeSecond()
        {
            return (int)(DateTime.Now - Jan1st1970).TotalSeconds;
        }

        public static string GetDate(int second)
        {
            var num = (long)second * 1000L;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(new TimeSpan(num * 10000)).ToUniversalTime();
            var hour = dateTime.Hour;
            var minute = dateTime.Minute;
            var day = dateTime.Day;
            var month = dateTime.Month;
            var year = dateTime.Year;
            return day + "/" + month + "/" + year + " " + hour + "h";
        }
        public static byte ConvertSbyteToByte(sbyte var)
        {
            if (var > 0)
            {
                return (byte)var;
            }
            return (byte)(var + 256);
        }
        public static byte[] ConvertSbyteToByte(sbyte[] var)
        {
            var array = new byte[var.Length];
            for (var i = 0; i < var.Length; i++)
            {
                if (var[i] > 0)
                {
                    array[i] = (byte)var[i];
                }
                else
                {
                    array[i] = (byte)(var[i] + 256);
                }
            }
            return array;
        }
        public static int[] ConvertStringToInt(string text)
        {
            try
            {
                return Array.ConvertAll(text.Replace("[", "").Replace("]", "").Trim().Split(','), int.Parse).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ConvertMilisecond(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            if (t.Days > 0)
            {
                return t.Hours < 60 ? $"{t.Days:D2}d {t.Hours:D2}h" : $"{t.Days:D2}d";
            }
            if (t.Hours > 0)
            {
                return t.Minutes < 60 ? $"{t.Hours:D2}h {t.Minutes:D2}ph" : $"{t.Hours:D2}h";
            }
            if (t.Minutes > 0)
            {
                return t.Seconds < 60 ? $"{t.Minutes:D2}ph {t.Seconds:D2}s" : $"{t.Minutes:D2}ph";
            }
            return $"{t.Seconds:D2}s";
        }

        public static string ConvertMilisecondToMinute(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Minutes:D2} phút";
        }

        public static string ConvertMilisecondToHour(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Hours:D2} giờ";
        }

        public static string ConvertMilisecondToDay(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Days:D2} ngày";
        }

        public static int ConvertSecondToDay(int time)
        {
            var hours = time / 3600;
            if (hours >= 24)
            {
                return (hours / 24);
            }
            else
            {
                return 0;
            }
        }
        public static string RemoveSpaceText(string text)
        {
            text = text.Trim(); // Xóa đầu cuối
            var trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
            return ConvertToUnSign3(trimmer.Replace(text, " "));
        }

        public static string ConvertToUnSign3(string s)
        {
            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            var temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
