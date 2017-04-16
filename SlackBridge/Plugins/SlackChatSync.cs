using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;

using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;

using Oxide.Ext.SlackBridge.Logging;

using Newtonsoft.Json;

namespace Oxide.Ext.SlackBridge.Plugins
{
    public class SlackChatSync : CSPlugin
    {
        static Covalence covalence;
        static WebRequests webRequest;
        static Lang lang;
        static Core.Configuration.DynamicConfigFile config;

        public string httpListenerPort;
        public string httpListenerDir;
        public string ingoingWebhookUrl;
        public string channelName;
        public string integrationName;
        public string integrationIcon;
        public List<string> whitelistedSlackUsers;

        public class Request
        {
            public string channel { get; set; }
            public string username { get; set; }
            public string text { get; set; }
            public string icon_emoji { get; set; }

            public Request(string channel, string username, string text, string icon_emoji)
            {
                this.channel = channel;
                this.username = username;
                this.text = text;
                this.icon_emoji = icon_emoji;
            }
        }

        [HookMethod("Init")]
        public void Init()
        {
            Title = "SlackChatSync";
            Author = "Spicy";
            Version = new VersionNumber(1, 0, 0);

            covalence = Interface.Oxide.GetLibrary<Covalence>();
            webRequest = Interface.Oxide.GetLibrary<WebRequests>();
            lang = Interface.Oxide.GetLibrary<Lang>();

            InitConfig();
            InitLang();
        }

        [HookMethod("OnUserChat")]
        public object OnUserChat(IPlayer player, string message)
        {
            webRequest.EnqueuePost(ingoingWebhookUrl,
                JsonConvert.SerializeObject(new Request(channelName, integrationName, string.Format(_("SlackMessage"), player.Id, player.Name, message), integrationIcon)),
                (code, response) => HandlePostCallback(code, response), this);

            return null;
        }

        [HookMethod("LoadDefaultConfig")]
        protected override void LoadDefaultConfig()
        {
            Config["Settings"] = new Dictionary<string, object>
            {
                ["HttpListenerPort"] = "16384",
                ["HttpListenerDir"] = "/chat",
                ["IngoingWebhookUrl"] = "https://hooks.slack.com/services/XXXXXXXXX/XXXXXXXXX/XXXXXXXXXXXXXXXXXXX",
                ["ChannelName"] = "#rust-chat",
                ["IntegrationName"] = "Chat",
                ["IntegrationIcon"] = ":sweat_drops:",
                ["WhitelistedSlackUsers"] = new List<string>
                {
                    "user1",
                    "user2",
                    "user3"
                }
            };
        }

        void InitConfig()
        {
            httpListenerPort = Config.Get<string>("Settings", "HttpListenerPort");
            httpListenerDir = Config.Get<string>("Settings", "HttpListenerDir");
            ingoingWebhookUrl = Config.Get<string>("Settings", "IngoingWebhookUrl");
            channelName = Config.Get<string>("Settings", "ChannelName");
            integrationName = Config.Get<string>("Settings", "IntegrationName");
            integrationIcon = Config.Get<string>("Settings", "IntegrationIcon");
            whitelistedSlackUsers = Config.Get<List<string>>("Settings", "WhitelistedSlackUsers");
        }

        void InitLang()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["SlackMessage"] = "<http://steamcommunity.com/profiles/{0}|{1}>: {2}",
                ["RustMessage"] = "{0} via Slack: {1}",
                ["ConsoleMessage"] = "{0} via Slack: {1}"
            }, this);
        }

        string _(string key, string userId = null) => lang.GetMessage(key, this, userId);

        public string HandlePostRequest(HttpListenerRequest request)
        {
            if (request.HttpMethod != "POST")
                return "Fail.";

            using (var reader = new StreamReader(request.InputStream))
            {
                var requestString = reader.ReadToEnd();
                var parsedQueryString = HttpUtility.ParseQueryString(requestString);
                var name = parsedQueryString["user_name"];
                var message = parsedQueryString["text"];
                var trigger = parsedQueryString["trigger_word"];
                var cut = trigger.Length + 1;

                if (!whitelistedSlackUsers.Contains(name))
                    return "Fail.";

                Logger.Puts(Title, _("ConsoleMessage"), name, message.Substring(cut));
                covalence.Server.Broadcast(string.Format(_("RustMessage"), name, message.Substring(cut)));
            }

            return "Success.";
        }

        void HandlePostCallback(int code, string response)
        {

        }
    }
}
