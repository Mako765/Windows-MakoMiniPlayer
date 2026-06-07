using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MakoMiniPlayer
{
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MakoMiniPlayer", "settings.json");

        public string OsApiKey { get; set; } = "";
        public string OsUsername { get; set; } = "";
        public string OsPassword { get; set; } = "";
        public string SubDLApiKey { get; set; } = "";

        public string Language { get; set; } = "hr";

        public float SubtitleSize { get; set; } = 32f;
        public bool SubtitleBold { get; set; } = false;
        public string SubtitleColor { get; set; } = "#FFFFFF";
        public bool SubtitleBackground { get; set; } = false;
        public double SubtitleBackgroundOpacity { get; set; } = 0.7;
        public bool SubtitleShadow { get; set; } = true;
        public double SubtitleShadowRadius { get; set; } = 6;
        public double SubtitleShadowOpacity { get; set; } = 0.9;
        public bool SubtitleOutline { get; set; } = true;
        public double SubtitleOutlineWidth { get; set; } = 2;
        public string SubtitleOutlineColor { get; set; } = "#000000";

        // Pozicija kao postotak visine video areala (0=dno, 95=vrh)
        public double SubtitleBottomMarginPct { get; set; } = 5;

        public string LastSearchTitle { get; set; } = "";
        public int Volume { get; set; } = 80;

        public List<string> RecentFiles { get; set; } = new();
        public Dictionary<string, long> SavedPositions { get; set; } = new();

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json)
                        ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                var json = JsonSerializer.Serialize(this,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        public void AddRecentFile(string path)
        {
            RecentFiles.Remove(path);
            RecentFiles.Insert(0, path);
            if (RecentFiles.Count > 10)
                RecentFiles.RemoveRange(10, RecentFiles.Count - 10);
        }

        public void SavePosition(string path, long positionMs)
        {
            if (positionMs < 5000) return;
            SavedPositions[path] = positionMs;
            if (SavedPositions.Count > 50)
            {
                var first = new List<string>(SavedPositions.Keys)[0];
                SavedPositions.Remove(first);
            }
        }

        public long GetSavedPosition(string path)
        {
            return SavedPositions.TryGetValue(path, out var pos) ? pos : 0;
        }
    }
}