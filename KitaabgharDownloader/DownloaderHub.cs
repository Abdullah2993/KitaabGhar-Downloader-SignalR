using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using Kitaabghar;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace KitaabgharDownloader
{
    [HubName("downloaderHub")]
    public class DownloaderHub : Hub

    {
        public static ConcurrentDictionary<string, Kitaabghar.KitaabgharDownloader> ClientList = new ConcurrentDictionary<string, Kitaabghar.KitaabgharDownloader>();

        [HubMethodName("startDownload")]
        public void StartDownload(string link)
        {
            ClientList[Context.ConnectionId].Start(link);
        }

        public override Task OnConnected()
        {
            var downloader = new Kitaabghar.KitaabgharDownloader { ConnectionId = Context.ConnectionId };
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.StatusChanged += downloader_StatusChanged;
            ClientList.TryAdd(Context.ConnectionId, downloader);
            return base.OnConnected();
        }

        void downloader_StatusChanged(object sender, StatusChangedEventHandler e)
        {
            Clients.Client(((Kitaabghar.KitaabgharDownloader)sender).ConnectionId).statusChanged(e.Status);
        }

        void downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Clients.Client(((Kitaabghar.KitaabgharDownloader)sender).ConnectionId).progressChanged(e.ProgressPercentage);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Kitaabghar.KitaabgharDownloader client;
            ClientList.TryRemove(Context.ConnectionId, out client);
            client.Stop();
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var downloader = new Kitaabghar.KitaabgharDownloader();
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.StatusChanged += downloader_StatusChanged;
            ClientList.TryAdd(Context.ConnectionId, downloader);
            return base.OnReconnected();
        }
    }
}