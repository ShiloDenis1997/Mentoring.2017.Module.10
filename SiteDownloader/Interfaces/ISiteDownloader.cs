namespace SiteDownloader.Interfaces
{
    public interface ISiteDownloader
    {
        int MaxDeepLevel { get; set; }
        void LoadFromUrl(string url);
    }
}