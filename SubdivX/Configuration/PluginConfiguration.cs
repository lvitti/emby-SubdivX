using MediaBrowser.Model.Plugins;

namespace SubdivX.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public bool UseOriginalTitle { get; set; } = false;
        public bool ShowTitleInResult { get; set; } = true;
        public bool ShowUploaderInResult { get; set; } = true;
    }
}
