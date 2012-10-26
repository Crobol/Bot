﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Bot.Core
{
    public class HttpHelper
    {
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