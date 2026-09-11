using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QuanLyBar.Client.Services
{
    public static class ReportLayoutPersist
    {
        private static readonly object SyncRoot = new object();
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuanLyBar");
        private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "reportLayouts.json");

        public static void Save(string reportName, string stemName)
        {
            if (string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(stemName)) return;

            lock (SyncRoot)
            {
                try
                {
                    Directory.CreateDirectory(ConfigDirectory);
                    var layouts = ReadLayouts();
                    layouts[reportName.Trim()] = stemName.Trim();

                    string tempPath = ConfigPath + ".tmp";
                    string json = JsonSerializer.Serialize(layouts, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(tempPath, json);
                    File.Move(tempPath, ConfigPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ReportLayoutPersist.Save error: " + ex.Message);
                }
            }
        }

        public static string Load(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return "";

            lock (SyncRoot)
            {
                try
                {
                    var layouts = ReadLayouts();
                    return layouts.TryGetValue(reportName.Trim(), out string? stemName)
                        ? stemName
                        : "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ReportLayoutPersist.Load error: " + ex.Message);
                    return "";
                }
            }
        }

        private static Dictionary<string, string> ReadLayouts()
        {
            if (!File.Exists(ConfigPath))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string json = File.ReadAllText(ConfigPath);
            var savedLayouts = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
            return new Dictionary<string, string>(savedLayouts, StringComparer.OrdinalIgnoreCase);
        }
    }
}
