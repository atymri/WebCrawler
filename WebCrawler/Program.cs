using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Drawing;
using Console = Colorful.Console;
using System.Linq;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly HashSet<string> visited = new HashSet<string>();
    private static readonly List<string> foundLinks = new List<string>();

    static async Task Main(string[] args)
    {
        ClearScreen();
        DisplayBanner();

        Console.Write("Enter the starting URL: ");
        string startUrl = Console.ReadLine()?.Trim() ?? "";

        if (!Uri.IsWellFormedUriString(startUrl, UriKind.Absolute))
        {
            Console.WriteLine("Invalid URL format.", Color.Red);
            return;
        }

        await CrawlAsync(startUrl);

        SaveLinksToFile("crawled_links.txt");

        Console.WriteLine($"Crawling completed!", Color.Green);
        Console.WriteLine($"Total links visited: {visited.Count}", Color.LimeGreen);
    }

    static void ClearScreen()
    {
        Console.Clear();
    }

    static void DisplayBanner()
    {
       
        Console.WriteLine("Web Crawler - 1.0.0.0", Color.LimeGreen);
    }

    static async Task CrawlAsync(string url)
    {
        if (visited.Contains(url))
            return;

        visited.Add(url);
        Console.WriteLine($"Visiting: {url}", Color.DarkGray);

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var hrefValue = link.GetAttributeValue("href", string.Empty);
                    if (string.IsNullOrEmpty(hrefValue))
                        continue;

                    // Combine relative URLs with base
                    Uri baseUri = new Uri(url);
                    Uri resultUri;
                    if (Uri.TryCreate(baseUri, hrefValue, out resultUri))
                    {
                        string absoluteUrl = resultUri.ToString();
                        Console.WriteLine($"Found link: {absoluteUrl}", Color.Lime);
                        foundLinks.Add(absoluteUrl);

                        // Recursively crawl, but limit depth or domain if desired to avoid infinite crawling
                        await CrawlAsync(absoluteUrl);
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error: {e.Message}", Color.Red);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Timeout or canceled request for {url}", Color.Red);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}", Color.Red);
        }
    }

    static void SaveLinksToFile(string filename)
    {
        try
        {
            File.WriteAllLines(filename, visited);
            Console.WriteLine($"Links saved to {filename}", Color.Blue);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to save links: {e.Message}", Color.Red);
        }
    }
}
