using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using Serilog;

namespace Dalamud.Injector;

public static class ArgumentEncryption
{
    private const string EncryptionChecksumTable = "fX1pGtdS5CAP4_VL";

    public static List<string> Decrypt(IEnumerable<string> arguments, ref bool shouldEncrypt)
    {
        var argDelimiterRegex = new Regex(" (?<!(?:^|[^ ])(?:  )*)/");
        var kvDelimiterRegex = new Regex(" (?<!(?:^|[^ ])(?:  )*)=");
        var shouldEncryptLocal = shouldEncrypt;

        var newArgs = arguments.SelectMany(x =>
        {
            if (!x.StartsWith("//**sqex0003") || !x.EndsWith("**//"))
            {
                return new List<string>() { x };
            }

            var checksum = EncryptionChecksumTable.IndexOf(x[x.Length - 5]);
            if (checksum == -1)
            {
                return new List<string>() { x };
            }

            var encData = Convert.FromBase64String(x.Substring(12, x.Length - 12 - 5).Replace('-', '+')
                                                    .Replace('_', '/').Replace('*', '='));
            var rawData = new byte[encData.Length];

            for (var i = (uint)checksum; i < 0x10000u; i += 0x10)
            {
                var bf = new LegacyBlowfish(Encoding.UTF8.GetBytes($"{i << 16:x08}"));
                Buffer.BlockCopy(encData, 0, rawData, 0, rawData.Length);
                bf.Decrypt(ref rawData);
                var rawString = Encoding.UTF8.GetString(rawData).Split('\0', 2).First();
                shouldEncryptLocal = true;
                var args = argDelimiterRegex.Split(rawString).Skip(1)
                                            .Select(y => string.Join('=', kvDelimiterRegex.Split(y, 2))
                                                               .Replace("  ", " ")).ToList();
                if (!args.Any())
                {
                    continue;
                }

                if (!args.First().StartsWith("T="))
                {
                    continue;
                }

                if (!uint.TryParse(args.First().Substring(2), out var tickCount))
                {
                    continue;
                }

                if (tickCount >> 16 != i)
                {
                    continue;
                }

                return args.Skip(1);
            }

            return new List<string> { x };
        });

        shouldEncrypt = shouldEncryptLocal;
        return newArgs.ToList();
    }

    public static string Encrypt(IList<string> gameArguments)
    {
        var rawTickCount = (uint)Environment.TickCount;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            [DllImport("c")]
#pragma warning disable SA1300
            static extern ulong clock_gettime_nsec_np(int clockId);
#pragma warning restore SA1300

            const int CLOCK_MONOTONIC_RAW = 4;
            var rawTickCountFixed = clock_gettime_nsec_np(CLOCK_MONOTONIC_RAW) / 1000000;
            Log.Information("ArgumentBuilder::DeriveKey() fixing up rawTickCount from {0} to {1} on macOS",
                            rawTickCount, rawTickCountFixed);
            rawTickCount = (uint)rawTickCountFixed;
        }

        var ticks = rawTickCount & 0xFFFF_FFFFu;
        var key = ticks & 0xFFFF_0000u;
        gameArguments.Insert(0, $"T={ticks}");

        var escapeValue = (string x) => x.Replace(" ", "  ");
        var gameArgumentString = gameArguments.Select(x => x.Split('=', 2))
                                              .Aggregate(new StringBuilder(),
                                                         (whole, part) =>
                                                             whole.Append(
                                                                 $" /{escapeValue(part[0])} ={escapeValue(part.Length > 1 ? part[1] : string.Empty)}"))
                                              .ToString();
        var bf = new LegacyBlowfish(Encoding.UTF8.GetBytes($"{key:x08}"));
        var ciphertext = bf.Encrypt(Encoding.UTF8.GetBytes(gameArgumentString));
        var base64Str = Convert.ToBase64String(ciphertext).Replace('+', '-').Replace('/', '_').Replace('=', '*');
        var checksum = EncryptionChecksumTable[(int)(key >> 16) & 0xF];
        return $"//**sqex0003{base64Str}{checksum}**//";
    }
}
