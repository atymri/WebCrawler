using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Threading;
using Colorful;
using Console = Colorful.Console;
using WebCrawler.Utils;

namespace WebCrawler.Services
{
    public class QueueCrawlerService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HashSet<string> _visited = new HashSet<string>();
        private readonly Queue<string> _queue = new Queue<string>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(50); // حداکثر 5 درخواست همزمان
        private readonly UrlHelper _urlHelper;
        public QueueCrawlerService(UrlHelper helper)
        {
            _urlHelper = helper;
        }

        public IReadOnlyCollection<string> VisitedUrls => _visited;

        public void EnqueueUrl(string url)
        {
            if (_urlHelper.IsUrlAllowed(url) && !_visited.Contains(url) && !_queue.Contains(url))
            {
                _queue.Enqueue(url);
            }
        }

        public async Task CrawlAsync(string startUrl)
        {
            EnqueueUrl(startUrl);

            while (_queue.Count > 0)
            {
                string currentUrl = _queue.Dequeue();

                await _semaphore.WaitAsync();
                try
                {
                    await ProcessUrlAsync(currentUrl);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task ProcessUrlAsync(string url)
        {
            if (_visited.Contains(url))
                return;

            _visited.Add(url);
            Console.WriteLine($"Visiting: {url}", System.Drawing.Color.LightGreen);

            try
            {
                var response = await _httpClient.GetAsync(url);
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

                        Uri baseUri = new Uri(url);
                        if (Uri.TryCreate(baseUri, hrefValue, out Uri resultUri))
                        {
                            string absoluteUrl = resultUri.ToString();
                            EnqueueUrl(absoluteUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error visiting {url}: {ex.Message}", System.Drawing.Color.Red);
            }
        }
    }
}
