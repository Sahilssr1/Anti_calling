using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace RiskManagement.Helper
{
    /// <summary>Standalone stub — mirrors the office hashing helper's shape.</summary>
    public class HashedHelper
    {
        public string GetHashedData(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}

namespace DataAccessHelpers
{
    /// <summary>Standalone stub — office helper that trusts self-signed hosts.</summary>
    public static class Ssl
    {
        public static void EnableTrustedHosts()
        {
        }
    }

    /// <summary>JSON shapes for the office telephony challenge/response API.</summary>
    public class ChallengeInfo
    {
        public class Root
        {
            [JsonProperty("response")]
            public Response response { get; set; }
        }

        public class Response
        {
            [JsonProperty("challenge")]
            public string challenge { get; set; }
        }
    }

    public class CookieInfo
    {
        public class Root
        {
            [JsonProperty("response")]
            public Response response { get; set; }
        }

        public class Response
        {
            [JsonProperty("cookie")]
            public string cookie { get; set; }
        }
    }
}
