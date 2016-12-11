using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Mnemosyne_Of_Mine
{
    internal static class CSVhandling
    {
        static void ExportCSV(Dictionary<string, int> archiveCount, string name)
        {
            var csv = new StringBuilder();
            foreach (KeyValuePair<string, int> i in archiveCount)
            {
                csv.AppendLine($"{Convert.ToBase64String(Encoding.UTF8.GetBytes(i.Key))},{Convert.ToBase64String(Encoding.UTF8.GetBytes(i.Value.ToString()))}"); // html can contain commas
            }
            File.WriteAllText($"./{name}", csv.ToString());
        }
        public static Dictionary<string, int> ExportDay(Dictionary<string, int> archiveCount)
        {
            DateTime time = DateTime.Now.AddDays(-1);
            string name = $"./{time.Year}-{time.DayOfYear}ArchiveCount.csv";
            ExportCSV(archiveCount, name);
            return archiveCount;
        }
        public static Dictionary<string, int> ExportMonth(Dictionary<string, int> archiveCount)
        {
            ExportDay(archiveCount);
            DateTime time = DateTime.Now.AddDays(-1);
            int offset = DateTime.DaysInMonth(time.Year, time.Month);
            string name = $"./{time.Year}--{time.Month}ArchiveCount.csv";
            List<Dictionary<string, int>> imports = new List<Dictionary<string, int>>();
            for (int i = time.DayOfYear - offset; i <= time.DayOfYear; i++)
            {
                imports.Add(ReadArchiveCountTrackingCSV($"./{time.Year}-{i}ArchiveCount.csv"));
            }
            Dictionary<string, int> temp = imports[0];
            imports.RemoveAt(0);
            foreach (var e in imports)
            {
                temp = Merge(temp, e);
            }
            ExportCSV(temp, name);
            return temp;
        }
        public static Dictionary<string, int> ExportYear(Dictionary<string, int> archiveCount)
        {
            ExportMonth(archiveCount);
            DateTime time = DateTime.Now.AddDays(-1);
            string name = $"./{time.Year}ArchiveCount.csv";
            List<Dictionary<string, int>> imports = new List<Dictionary<string, int>>();
            for (int i = 1; i <= 12; i++)
            {
                imports.Add(ReadArchiveCountTrackingCSV($"./{time.Year}--{i}ArchiveCount.csv"));
            }
            Dictionary<string, int> temp = imports[0];
            imports.RemoveAt(0);
            foreach (var e in imports)
            {
                temp = Merge(temp, e);
            }
            ExportCSV(temp, name);
            return temp;
        }
        /// <summary>
        /// Top10, should be sorted by high -> low
        /// </summary>
        /// <param name="ArchiveCount">Whatchu' think?</param>
        /// <returns></returns>
        public static SortedDictionary<int, string> Top10(Dictionary<string, int> ArchiveCount)
        {
            KeyValuePair<string, int> max = new KeyValuePair<string, int>();
            SortedDictionary<int, string> Top10 = new SortedDictionary<int, string>();
            for (int i = 0; i < 10; i++)
            {
                max = ArchiveCount.Aggregate((l, r) => l.Value > r.Value ? l : r);
                Top10.Add(max.Value, max.Key);
                ArchiveCount.Remove(max.Key);
            }
            return Top10;
        }
        static Dictionary<string, int> ReadArchiveCountTrackingCSV(string file)
        {
            Dictionary<string, int> ArchiveCount = new Dictionary<string, int>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string originalURL = Encoding.UTF8.GetString(Convert.FromBase64String(elements[i]));
                int count = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(elements[i + 1])));
                ArchiveCount.Add(originalURL, count);
            }
            return ArchiveCount;
        }
        static Dictionary<string, int> Merge(Dictionary<string, int> older, Dictionary<string, int> newer)
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();
            foreach (var e in older)
            {
                if (newer.ContainsKey(e.Key))
                {
                    temp.Add(e.Key, older[e.Key] + newer[e.Key]);
                }
                else
                {
                    temp.Add(e.Key, e.Value);
                }
            }
            return temp;
        }
    }
}
