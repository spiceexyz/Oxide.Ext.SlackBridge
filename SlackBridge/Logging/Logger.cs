using Oxide.Core;

namespace Oxide.Ext.SlackBridge.Logging
{
    public class Logger
    {
        public static void Log(string pluginTitle, string format, params object[] args)
        {
            Interface.Oxide.LogInfo("[{0}] {1}", pluginTitle, args.Length > 0 ? string.Format(format, args) : format);
        }

        public static void LogWarning(string pluginTitle, string format, params object[] args)
        {
            Interface.Oxide.LogWarning("[{0}] {1}", pluginTitle, args.Length > 0 ? string.Format(format, args) : format);
        }

        public static void LogError(string pluginTitle, string format, params object[] args)
        {
            Interface.Oxide.LogError("[{0}] {1}", pluginTitle, args.Length > 0 ? string.Format(format, args) : format);
        }

        public static void Puts(string pluginTitle, string format, params object[] args)
        {
            Interface.Oxide.LogInfo("[{0}] {1}", pluginTitle, args.Length > 0 ? string.Format(format, args) : format);
        }
    }
}
