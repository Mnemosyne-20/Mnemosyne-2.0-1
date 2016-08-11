using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mnemosyne_Of_Mine
{
    static class ExportToCSV
    {
        static void ExportCSV(Dictionary<string, int> archiveCount, string name)
        {
            var csv = new StringBuilder();
            foreach(KeyValuePair<string, int> i in archiveCount)
            {
                csv.AppendLine($"{i.Key},{i.Value}");
            }
            File.WriteAllText($"./{name}", csv.ToString());
        }
    }
}
