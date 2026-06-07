using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MakoMiniPlayer
{
    public class SubtitleEntry
    {
        public long StartMs { get; set; }
        public long EndMs { get; set; }
        public string Text { get; set; }
    }

    public static class SubtitleParser
    {
        public static List<SubtitleEntry> Parse(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                var text = DetectAndDecode(bytes);
                var ext = Path.GetExtension(filePath).ToLower();

                if (ext == ".ass" || ext == ".ssa")
                    return ParseAss(text);
                else
                    return ParseSrt(text);
            }
            catch
            {
                return new List<SubtitleEntry>();
            }
        }

        private static string DetectAndDecode(byte[] bytes)
        {
            // BOM detection
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes).TrimStart('\uFEFF');
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(bytes).TrimStart('\uFEFF');
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

            // Detect Balkan UTF-8
            bool hasUtf8Balkan = false;
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                int b0 = bytes[i] & 0xFF;
                int b1 = bytes[i + 1] & 0xFF;
                if ((b0 == 0xC5 && (b1 == 0xA0 || b1 == 0xA1 || b1 == 0xBD || b1 == 0xBE)) ||
                    (b0 == 0xC4 && (b1 == 0x8C || b1 == 0x8D || b1 == 0x86 || b1 == 0x87 || b1 == 0x90 || b1 == 0x91)))
                {
                    hasUtf8Balkan = true;
                    break;
                }
            }

            // Detect Windows-1250
            bool hasWin1250 = bytes.Any(b =>
            {
                int u = b & 0xFF;
                return u == 0x8A || u == 0x9A || u == 0x8E || u == 0x9E ||
                       u == 0xC8 || u == 0xE8 || u == 0xC6 || u == 0xE6 ||
                       u == 0xD0 || u == 0xF0;
            });

            if (hasUtf8Balkan)
                return Encoding.UTF8.GetString(bytes);
            if (hasWin1250)
                return Encoding.GetEncoding("Windows-1250").GetString(bytes);

            try
            {
                var utf8 = Encoding.UTF8.GetString(bytes);
                if (!utf8.Contains('\uFFFD')) return utf8;
            }
            catch { }

            return Encoding.GetEncoding("Windows-1250").GetString(bytes);
        }

        private static List<SubtitleEntry> ParseSrt(string text)
        {
            var entries = new List<SubtitleEntry>();
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            var blocks = text.Trim().Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in blocks)
            {
                var lines = block.Trim().Split('\n');
                if (lines.Length < 2) continue;

                var timeLine = lines.FirstOrDefault(l => l.Contains("-->"));
                if (timeLine == null) continue;

                var parts = timeLine.Split(new[] { "-->" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;

                var startMs = ParseTimestamp(parts[0].Trim());
                var endMs = ParseTimestamp(parts[1].Trim().Split(' ')[0]);
                if (startMs < 0 || endMs < 0 || endMs <= startMs) continue;

                var timeIndex = Array.IndexOf(lines, timeLine);
                var subText = string.Join("\n", lines.Skip(timeIndex + 1).Where(l => l.Trim().Length > 0));
                subText = System.Text.RegularExpressions.Regex.Replace(subText, "<[^>]*>", "");
                subText = System.Text.RegularExpressions.Regex.Replace(subText, "\\{[^}]*\\}", "");
                subText = subText.Trim();

                if (subText.Length > 0)
                    entries.Add(new SubtitleEntry { StartMs = startMs, EndMs = endMs, Text = subText });
            }

            return entries.OrderBy(e => e.StartMs).ToList();
        }

        private static List<SubtitleEntry> ParseAss(string text)
        {
            var entries = new List<SubtitleEntry>();
            bool inEvents = false;
            int startIdx = -1, endIdx = -1, textIdx = -1;

            foreach (var rawLine in text.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line == "[Events]") { inEvents = true; continue; }
                if (!inEvents) continue;

                if (line.StartsWith("Format:"))
                {
                    var cols = line.Substring(7).Split(',').Select(c => c.Trim()).ToList();
                    startIdx = cols.IndexOf("Start");
                    endIdx = cols.IndexOf("End");
                    textIdx = cols.IndexOf("Text");
                    continue;
                }

                if (line.StartsWith("Dialogue:") && startIdx >= 0)
                {
                    var cols = line.Substring(9).Split(new[] { ',' }, textIdx + 1);
                    if (cols.Length <= textIdx) continue;

                    var startMs = ParseAssTimestamp(cols[startIdx].Trim());
                    var endMs = ParseAssTimestamp(cols[endIdx].Trim());
                    var subText = cols[textIdx].Trim();
                    subText = System.Text.RegularExpressions.Regex.Replace(subText, "\\{[^}]*\\}", "");
                    subText = subText.Replace("\\N", "\n").Replace("\\n", "\n");

                    if (startMs >= 0 && endMs > startMs && subText.Length > 0)
                        entries.Add(new SubtitleEntry { StartMs = startMs, EndMs = endMs, Text = subText });
                }
            }

            return entries.OrderBy(e => e.StartMs).ToList();
        }

        private static long ParseTimestamp(string ts)
        {
            try
            {
                var clean = ts.Trim().Replace(",", ".");
                var parts = clean.Split(':');
                if (parts.Length != 3) return -1;
                long hours = long.Parse(parts[0]);
                long minutes = long.Parse(parts[1]);
                var secMs = parts[2].Split('.');
                long seconds = long.Parse(secMs[0]);
                long ms = secMs.Length > 1 ? long.Parse(secMs[1].PadRight(3, '0').Substring(0, 3)) : 0;
                return (hours * 3600 + minutes * 60 + seconds) * 1000 + ms;
            }
            catch { return -1; }
        }

        private static long ParseAssTimestamp(string ts)
        {
            try
            {
                var parts = ts.Split(':');
                if (parts.Length != 3) return -1;
                long hours = long.Parse(parts[0]);
                long minutes = long.Parse(parts[1]);
                var secCs = parts[2].Split('.');
                long seconds = long.Parse(secCs[0]);
                long cs = secCs.Length > 1 ? long.Parse(secCs[1]) : 0;
                return (hours * 3600 + minutes * 60 + seconds) * 1000 + cs * 10;
            }
            catch { return -1; }
        }

        public static string? FindSubtitleInDir(string videoPath)
        {
            var videoDir = Path.GetDirectoryName(videoPath);
            var videoNameNoExt = Path.GetFileNameWithoutExtension(videoPath).ToLower();
            if (videoDir == null) return null;

            var extensions = new[] { ".srt", ".ass", ".sub" };

            // Traži u istom folderu
            foreach (var ext in extensions)
            {
                var direct = Path.Combine(videoDir, Path.GetFileNameWithoutExtension(videoPath) + ext);
                if (File.Exists(direct)) return direct;
            }

            // Fuzzy match u istom folderu
            var allSubFiles = Directory.GetFiles(videoDir)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            foreach (var subFile in allSubFiles)
            {
                var subName = Path.GetFileNameWithoutExtension(subFile).ToLower();
                if (subName == videoNameNoExt ||
                    videoNameNoExt.Contains(subName) ||
                    subName.Contains(videoNameNoExt) ||
                    Similarity(videoNameNoExt, subName) >= 0.85f)
                    return subFile;
            }

            // Traži u Subs/ subfolderu
            var subFolders = new[] { "Subs", "subs", "Subtitles", "subtitles", "Sub", "sub" };
            foreach (var folder in subFolders)
            {
                var subDir = Path.Combine(videoDir, folder);
                if (!Directory.Exists(subDir)) continue;

                foreach (var ext in extensions)
                {
                    var direct = Path.Combine(subDir, Path.GetFileNameWithoutExtension(videoPath) + ext);
                    if (File.Exists(direct)) return direct;
                }

                var subFiles = Directory.GetFiles(subDir)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                foreach (var subFile in subFiles)
                {
                    var subName = Path.GetFileNameWithoutExtension(subFile).ToLower();
                    if (subName == videoNameNoExt ||
                        videoNameNoExt.Contains(subName) ||
                        subName.Contains(videoNameNoExt) ||
                        Similarity(videoNameNoExt, subName) >= 0.85f)
                        return subFile;
                }
            }

            return null;
        }

        private static float Similarity(string a, string b)
        {
            if (a == b) return 1f;
            if (a.Length == 0 || b.Length == 0) return 0f;
            var longer = a.Length > b.Length ? a : b;
            var shorter = a.Length > b.Length ? b : a;
            int common = shorter.Count(c => longer.Contains(c));
            return (float)common / longer.Length;
        }
    }
}