using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.PreRollEnhanced
{
    /// <summary>
    /// Register plugin services with Jellyfin's dependency injection
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<PreRollManager>();
        }
    }

    /// <summary>
    /// Entry point to activate the pre-roll manager
    /// </summary>
    public class PreRollEntryPoint : IServerEntryPoint
    {
        private readonly PreRollManager _preRollManager;

        public PreRollEntryPoint(PreRollManager preRollManager)
        {
            _preRollManager = preRollManager;
        }

        public Task RunAsync()
        {
            // Manager is activated via constructor - session events are now subscribed
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _preRollManager?.Dispose();
        }
    }
}
