using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Kitaabghar
{
    public static class KitaabGhar
    {
        private const string FirstIndexRegex = @"<option value=""(\d*?)"" Selected>Page # \1</option>";
        private const string LastIndexRegex = @"<option value=""(\d*?)"">Page # \1</option></select>";
        private const string RefLinkRegex = @"document.location.href = ""(.*?)"" \+ page;";
        private const string ImageLinkRegex = @"background=""(.*?)(\d*?)\.gif""";
        private const string AlternateImageLinkRegex = @"<td><img src=""(.*?)(\d*?)\.gif""";

        public static bool IsValidLink(string url)
        {
            var result = false;
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                if (url.ToLower().Contains("kitaabghar.") &&
                    (url.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase) ||
                     url.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase)))
                {
                    result = true;
                }
            }
            return result;
        }

        public static Novel GetNovelInformation(string link)
        {
            try
            {
                var prefix = link.Substring(0, link.IndexOf('/', 7));
                var path = link.Substring(0, link.LastIndexOf('/') + 1);
                string source;
                using (var wc = new WebClient())
                {
                    source = HttpUtility.HtmlDecode(wc.DownloadString(link));
                }
                var firstIndex = int.Parse(Regex.Match(source, FirstIndexRegex, RegexOptions.IgnoreCase).Groups[1].Value);
                var lastIndex = int.Parse(Regex.Match(source, LastIndexRegex).Groups[1].Value);
                var refLink = Regex.Match(source, RefLinkRegex).Groups[1].Value + "{0}";
                var resultImageVal = Regex.Match(source, ImageLinkRegex).Groups[1].Value;
                var newFormat = string.IsNullOrEmpty(resultImageVal);
                var imageLink = (newFormat
                                     ? Regex.Match(source, AlternateImageLinkRegex).Groups[1].Value
                                     : resultImageVal) + "{0}.gif";

                refLink = refLink.StartsWith("/") ? prefix + refLink : string.Format("{0}/{1}", prefix, refLink);

                imageLink = FixImageLink(imageLink, path);
                //TODO Smart ID
                if (true)
                {
                    using (var wc = new WebClient())
                    {
                        source = HttpUtility.HtmlDecode(wc.DownloadString(string.Format(refLink, 2)));
                    }
                    resultImageVal = Regex.Match(source, ImageLinkRegex).Groups[1].Value;
                    var imageLink2 = (newFormat
                                          ? Regex.Match(source, AlternateImageLinkRegex).Groups[1].Value
                                          : resultImageVal) + "{0}.gif";

                    imageLink2 = FixImageLink(imageLink2, path);
                    imageLink = SmartIdentifier(imageLink, imageLink2);
                }

                return new Novel(link, firstIndex, lastIndex, imageLink, refLink,newFormat);
            }
            catch (Exception)
            {
                throw new Exception("unable to gather information.");
            }
        }

        private static string SmartIdentifier(string link1, string link2)
        {
            if (link1.Length != link2.Length)
                throw new Exception("Smart identification method failed.");

            var index = 0;
            for (var i = 0; i < link1.Length; i++)
            {
                if (link1[i] == link2[i])
                    continue;

                index = i;
                break;
            }
            if (index == 0)
            {
                return link1;
            }

            var str = link1.Remove(index, 1);
            str = str.Insert(index, "{0}");
            return str;
        }

        private static string FixImageLink(string imageLink, string path)
        {
            if (!imageLink.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                if (imageLink.StartsWith("./"))
                {
                    imageLink = path + imageLink.Substring(2);
                }
                else if (imageLink.StartsWith("/"))
                {
                    imageLink = path + imageLink.Substring(1);
                }
                else
                {
                    imageLink = path + imageLink;
                }
            }
            return imageLink;
        }
    }
}