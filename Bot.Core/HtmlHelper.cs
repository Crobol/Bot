using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Bot.Core
{
    public class HtmlHelper
    {
        public static string GetFromUrl(string url)
        {
            WebClient webClient = new WebClient();
            webClient.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.CacheIfAvailable);
            webClient.Proxy = null;
            webClient.Encoding = System.Text.Encoding.UTF8;

            return webClient.DownloadString(url);
        }
    }
}
