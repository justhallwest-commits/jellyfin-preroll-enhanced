using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Plugin.PreRollEnhanced.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.PreRollEnhanced
{
    /// <summary>
    /// Enhanced Pre-Roll Videos Plugin - Plays pre-rolls before movies and TV shows on ALL clients
    /// Combines session-based universal client support with rich metadata selection rules
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "Pre-Roll Videos Enhanced";

        public override Guid Id => Guid.Parse("e4f5b6c7-d8a9-4b1c-2d3e-4f5a6b7c8d9e");

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = this.Name,
                    EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
                }
            };
        }
    }
}
