using System;
using System.Linq;
using System.Reflection;
using ColossalFramework.Plugins;

namespace ChooseYourRocket
{
    public static class Util
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            if (field == null)
            {
                throw new Exception(string.Format("Type '{0}' doesn't have field '{1}", type, fieldName));
            }
            return field.GetValue(instance);
        }

        public static bool IsModActive(ulong modId)
        {
            try
            {
                var plugins = PluginManager.instance.GetPluginsInfo();
                return plugins.Any(p => p != null && p.isEnabled && p.publishedFileID.AsUInt64 == modId);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to detect if mod {modId} is active");
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }
        
        public static string CleanName(string name)
        {
            return name.Substring(name.IndexOf('.') + 1).Replace("_Data", "");
        }
    }
}