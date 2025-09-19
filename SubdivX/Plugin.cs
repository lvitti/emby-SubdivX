using SubdivX.Configuration;
using System;
using MediaBrowser.Common;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Logging;

namespace SubdivX
{
    public class Plugin : BasePluginSimpleUI<PluginConfiguration>
    {
        public Plugin(IApplicationHost applicationHost, ILogManager logManager) : base(applicationHost)
        {
            Instance = this;
        }

        public override Guid Id => new Guid("AA3B7CCD-BF23-4FE2-A9B9-69CCAD2E076B");
        public override string Name => "SubdivX";
        public override string Description => "Download subtitles for Movies and TV Shows using SubdivX";
        public static Plugin Instance { get; private set; }

        public virtual PluginConfiguration GetConfiguration()
        {
            return this.GetOptions();
        }
    }
}
