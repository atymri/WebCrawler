using System;
using System.Threading.Tasks;
using Colorful;
using WebCrawler.Services;
using WebCrawler.Utils;
using Console = Colorful.Console;

class Program
{
    static async Task Main()
    {
        Console.Clear();
        Console.WriteAscii("Web Crawler", System.Drawing.Color.LimeGreen);

        Console.WriteLine("Enter the starting URL:", System.Drawing.Color.Cyan);
        string startUrl = Console.ReadLine()?.Trim() ?? "";

        if (!Uri.IsWellFormedUriString(startUrl, UriKind.Absolute))
        {
            Console.WriteLine("Invalid URL format.", System.Drawing.Color.Red);
            return;
        }

        var urlHelper = GetUrlFilterSettingsFromUser();
        var crawler = new QueueCrawlerService(urlHelper);

        await crawler.CrawlAsync(startUrl);

        FileHelper.SaveLinksToFile(crawler.VisitedUrls, "crawled_links.txt");

        Console.WriteLine(new string('-', 40), System.Drawing.Color.Gray);
        Console.WriteLine($"Crawling completed!", System.Drawing.Color.LimeGreen);
        Console.WriteLine($"Total links visited: {crawler.VisitedUrls.Count}", System.Drawing.Color.Yellow);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    public static UrlHelper GetUrlFilterSettingsFromUser()
    {
        Console.Write("Enter allowed domain (e.g. example.com) or leave empty for no domain filter: ");
        string allowedDomain = Console.ReadLine()?.Trim();

        Console.Write("Enter allowed extensions separated by commas (e.g. .html,.php,/) or leave empty: ");
        string allowedExtensionsInput = Console.ReadLine() ?? "";
        var allowedExtensions = allowedExtensionsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        Console.Write("Enter include keywords separated by commas (e.g. /blog/,/product/) or leave empty: ");
        string includeKeywordsInput = Console.ReadLine() ?? "";
        var includeKeywords = includeKeywordsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        Console.Write("Enter exclude keywords separated by commas (e.g. logout,session) or leave empty: ");
        string excludeKeywordsInput = Console.ReadLine() ?? "";
        var excludeKeywords = excludeKeywordsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        bool noFilters = string.IsNullOrEmpty(allowedDomain)
                 && !allowedExtensions.Any()
                 && !includeKeywords.Any()
                 && !excludeKeywords.Any();

        if (noFilters)
        {
            Console.WriteLine("No filters applied, crawling all URLs.", System.Drawing.Color.Yellow);
        }

        return new UrlHelper(
            allowedDomain: string.IsNullOrEmpty(allowedDomain) ? null : allowedDomain,
            allowedExtensions: allowedExtensions,
            includeKeywords: includeKeywords,
            excludeKeywords: excludeKeywords);
    }
}
