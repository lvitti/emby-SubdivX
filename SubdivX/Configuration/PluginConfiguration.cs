using System.ComponentModel;
using Emby.Web.GenericEdit;

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

        [DisplayName("FlareSolverr URL")] 
        public string FlareSolverrUrl { get; set; }
    }
}
