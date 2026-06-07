using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MakoMiniPlayer
{
    public partial class SubtitleSearchWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly string _videoPath;
        private string _osToken = "";
        private List<SearchResult> _results = new();

        private static readonly HttpClient Http = new(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        public string? DownloadedSubtitlePath { get; private set; }

        private readonly List<(string Name, string Code)> _languages = new()
        {
            ("Svi jezici", ""),
            ("Bosanski", "bs"),
            ("Hrvatski", "hr"),
            ("Srpski", "sr"),
            ("Slovenački", "sl"),
            ("Engleski", "en"),
            ("Njemački", "de"),
            ("Francuski", "fr"),
            ("Španski", "es"),
            ("Talijanski", "it"),
            ("Portugalski", "pt"),
            ("Ruski", "ru"),
            ("Poljski", "pl"),
            ("Češki", "cs"),
            ("Mađarski", "hu"),
            ("Rumunjski", "ro"),
            ("Turski", "tr"),
            ("Arapski", "ar"),
            ("Japanski", "ja"),
            ("Kineski", "zh"),
            ("Korejski", "ko")
        };

        private readonly List<(string Name, string Code)> _encodings = new()
        {
            ("Auto-detect", "auto"),
            ("UTF-8", "utf-8"),
            ("Windows-1250 (Balkan)", "windows-1250"),
            ("Windows-1252 (Western)", "windows-1252"),
            ("ISO-8859-1", "iso-8859-1"),
            ("ISO-8859-2", "iso-8859-2"),
            ("UTF-16", "utf-16"),
        };

        public SubtitleSearchWindow(AppSettings settings, string suggestedTitle = "",
            string videoPath = "")
        {
            InitializeComponent();
            _settings = settings;
            _videoPath = videoPath;

            foreach (var lang in _languages)
                LstLanguages.Items.Add(lang.Name);

            LstLanguages.SelectedItems.Add(LstLanguages.Items[1]); // bs
            LstLanguages.SelectedItems.Add(LstLanguages.Items[2]); // hr
            LstLanguages.SelectedItems.Add(LstLanguages.Items[3]); // sr

            foreach (var enc in _encodings)
                CmbEncoding.Items.Add(enc.Name);
            CmbEncoding.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(suggestedTitle))
                TxtQuery.Text = suggestedTitle;
            else if (!string.IsNullOrEmpty(settings.LastSearchTitle))
                TxtQuery.Text = settings.LastSearchTitle;

            LstResults.SelectionChanged += (s, e) =>
                BtnDownload.IsEnabled = LstResults.SelectedIndex >= 0;
        }

        private string GetSelectedLanguages()
        {
            return string.Join(",",
                LstLanguages.SelectedItems.Cast<string>()
                    .Select(name => _languages.First(l => l.Name == name).Code)
                    .Where(c => !string.IsNullOrEmpty(c)));
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var query = TxtQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                TxtStatus.Text = "Upiši naziv filma ili serije!";
                return;
            }

            _settings.LastSearchTitle = query;
            _settings.Save();

            int? season = int.TryParse(TxtSeason.Text.Trim(), out int s) ? s : null;
            int? episode = int.TryParse(TxtEpisode.Text.Trim(), out int ep) ? ep : null;
            var langCode = GetSelectedLanguages();

            BtnSearch.IsEnabled = false;
            TxtStatus.Text = "Pretraživanje...";
            LstResults.Items.Clear();
            _results.Clear();
            BtnDownload.IsEnabled = false;
            BtnDownload.Content = "⬇️ Preuzmi";

            try
            {
                if (RbOpenSub.IsChecked == true)
                    await SearchOpenSubtitles(query, season, episode, langCode);
                else
                    await SearchSubDL(query, season, episode, langCode);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Greška: {ex.Message}";
            }

            BtnSearch.IsEnabled = true;
        }

        // ── OpenSubtitles ─────────────────────────────────────────
        private async Task SearchOpenSubtitles(string query, int? season,
            int? episode, string lang)
        {
            if (string.IsNullOrEmpty(_settings.OsApiKey))
            {
                TxtStatus.Text = "Nedostaje API ključ. Klikni ⚙️ API.";
                return;
            }

            if (string.IsNullOrEmpty(_osToken))
            {
                TxtStatus.Text = "Prijava na OpenSubtitles...";
                _osToken = await LoginOpenSubtitles();
                if (string.IsNullOrEmpty(_osToken))
                {
                    TxtStatus.Text = "Prijava neuspješna. Provjeri API ključ, username i password.";
                    return;
                }
            }

            var url = $"https://api.opensubtitles.com/api/v1/subtitles?query={Uri.EscapeDataString(query)}";
            if (season.HasValue) url += $"&season_number={season}";
            if (episode.HasValue) url += $"&episode_number={episode}";
            if (!string.IsNullOrEmpty(lang)) url += $"&languages={lang}";
            url += "&per_page=30";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Api-Key", _settings.OsApiKey);
            request.Headers.Add("Authorization", $"Bearer {_osToken}");
            request.Headers.Add("User-Agent", "MakoMiniPlayer v2.0");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

            var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!json.TrimStart().StartsWith("{"))
            {
                TxtStatus.Text = $"Server greška ({(int)response.StatusCode}). Provjeri API ključ.";
                return;
            }

            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data))
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString() : "Nema rezultata.";
                TxtStatus.Text = msg;
                return;
            }

            foreach (var item in data.EnumerateArray())
            {
                var attrs = item.GetProperty("attributes");
                var name = attrs.TryGetProperty("release", out var rel)
                    ? rel.GetString() : "";
                if (string.IsNullOrEmpty(name))
                    name = attrs.TryGetProperty("feature_details", out var fd) &&
                           fd.TryGetProperty("movie_name", out var mn)
                        ? mn.GetString() : "Unknown";
                var langProp = attrs.TryGetProperty("language", out var l)
                    ? l.GetString() : "";
                var downloads = attrs.TryGetProperty("download_count", out var dl)
                    ? dl.GetInt32() : 0;

                int fileId = 0;
                if (attrs.TryGetProperty("files", out var files) &&
                    files.GetArrayLength() > 0)
                    fileId = files[0].GetProperty("file_id").GetInt32();

                _results.Add(new SearchResult { Source = "os", FileId = fileId });
                LstResults.Items.Add($"{name} [{langProp}] ↓{downloads}");
            }

            TxtStatus.Text = _results.Count > 0
                ? $"Pronađeno: {_results.Count}" : "Nema rezultata.";
        }

        private async Task<string> LoginOpenSubtitles()
        {
            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    username = _settings.OsUsername,
                    password = _settings.OsPassword
                });
                var request = new HttpRequestMessage(HttpMethod.Post,
                    "https://api.opensubtitles.com/api/v1/login");
                request.Headers.Add("Api-Key", _settings.OsApiKey);
                request.Headers.Add("User-Agent", "MakoMiniPlayer v2.0");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await Http.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (!json.TrimStart().StartsWith("{")) return "";

                var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("token", out var token)
                    ? token.GetString() ?? "" : "";
            }
            catch { return ""; }
        }

        private async Task DownloadOpenSubtitles(int fileId)
        {
            var body = JsonSerializer.Serialize(new { file_id = fileId });
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.opensubtitles.com/api/v1/download");
            request.Headers.Add("Api-Key", _settings.OsApiKey);
            request.Headers.Add("Authorization", $"Bearer {_osToken}");
            request.Headers.Add("User-Agent", "MakoMiniPlayer v2.0");
            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!json.TrimStart().StartsWith("{"))
            {
                TxtStatus.Text = $"Server greška ({(int)response.StatusCode}).";
                return;
            }

            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("remaining", out var rem) &&
                rem.GetInt32() <= 0)
            {
                TxtStatus.Text = "Dostignut dnevni limit preuzimanja!";
                return;
            }

            if (!doc.RootElement.TryGetProperty("link", out var linkProp))
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString() : "Nema download linka.";
                TxtStatus.Text = msg;
                return;
            }

            var dlUrl = linkProp.GetString()!;
            var dlRequest = new HttpRequestMessage(HttpMethod.Get, dlUrl);
            dlRequest.Headers.Add("User-Agent", "Mozilla/5.0");
            dlRequest.Headers.Add("Accept", "*/*");
            var dlResponse = await Http.SendAsync(dlRequest);
            var bytes = await dlResponse.Content.ReadAsByteArrayAsync();
            await SaveSubtitleBytes(bytes);
        }

        // ── SubDL ─────────────────────────────────────────────────
        private async Task SearchSubDL(string query, int? season,
            int? episode, string lang)
        {
            if (string.IsNullOrEmpty(_settings.SubDLApiKey))
            {
                TxtStatus.Text = "Nedostaje SubDL API ključ. Klikni ⚙️ API.";
                return;
            }

            var url = $"https://api.subdl.com/api/v1/subtitles?api_key={_settings.SubDLApiKey}&film_name={Uri.EscapeDataString(query)}";
            if (season.HasValue && episode.HasValue)
                url += $"&type=tv&season_number={season}&episode_number={episode}";
            else
                url += "&type=movie";
            if (!string.IsNullOrEmpty(lang))
                url += $"&languages={lang.ToUpper()}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "MakoMiniPlayer/2.0");
            request.Headers.Add("Accept", "application/json");

            var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!json.TrimStart().StartsWith("{"))
            {
                TxtStatus.Text = $"Server greška ({(int)response.StatusCode}).";
                return;
            }

            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("subtitles", out var subs))
            {
                TxtStatus.Text = "Nema rezultata.";
                return;
            }

            foreach (var item in subs.EnumerateArray())
            {
                var name = item.TryGetProperty("release_name", out var rn)
                    ? rn.GetString()
                    : item.TryGetProperty("name", out var n)
                        ? n.GetString() : "Unknown";
                var langProp = item.TryGetProperty("language", out var l)
                    ? l.GetString() : "";
                var urlPath = item.TryGetProperty("url", out var u)
                    ? u.GetString() : "";
                var fullUrl = urlPath?.StartsWith("http") == true
                    ? urlPath : $"https://dl.subdl.com{urlPath}";

                _results.Add(new SearchResult { Source = "subdl", DownloadUrl = fullUrl });
                LstResults.Items.Add($"{name} [{langProp}]");
            }

            TxtStatus.Text = _results.Count > 0
                ? $"Pronađeno: {_results.Count}" : "Nema rezultata.";
        }

        private async Task DownloadSubDL(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "MakoMiniPlayer/2.0");
            request.Headers.Add("Accept", "*/*");
            var response = await Http.SendAsync(request);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            await SaveSubtitleBytes(bytes);
        }

        // ── Save ──────────────────────────────────────────────────
        private async Task SaveSubtitleBytes(byte[] bytes)
        {
            // Folder i naziv na osnovu filma
            string targetDir;
            string baseName;

            if (!string.IsNullOrEmpty(_videoPath) && File.Exists(_videoPath))
            {
                targetDir = Path.GetDirectoryName(_videoPath)!;
                baseName = Path.GetFileNameWithoutExtension(_videoPath);
            }
            else
            {
                targetDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MakoMiniPlayer", "Subtitles");
                Directory.CreateDirectory(targetDir);
                baseName = System.Text.RegularExpressions.Regex
                    .Replace(_settings.LastSearchTitle, "[^a-zA-Z0-9._-]", "_");
            }

            var filePath = Path.Combine(targetDir, $"{baseName}.srt");

            // Raspakuj ZIP ako treba
            if (IsZip(bytes))
            {
                using var zip = new ZipArchive(new MemoryStream(bytes));
                bool found = false;
                foreach (var entry in zip.Entries)
                {
                    var ext = Path.GetExtension(entry.Name).ToLower();
                    if (ext == ".srt" || ext == ".ass" || ext == ".sub")
                    {
                        using var stream = entry.Open();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        bytes = ms.ToArray();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    TxtStatus.Text = "Nema SRT fajla u ZIP arhivi.";
                    return;
                }
            }

            // Primijeni encoding
            if (CmbEncoding.SelectedIndex > 0)
            {
                var encCode = _encodings[CmbEncoding.SelectedIndex].Code;
                try
                {
                    var enc = Encoding.GetEncoding(encCode);
                    var text = enc.GetString(bytes);
                    await File.WriteAllTextAsync(filePath, text, Encoding.UTF8);
                }
                catch
                {
                    await File.WriteAllBytesAsync(filePath, bytes);
                }
            }
            else
            {
                await File.WriteAllBytesAsync(filePath, bytes);
            }

            DownloadedSubtitlePath = filePath;
            TxtStatus.Text = $"Preuzeto: {Path.GetFileName(filePath)}";
            BtnDownload.Content = "✅ Preuzeto";
        }

        private static bool IsZip(byte[] bytes) =>
            bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B;

        // ── Download button ───────────────────────────────────────
        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (LstResults.SelectedIndex < 0) return;
            var result = _results[LstResults.SelectedIndex];

            BtnDownload.IsEnabled = false;
            TxtStatus.Text = "Preuzimanje...";

            try
            {
                if (result.Source == "os")
                    await DownloadOpenSubtitles(result.FileId);
                else
                    await DownloadSubDL(result.DownloadUrl!);

                if (DownloadedSubtitlePath != null)
                {
                    await Task.Delay(600);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Greška: {ex.Message}";
                BtnDownload.IsEnabled = true;
            }
        }

        // ── Settings ──────────────────────────────────────────────
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var win = new ApiSettingsWindow(_settings);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                _osToken = "";
                _settings.Save();
            }
        }

        private class SearchResult
        {
            public string Source { get; set; } = "";
            public int FileId { get; set; }
            public string? DownloadUrl { get; set; }
        }
    }
}