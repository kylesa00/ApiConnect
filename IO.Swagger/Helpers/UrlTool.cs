using IO.Swagger.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IO.Swagger.Helpers
{
    public static class UrlTool
    {
        private const string PrefixToRemove = "/apps/prod-webshop-service-app";

        public static string TrimAppPrefix(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            return url.StartsWith(PrefixToRemove, StringComparison.OrdinalIgnoreCase)
                ? url.Substring(PrefixToRemove.Length)
                : url;
        }

        public static Dictionary<string, LinkEntry> ParseLinks(Dictionary<string, LinkEntry> links)
        {
            foreach (var key in links.Keys)
            {
                links[key].Href = TrimAppPrefix(links[key].Href);

            }
            return links;
        }

    }
}
