using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCrawler.Utils
{
    public class UrlHelper
    {
        private readonly string _allowedDomain;
        private readonly List<string> _allowedExtensions;
        private readonly List<string> _includeKeywords;
        private readonly List<string> _excludeKeywords;

        public UrlHelper(
            string allowedDomain = null,
            List<string> allowedExtensions = null,
            List<string> includeKeywords = null,
            List<string> excludeKeywords = null)
        {
            _allowedDomain = allowedDomain?.ToLower();
            _allowedExtensions = allowedExtensions ?? new List<string>();
            _includeKeywords = includeKeywords ?? new List<string>();
            _excludeKeywords = excludeKeywords ?? new List<string>();
        }

        public bool IsUrlAllowed(string url)
        {
            var lowerUrl = url.ToLower();

            if (!string.IsNullOrEmpty(_allowedDomain) && !lowerUrl.Contains(_allowedDomain))
                return false;

            if (_allowedExtensions.Any() && !_allowedExtensions.Any(ext => lowerUrl.EndsWith(ext)))
                return false;

            if (_includeKeywords.Any() && !_includeKeywords.Any(k => lowerUrl.Contains(k)))
                return false;

            if (_excludeKeywords.Any() && _excludeKeywords.Any(k => lowerUrl.Contains(k)))
                return false;

            return true;
        }

       
    }
}
