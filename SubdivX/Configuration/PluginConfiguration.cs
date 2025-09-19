using System.ComponentModel;
using Emby.Web.GenericEdit;
using MediaBrowser.Model.Attributes;

namespace SubdivX.Configuration
{
    public class PluginConfiguration : EditableOptionsBase
    {
        public override string EditorTitle => "SubdivX Options";

        [DisplayName("Use original titles")]
        public bool UseOriginalTitle { get; set; } = false;
        [DisplayName("Show title in result")]
        public bool ShowTitleInResult { get; set; } = true;
        [DisplayName("Show uploader in result")]
        public bool ShowUploaderInResult { get; set; } = true;

        public bool ShowDescriptionInResult { get; set; } = true;

        [DisplayName("SubX Api Token")] 
        [Description("Read the README to learn how to get your token <a target=\"_blank\" href='https://github.com/lvitti/emby-SubdivX/blob/master/README.md#subx-api-token'><i class=\"md-icon\">help</i></a>")]
        [IsPassword]
        public string Token { get; set; }
        public string SubXApiUrl { get; set; } = "https://subx-api.duckdns.org";
    }
}
