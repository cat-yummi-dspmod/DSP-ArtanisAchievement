using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace ArtanisAchievement
{
    /// <summary>CatAPI 只记录失败的补丁；这里确认每个钩子都已挂上，并说明缺失时哪些成就不会推进。
    /// CatAPI only records failed patches; this confirms every hook is in place and names the achievements that stop working when one is missing.</summary>
    internal static class PatchCheck
    {
        struct Hook
        {
            public MethodBase Target;
            public string Name;
            public string Affects;
        }

        internal static void Report(Harmony harmony)
        {
            var hooks = new List<Hook>
            {
                new Hook { Target = AccessTools.Method(typeof(SpaceSector), nameof(SpaceSector.KillCraftFinal)), Name = "SpaceSector.KillCraftFinal", Affects = "My forces have suffered great loss, I must regroup my forces, Well fought" },
                new Hook { Target = AccessTools.Method(typeof(PlanetFactory), nameof(PlanetFactory.KillCraftFinally)), Name = "PlanetFactory.KillCraftFinally", Affects = "The battle has turned against my warriors, My warriors could not hold" },
                new Hook { Target = AccessTools.Method(typeof(SpaceSector), nameof(SpaceSector.KillEnemyFinal)), Name = "SpaceSector.KillEnemyFinal", Affects = "You have done well, Well fought" },
                new Hook { Target = AccessTools.Method(typeof(Mecha), nameof(Mecha.UpdateCombatStats)), Name = "Mecha.UpdateCombatStats", Affects = "Activating shields!" },
            };

            int applied = 0;
            foreach (Hook hook in hooks)
            {
                Patches info = hook.Target == null ? null : Harmony.GetPatchInfo(hook.Target);
                if (info != null && info.Owners.Contains(harmony.Id))
                {
                    applied++;
                    Plugin.Log.LogInfo("  hooked " + hook.Name);
                }
                else
                {
                    Plugin.Log.LogWarning("  NOT hooked " + hook.Name + " -> these achievements will not progress: " + hook.Affects);
                }
            }
            Plugin.Log.LogInfo($"Combat hooks: {applied}/{hooks.Count} applied");
        }
    }
}
