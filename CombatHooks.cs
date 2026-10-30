using HarmonyLib;

namespace ArtanisAchievement
{
    [HarmonyPatch]
    internal static class CombatHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.KillCraftFinal))]
        [CatAPI.Guard("8d331b6d165290d0")]
        public static void SpaceSector_KillCraftFinal_Prefix(SpaceSector __instance, int craftId)
        {
            RecordFighterLoss(__instance.craftPool, craftId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.KillCraftFinally))]
        [CatAPI.Guard("4438112e7d007854")]
        public static void PlanetFactory_KillCraftFinally_Prefix(PlanetFactory __instance, int craftId)
        {
            RecordFighterLoss(__instance.craftPool, craftId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.KillEnemyFinal))]
        [CatAPI.Guard("cb31108bc5305076")]
        public static void SpaceSector_KillEnemyFinal_Prefix(SpaceSector __instance, int enemyId, ref CombatStat combatStat)
        {
            RecordMechaKill(__instance.enemyPool, enemyId, ref combatStat);
            EnemyData[] pool = __instance.enemyPool;
            if (enemyId <= 0 || pool == null || enemyId >= pool.Length)
            {
                return;
            }
            ref EnemyData enemy = ref pool[enemyId];
            if (enemy.id != enemyId)
            {
                return;
            }
            if (enemy.isAssaultingUnit)
            {
                CombatEvents.OnAssaultUnitKilled(enemyId);
            }
            if (enemy.dfSCoreId > 0)
            {
                EnemyDFHiveSystem hive = __instance.GetHiveByAstroId(enemy.originAstroId);
                CombatEvents.OnCoreKilled(hive?.starData?.id ?? 0);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.KillEnemyFinally))]
        [CatAPI.Guard("e83f0731e4b18ba0")]
        public static void PlanetFactory_KillEnemyFinally_Prefix(PlanetFactory __instance, int enemyId, ref CombatStat combatStat)
        {
            RecordMechaKill(__instance.enemyPool, enemyId, ref combatStat);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAction_Combat), nameof(PlayerAction_Combat.ShieldBurst))]
        [CatAPI.Guard("9442e06fa083a721")]
        public static void PlayerAction_Combat_ShieldBurst_Postfix(PlayerAction_Combat __instance)
        {
            if (__instance.player != null && __instance.player == GameMain.mainPlayer)
            {
                CombatEvents.OnShieldBurst();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.UpdateCombatStats))]
        [CatAPI.Guard("e0c3d2d8aa654f22")]
        public static void Mecha_UpdateCombatStats_Prefix(Mecha __instance, out long __state)
        {
            __state = __instance.energyShieldEnergy;
        }

        /// <summary>UpdateCombatStats 里护盾只会因回复而增加，伤害在别处扣除，所以前后差值就是这一帧的回复量。
        /// Inside UpdateCombatStats the shield only goes up from regeneration (damage is applied elsewhere), so the difference is this tick's regen.</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.UpdateCombatStats))]
        [CatAPI.Guard("e0c3d2d8aa654f22")]
        public static void Mecha_UpdateCombatStats_Postfix(Mecha __instance, long __state)
        {
            long gain = __instance.energyShieldEnergy - __state;
            if (gain > 0 && __instance.player != null && __instance.player == GameMain.mainPlayer)
            {
                CombatEvents.OnShieldGain(gain);
            }
        }

        /// <summary>致命一击来自伊卡洛斯（caster 为 Player 1）的黑雾击杀。 / A Dark Fog kill whose last hit came from Icarus (caster Player 1).</summary>
        static void RecordMechaKill(EnemyData[] pool, int enemyId, ref CombatStat combatStat)
        {
            if (enemyId <= 0 || pool == null || enemyId >= pool.Length || pool[enemyId].id != enemyId)
            {
                return;
            }
            if (combatStat.lastCaster.type == ETargetType.Player && combatStat.lastCaster.id == 1)
            {
                CombatEvents.OnMechaKill();
            }
        }

        /// <summary>战斗机的 owner 是所属编队（一个 craft），编队的 owner &lt; 0 表示伊卡洛斯，&gt; 0 表示战场分析基站等建筑。
        /// A fighter's owner is its fleet craft; the fleet's owner is &lt; 0 for Icarus and &gt; 0 for buildings such as battlefield analysis bases.</summary>
        static void RecordFighterLoss(CraftData[] pool, int craftId)
        {
            if (craftId <= 0 || pool == null || craftId >= pool.Length)
            {
                return;
            }
            ref CraftData craft = ref pool[craftId];
            if (craft.id != craftId || craft.unitId <= 0 || craft.prototype != ECraftProto.Item)
            {
                return;
            }
            int fleet = craft.owner;
            if (fleet <= 0 || fleet >= pool.Length)
            {
                return;
            }
            int side = pool[fleet].owner;
            if (side == 0)
            {
                return;
            }
            CombatEvents.OnFighterLost(craft.protoId, side < 0 && craft.isSpace);
        }
    }
}
