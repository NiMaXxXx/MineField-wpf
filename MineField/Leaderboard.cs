using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace курсач
{
    public class LeaderRecord
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Seconds { get; set; }
    }

    public static class Leaderboard
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MineField", "Records.txt");

        public static List<LeaderRecord> Best(IEnumerable<LeaderRecord> records)
        {
            return records
                .Where(r => !string.IsNullOrWhiteSpace(r.Name) && r.Level >= 1 && r.Seconds >= 0)
                .GroupBy(r => r.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderByDescending(r => r.Level).ThenBy(r => r.Seconds).First())
                .OrderByDescending(r => r.Level)
                .ThenBy(r => r.Seconds)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Take(10).ToList();
        }

        public static List<LeaderRecord> Parse(IEnumerable<string> lines)
        {
            var records = new List<LeaderRecord>();
            foreach (string line in lines)
            {
                var match = Regex.Match(line, @"^(.*?)\s+(\d+)\s+(\d+)\s+сек\s*$");
                int level, seconds;
                if (match.Success && int.TryParse(match.Groups[2].Value, out level)
                    && int.TryParse(match.Groups[3].Value, out seconds))
                    records.Add(new LeaderRecord { Name = match.Groups[1].Value.Trim(), Level = level, Seconds = seconds });
            }
            return Best(records);
        }

        public static List<LeaderRecord> Load()
        {
            if (File.Exists(FilePath)) return Parse(File.ReadAllLines(FilePath));
            // Import previous versions' records on the first use.
            var legacyPaths = new[] {
                Path.Combine(Environment.CurrentDirectory, "Records.txt"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Records.txt")
            }.Distinct(StringComparer.OrdinalIgnoreCase);
            return Parse(legacyPaths.Where(File.Exists).SelectMany(File.ReadAllLines));
        }

        public static void Save(string name, int level, int seconds)
        {
            var records = Load();
            records.Add(new LeaderRecord { Name = name.Trim(), Level = level, Seconds = seconds });
            var lines = Best(records).Select(r => $"{r.Name}   {r.Level}   {r.Seconds} сек");
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllLines(FilePath, lines);
        }
    }
}
