using BepInEx;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace LCGoodies
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "cloalkteorn.LCGoodies";
        private const string modName = "LCGoodies";
        private const string modVersion = "1.1.2.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static Plugin Instance;

        public static ManualLogSource ls;
        public static AssetBundle ab;
        public static ConfigFile config;

        public static ManualLogSource GetLogSource()
        {
            return ls;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            ls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            config = Config;
            PrepareNetworkPatching();

            //load asset bundle
            ab = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("LCGoodies.dll", "goodies"));

            Configuration.LoadConfig();
            CustomScrap.RegisterScrap();
            CustomTraps.RegisterTraps();

            // Plugin startup logic
            ls.LogInfo($"Plugin {modGUID} is loaded!");

            harmony.PatchAll(typeof(Plugin));
        }

        void PrepareNetworkPatching()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}