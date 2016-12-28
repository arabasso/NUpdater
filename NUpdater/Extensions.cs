﻿using System.IO;
using System.Security.Cryptography;
using System.Text;

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
    }
}