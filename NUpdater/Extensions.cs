using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NUpdater
{
    public static class Extensions
    {
        public static string ToMd5(this string value)
        {
            var md5Hasher = MD5.Create();

            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));

            var sBuilder = new StringBuilder();

            foreach (var d in data)
            {
                sBuilder.Append(d.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static string ToMd5(this Stream stream)
        {
            var md5Hasher = MD5.Create();

            var data = md5Hasher.ComputeHash(stream);

            var sBuilder = new StringBuilder();

            foreach (var d in data)
            {
                sBuilder.Append(d.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static void ShowBalloonTip(this NotifyIcon notifyIcon, Exception exception)
        {
            notifyIcon.ShowBalloonTip(2000, "", exception.Message, ToolTipIcon.Error);
        }

        public static bool Like(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
