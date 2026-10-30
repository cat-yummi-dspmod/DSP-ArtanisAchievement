using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ArtanisAchievement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CatAPI.HarnessPlugin.PluginGuid)]
    [BepInDependency(CustomAchievements.Plugin.PluginGuid)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.yummi.dsp.artanisachievement";
        public const string PluginName = "Artanis Achievement";
        public const string PluginVersion = "0.0.0";
        public const string GitHubUrl = "https://github.com/cat-yummi-dspmod/DSP-ArtanisAchievement";

        private static readonly Harmony harmony = new Harmony(PluginGuid);

        internal static ManualLogSource Log;

        private bool trackerFailed;

        private void Awake()
        {
            Log = Logger;
            Achievements.Register();
            CatAPI.Harness.PatchAll(harmony, typeof(CombatHooks), new CatAPI.PatchOptions { ModName = PluginName });
            PatchCheck.Report(harmony);
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded");
        }

        private void Update()
        {
            if (trackerFailed)
            {
                return;
            }
            try
            {
                Tracker.Update();
            }
            catch (Exception e)
            {
                trackerFailed = true;
                Log.LogError("Achievement tracking stopped after an error: " + e);
                CatAPI.Harness.Report(CatAPI.NoticeLevel.Error, PluginName,
                    "成就判定出错，已停止，详情见日志", "Achievement tracking failed and stopped; see the log");
            }
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
