using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using sharpPDF;

namespace Kitaabghar
{
    public class KitaabgharDownloader
    {
        private Novel _novel;
        private volatile bool _running;

        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler<StatusChangedEventHandler> StatusChanged;


        public string ConnectionId { get; set; }

        public int ProgressValue { get; set; }
        public string DownloadLocation { get; set; }

        public KitaabgharDownloader()
        {
            DownloadLocation = "~/";
            ProgressValue = 0;
        }

        public void Stop()
        {
            _running = false;
        }

        public void Start(string linkText)
        {
            if (!_running)
            {
                if (!string.IsNullOrEmpty(linkText.Trim()))
                {
                    linkText = linkText.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                                        ? linkText
                                        : "http://" + linkText;

                    if (KitaabGhar.IsValidLink(linkText.Trim()))
                    {
                        ProgressValue = 0;
                        LogMessage("Link: " + linkText);
                        LogMessage("Gathering information.");
                        //TODO Smart ID
                        if (true)
                        {
                            LogMessage("Smart identification is on so this may take a while.");
                        }

                        _running = true;
                        var mainThread = new Thread(Main) { IsBackground = true };
                        mainThread.Start(linkText);
                    }
                    else
                    {
                        LogMessage("Invalid download link.");
                    }
                }
                else
                {
                    LogMessage("Link missing.");
                }
            }
            else
            {
                LogMessage("Application is already running.");
            }
        }

        private void Main(object link)
        {
            try
            {
                _novel = KitaabGhar.GetNovelInformation(link.ToString());
                LogMessage("Information collected.");
                LogMessage(string.Format("Total pages: {0} ({1} - {2})", _novel.TotalPages,
                                         _novel.FirstIndex, _novel.LastIndex));

            }
            catch (Exception ex)
            {
                LogMessage("Error: " + ex.Message);
                _running = false;
                return;
            }

            var directory = System.Web.Hosting.HostingEnvironment.MapPath(Path.Combine(DownloadLocation, _novel.Name));
            if (directory == null)
            {
                LogMessage("Unable to get permission");
                return;
            }
            var imageFormat = Path.Combine(directory, "{0}.gif");
            Directory.CreateDirectory(directory);
            for (var i = _novel.FirstIndex; i <= _novel.LastIndex; i++)
            {
                LogMessage("Downloading page: " + i);

                var r = 0;
            Retry:
                if (!_running)
                {
                    return;
                }
                try
                {
                    //TODO retries
                    if (r <= 3)
                    {
                        //TODO resume
                        //if (!(true  && File.Exists(string.Format(imageFormat, i))))
                        if(!File.Exists(string.Format(imageFormat, i)))
                        {
                            var tempImage = Http.DownloadImage(_novel.GetImageLink(i), _novel.GetRefLink(i),
                                                               _novel.NewFormat);
                            tempImage.Save(string.Format(imageFormat, i));
                            tempImage.Dispose();
                        }
                        if (!_running)
                        {
                            return;
                        }
                        Progress((int)((double)((i - _novel.FirstIndex) * 100) / _novel.TotalPages));
                    }
                    else
                    {
                        LogMessage("Unable to download within the provided number of retries.");
                    }
                }
                catch
                    (Exception
                        ex)
                {
                    LogMessage("Unable to download page: " + i);
                    LogMessage("Trying to download page" + i + " again.");
                    LogMessage("Error: " + ex.Message);
                    r++;
                    goto Retry;
                }
            }
            LogMessage("Download complete.");
            //TODO ApplicationSettings.CreatePdf
            if (true)
            {
                LogMessage("Creating PDF file.");
                try
                {
                    using (var pdfFile = new pdfDocument(_novel.Name, "Abdullah Saleem"))
                    {
                        for (var i = _novel.FirstIndex; i <= _novel.LastIndex; i++)
                        {
                            var file = string.Format(imageFormat, i);
                            LogMessage("Processing page: " + i);
                            if (File.Exists(file))
                            {
                                try
                                {
                                    using (var image = Image.FromFile(file))
                                    {
                                        pdfFile.addImageReference(file, i.ToString(CultureInfo.InvariantCulture));
                                        var tempPage = pdfFile.addPage(image.Height, image.Width);
                                        tempPage.addImage(
                                            pdfFile.getImageReference(i.ToString(CultureInfo.InvariantCulture)), 0, 0);
                                    }
                                    LogMessage("Page " + i + " added.");
                                }
                                catch (Exception)
                                {
                                    LogMessage("Page number " + i + "missing.");
                                }
                            }
                            else
                            {
                                LogMessage("Page number " + i + "missing.");
                            }
                        }
                        pdfFile.createPDF(Path.Combine(directory, _novel.Name + ".pdf"));
                        LogMessage("PDF Created!");

                        //TODO ApplicationSettings.OpenPdf
                        //if (false)
                        //{
                        //    Process.Start(Path.Combine(directory, _novel.Name + ".pdf"));
                        //}
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Error occured while creating the PDF.");
                    LogMessage("Error: " + ex.Message);
                }
            }
            //TODO: ApplicationSettings.DeleteImages
            if (true)
            {
                LogMessage("Deleting images.");
                for (var i = _novel.FirstIndex; i <= _novel.LastIndex; i++)
                {
                    if (File.Exists(string.Format(imageFormat, i)))
                    {
                        try
                        {
                            File.Delete(string.Format(imageFormat, i));
                        }
                        catch (Exception ex)
                        {
                            LogMessage("Error occured while deleting images.");
                            LogMessage("Error: " + ex.Message);
                        }
                    }
                }
            }
            LogMessage("Completed.");
        }

        private void LogMessage(string message)
        {
            OnStatusChanged(new StatusChangedEventHandler(string.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), message)));
        }

        private void Progress(int percentage)
        {
            ProgressValue = percentage;
            OnProgressChanged(new ProgressChangedEventArgs(percentage,null));
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            var handler = ProgressChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnStatusChanged(StatusChangedEventHandler e)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, e);
        }
    }
}