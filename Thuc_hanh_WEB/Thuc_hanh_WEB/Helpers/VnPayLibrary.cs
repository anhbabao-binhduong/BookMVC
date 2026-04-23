using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Thuc_hanh_WEB.Helpers
{
    /// <summary>
    /// VNPAY payment library - follows official VNPAY v2.1.0 specification.
    /// Sign data uses RAW (not URL-encoded) values sorted by key (Ordinal).
    /// URL query string uses URL-encoded values.
    /// </summary>
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData
            = new SortedList<string, string>(StringComparer.Ordinal);

        // ── Add request parameter ─────────────────────────────────────────
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData[key] = value;
        }

        // ── Build payment URL ─────────────────────────────────────────────
        /// <summary>
        /// Builds the VNPAY payment URL.
        /// Sign data = URL-encoded key=value pairs joined by &amp; (sorted Ordinal).
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();

            foreach (var kv in _requestData)
            {
                if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value))
                    continue;

                data.Append(WebUtility.UrlEncode(kv.Key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(kv.Value))
                    .Append('&');
            }

            string queryString = data.ToString().TrimEnd('&');
            string secureHash  = HmacSha512(hashSecret, queryString);

            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + secureHash;
        }

        // ── Validate VNPAY return signature ───────────────────────────────
        /// <summary>
        /// Validates the HMAC-SHA512 signature on the VNPAY return URL.
        /// </summary>
        public static bool ValidateSignature(NameValueCollection queryString, string hashSecret)
        {
            string vnpSecureHash = queryString["vnp_SecureHash"];
            if (string.IsNullOrEmpty(vnpSecureHash)) return false;

            // Collect all vnp_ params except hash fields, sort Ordinal
            var fields = new SortedList<string, string>(StringComparer.Ordinal);
            foreach (string key in queryString)
            {
                if (!string.IsNullOrEmpty(key) &&
                    key.StartsWith("vnp_") &&
                    key != "vnp_SecureHash" &&
                    key != "vnp_SecureHashType")
                {
                    fields[key] = queryString[key];
                }
            }

            // Build sign data (URL-encoded per v2.1.0 standard)
            var rawData = new StringBuilder();
            foreach (var kv in fields)
            {
                rawData.Append(WebUtility.UrlEncode(kv.Key))
                       .Append('=')
                       .Append(WebUtility.UrlEncode(kv.Value))
                       .Append('&');
            }

            string signData = rawData.ToString().TrimEnd('&');
            string myHash   = HmacSha512(hashSecret, signData);

            return myHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);
        }

        // ── HMAC-SHA512 ───────────────────────────────────────────────────
        public static string HmacSha512(string key, string data)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}

