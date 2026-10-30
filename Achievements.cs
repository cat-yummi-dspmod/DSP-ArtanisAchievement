using CustomAchievements;

namespace ArtanisAchievement
{
    internal static class Achievements
    {
        internal const int PrecisionDrone = 5102;
        internal const int AttackDrone = 5103;
        internal const int Corvette = 5111;
        internal const int Destroyer = 5112;

        internal const int ShieldTarget = 20000;
        internal const int LossTarget = 152;
        internal const int BurstKillTarget = 50;

        internal static Achievement ActivatingShields;
        internal static Achievement WellDone;
        internal static Achievement WellFought;
        internal static Achievement BattleTurned;
        internal static Achievement CouldNotHold;
        internal static Achievement GreatLosses;
        internal static Achievement MustRegroup;
        internal static Achievement EmbraceGlory;

        internal static void Register()
        {
            AchievementMod mod = AchievementRegistry.RegisterMod(Plugin.GitHubUrl,
                AchText.En("Artanis Achievement").ZhCN("荣耀贩子"));

            ActivatingShields = Add(mod, "activating-shields",
                AchText.En("Activating shields!").ZhCN("激活护盾！"),
                AchText.En("Icarus regenerates 20,000 shield within 10 seconds.").ZhCN("伊卡洛斯10秒内恢复20000点护盾值。"),
                ShieldTarget);

            WellDone = Add(mod, "well-done",
                AchText.En("You have done well.").ZhCN("大家做得很好"),
                AchText.En("Wipe out a Dark Fog space assault without touching the controls.").ZhCN("伊卡洛斯在没有任何操作的前提下，全灭一次太空黑雾的攻击。"));

            WellFought = Add(mod, "well-fought",
                AchText.En("Well fought.").ZhCN("漂亮的一仗"),
                AchText.En("Destroy a Dark Fog space core with no save loaded and no ship lost in the last 5 minutes.").ZhCN("在5分钟内没有读档且没有损失舰船的情况下，消灭一个太空黑雾核心。"));

            BattleTurned = Add(mod, "battle-turned",
                AchText.En("The battle has turned against my warriors.").ZhCN("战局对我们的战士太不利了"),
                AchText.En("Lose 152 Precision Drones within 5 minutes.").ZhCN("5分钟内阵亡152个精准无人机。"),
                LossTarget);

            CouldNotHold = Add(mod, "could-not-hold",
                AchText.En("My warriors could not hold.").ZhCN("我的战士们坚持不住了"),
                AchText.En("Lose 152 Attack Drones within 5 minutes.").ZhCN("5分钟内阵亡152个攻击无人机。"),
                LossTarget);

            GreatLosses = Add(mod, "great-losses",
                AchText.En("My forces have suffered great loss.").ZhCN("我的部队损失惨重"),
                AchText.En("Lose 152 Corvettes within 5 minutes.").ZhCN("5分钟内阵亡152个护卫舰。"),
                LossTarget);

            MustRegroup = Add(mod, "must-regroup",
                AchText.En("I must regroup my forces.").ZhCN("我得重新集结部队"),
                AchText.En("Lose 152 Destroyers within 5 minutes.").ZhCN("5分钟内阵亡152个驱逐舰。"),
                LossTarget);

            EmbraceGlory = Add(mod, "embrace-glory",
                AchText.En("We embrace the glory of battle.").ZhCN("拥抱战斗的荣耀"),
                AchText.En("Destroy 50 or more Dark Fog units with a single Energy Shell Burst.").ZhCN("用能量盾爆破一次性消灭50个以上黑雾单位。"),
                BurstKillTarget);
        }

        static Achievement Add(AchievementMod mod, string key, AchText name, AchText description, long target = 1)
        {
            AchIcon icon = AchIcon.FromEmbeddedPng(typeof(Achievements).Assembly, "ArtanisAchievement.icons." + key + ".png");
            return mod.Register(key, name, description, target, icon);
        }

        internal static Achievement ForFighter(int protoId)
        {
            switch (protoId)
            {
                case PrecisionDrone: return BattleTurned;
                case AttackDrone: return CouldNotHold;
                case Corvette: return GreatLosses;
                case Destroyer: return MustRegroup;
                default: return null;
            }
        }
    }
}
