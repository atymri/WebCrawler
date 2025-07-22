using System;
using System.Collections.Generic;
using System.IO;

namespace WebCrawler.Utils
{
    public static class FileHelper
    {
        public static void SaveLinksToFile(IEnumerable<string> links, string filename)
        {
            try
            {
                File.WriteAllLines(filename, links);
                Console.WriteLine($"Links saved to {filename}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to save links: {e.Message}");
            }
        }
    }
}
