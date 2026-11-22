using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EmpireOneRestAPIITJ.Security
{
    /// <summary>HMAC-signed bearer tokens for logged-in API calls (stateless).</summary>
    public static class ItjSessionToken
    {
        private const int DefaultLifetimeHours = 72;

        public static string Issue(int userId, string userAlias = null)
        {
            var key = GetSigningKey();
            var exp = DateTimeOffset.UtcNow.AddHours(DefaultLifetimeHours).ToUnixTimeSeconds();
            var alias = (userAlias ?? string.Empty).Trim().ToUpperInvariant();
            var payload = $"{userId}|{alias}|{exp}";
            var sig = Sign(payload, key);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{sig}"));
        }

        public static bool TryValidate(string token, out int userId, out string userAlias)
        {
            userId = 0;
            userAlias = null;
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[0], out userId)) return false;
                userAlias = parts[1];
                if (!long.TryParse(parts[2], out var expUnix)) return false;

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now > expUnix) return false;

                var payload = $"{parts[0]}|{parts[1]}|{parts[2]}";
                var expectedSig = Sign(payload, GetSigningKey());
                return SlowEquals(parts[3], expectedSig);
            }
            catch
            {
                return false;
            }
        }

        private static string GetSigningKey()
        {
            var key = ConfigurationManager.AppSettings["SessionSigningKey"];
            if (string.IsNullOrWhiteSpace(key) || key.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SessionSigningKey is not configured.");
            return key;
        }

        private static string Sign(string payload, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return Convert.ToBase64String(hash);
            }
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}