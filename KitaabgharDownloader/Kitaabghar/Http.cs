using System;
using System.Drawing;
using System.Net;

namespace Kitaabghar
{
    public static class Http
    {
        public static Image DownloadImage(string url, string referer,bool newFormat)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:22.0) Gecko/20100101 Firefox/22.0";
            request.Accept = "image/png,image/*;q=0.8,*/*;q=0.5";
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.KeepAlive = true;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Referer = referer;
            //TODO timeout
            request.Timeout = 60*1000;
            //request.Timeout = ApplicationSettings.Timeout*1000;

            var response = (HttpWebResponse) request.GetResponse();

            using (var stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    if (!newFormat)
                    {
                        return Image.FromStream(stream);
                    }

                    using (var uImage = Image.FromStream(stream))
                    {
                        var index = url.LastIndexOf('/') + 1;
                        url = url.Remove(index, 1);
                        url=url.Insert(index, "l");

                        using (var lImage=DownloadImage(url,referer,false))
                        {
                            url = url.Remove(index, 1);
                            url = url.Insert(index, "r");
                            using (var rImage=DownloadImage(url,referer,false))
                            {
                                return ImagePatch.PatchImages(uImage, lImage, rImage);
                            }

                        }
                    }
                }
            }

            throw new Exception("Unable to download image.");
        }
    }
}