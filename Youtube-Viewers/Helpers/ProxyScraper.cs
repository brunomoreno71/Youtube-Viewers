using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Youtube_Viewers.Helpers
{
    class ProxyScraper
    {
        public static string[] Urls { get; private set; } = new string[] {
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
            "https://api.proxyscrape.com/?request=getproxies&proxytype=socks4&timeout=9000&ssl=yes",
            "https://www.proxy-list.download/api/v1/get?type=socks4"
        };

        public int Time { get; private set; } = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        public string FileName { get; private set; }

        public List<Proxy> Proxies { get; private set; }
        private Queue<Proxy> proxies;

        public ProxyScraper()
        {
            Scrape();
        }
        public ProxyScraper(string fileName)
        {
            FileName = fileName;
            Scrape();
        }

        public Proxy Next()
        {
            if (proxies.Count == 0 && Proxies.Count != 0)
                proxies = new Queue<Proxy>(Proxies);

            return proxies.Dequeue();
        }

        public void Scrape()
        {
            if (Program.proxyType != 0)
                fromFile();
            else
                fromUrls();

            proxies = new Queue<Proxy>(Proxies);
        }

        private void fromFile()
        {
            List<string> proxies = new List<string>();

            foreach (string proxy in File.ReadAllText(FileName).Trim().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                proxies.Add(proxy.ToLower().Trim());
            }

            Proxies = Proxy.GetList(proxies);
        }

        private void fromUrls()
        {
            List<string> proxies = new List<string>();
            using (HttpRequest req = new HttpRequest())
            {
                foreach (string url in Urls)
                {
                    try
                    {
                        string[] res = req.Get(url).ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        foreach (string proxy in res)
                            if (!proxies.Contains(proxy.Trim()))
                                proxies.Add(proxy.Trim());
                    }
                    catch { }
                }
            }

            Proxies = Proxy.GetList(proxies);
            Time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
