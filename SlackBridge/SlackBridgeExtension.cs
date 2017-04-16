using System.Reflection;
using System.Collections.Generic;

using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Extensions;

using Oxide.Ext.SlackBridge.Web;
using Oxide.Ext.SlackBridge.Plugins;

namespace Oxide.Ext.SlackBridge
{
    public class SlackBridgeExtension : Extension
    {
        public override string Name => "SlackBridge";
        public override string Author => "Spicy";
        public override VersionNumber Version => new VersionNumber(1, 0, 0);

        Dictionary<CSPlugin, WebServer> pluginServerMap;

        public SlackBridgeExtension(ExtensionManager manager) : base(manager)
        {
        }

        public override void Load()
        {
            // Load System.Web.dll from embedded resource into memory
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Oxide.Ext.SlackBridge.Dependencies.System.Web.dll"))
            {
                var array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                Assembly.Load(array);
            }

            var slackChatSync = new SlackChatSync();
            Interface.Oxide.RootPluginManager.AddPlugin(slackChatSync);
            var slackChatSyncServer = new WebServer($"http://*:{slackChatSync.httpListenerPort}{slackChatSync.httpListenerDir}", slackChatSync.HandlePostRequest);
            slackChatSyncServer.Start();

            pluginServerMap.Add(slackChatSync, slackChatSyncServer);
        }

        public override void OnShutdown()
        {
            foreach (var pluginServer in pluginServerMap)
            {
                Interface.Oxide.RootPluginManager.RemovePlugin(pluginServer.Key);
                pluginServer.Value.Stop();
            }
        }
    }
}
