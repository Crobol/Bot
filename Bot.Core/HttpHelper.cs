using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Bot.Core
{
    /// <summary>
    /// Helper functions for dealing with HTTP.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// GET HTML from specified URL.
        /// </summary>
        /// <param name="url">Url to download from.</param>
        /// <returns>Returns string containing HTML on success.</returns>
        public static string GetFromUrl(string url)
        {
            WebClient webClient = new WebClient();
            webClient.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.CacheIfAvailable);
            webClient.Proxy = null;
            webClient.Encoding = System.Text.Encoding.UTF8;
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return webClient.DownloadString(url);
        }
    }
}
