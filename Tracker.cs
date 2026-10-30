using System.Collections.Generic;
using CustomAchievements;
using UnityEngine;

namespace ArtanisAchievement
{
    /// <summary>主线程上每帧调用，按游戏 tick（60/秒）判定所有成就。
    /// Called every frame on the main thread; judges every achievement in game ticks (60 per second).</summary>
    internal static class Tracker
    {
        const long ShieldWindow = 10 * 60;
        const long LossWindow = 5 * 60 * 60;
        const long ScanInterval = 10;
        const int FreshWaveTicks = 300;

        static readonly int[] fighterTypes =
        {
            Achievements.PrecisionDrone, Achievements.AttackDrone, Achievements.Corvette, Achievements.Destroyer
        };

        static readonly Dictionary<EnemyDFHiveSystem, AssaultWatch> watches = new Dictionary<EnemyDFHiveSystem, AssaultWatch>();
        static readonly HashSet<EnemyDFHiveSystem> joinedLate = new HashSet<EnemyDFHiveSystem>();
        static readonly HashSet<EnemyDFHiveSystem> seen = new HashSet<EnemyDFHiveSystem>();
        static readonly List<EnemyDFHiveSystem> leftBehind = new List<EnemyDFHiveSystem>();

        static GameData trackedGame;
        static long trackingStartTick;
        static long nextScanTick;
        static long lastInputTick;
        static long lastTick;

        internal static void Update()
        {
            GameData data = GameMain.data;
            if (!GameMain.isRunning || DSPGame.IsMenuDemo || data == null || GameMain.mainPlayer == null)
            {
                trackedGame = null;
                return;
            }
            long now = GameMain.gameTick;
            if (data != trackedGame || now < lastTick)
            {
                StartTracking(data);
            }
            lastTick = now;

            if (Input.anyKey || Input.mouseScrollDelta != Vector2.zero)
            {
                lastInputTick = now;
            }
            if (now < nextScanTick)
            {
                return;
            }
            nextScanTick = now + ScanInterval;

            CheckShield(now);
            CheckLosses(now);
            Raise(Achievements.EmbraceGlory, CombatEvents.BurstKills);
            CheckCoreKills(now);
            CheckAssaults(now, data);
        }

        static void StartTracking(GameData data)
        {
            trackedGame = data;
            trackingStartTick = GameMain.gameTick;
            nextScanTick = 0;
            lastInputTick = long.MinValue;
            watches.Clear();
            joinedLate.Clear();
            CombatEvents.Reset();
            bool sandbox = data.gameDesc.isSandboxMode;
            Plugin.Log.LogInfo(sandbox
                ? "Game started in sandbox mode; achievements will not progress"
                : $"Tracking achievements for this game (combat mode: {data.gameDesc.isCombatMode})");
        }

        /// <summary>进度只记录最好成绩。 / Progress keeps the best result.</summary>
        static void Raise(Achievement achievement, long value)
        {
            if (achievement != null && achievement.CanProgress && value > achievement.Progress)
            {
                achievement.SetProgress(value);
            }
        }

        /// <summary>护盾显示值 = 内部能量 / energyShieldEnergyRate / 100，与机甲面板一致。
        /// Displayed shield = internal energy / energyShieldEnergyRate / 100, same as the mecha panel.</summary>
        static void CheckShield(long now)
        {
            long energy = CombatEvents.ShieldEnergySince(now - ShieldWindow);
            long rate = GameMain.mainPlayer.mecha.energyShieldEnergyRate;
            if (rate > 0)
            {
                Raise(Achievements.ActivatingShields, energy / (rate * 100));
            }
        }

        static void CheckLosses(long now)
        {
            foreach (int protoId in fighterTypes)
            {
                Raise(Achievements.ForFighter(protoId), CombatEvents.FighterLossesSince(protoId, now - LossWindow));
            }
        }

        /// <summary>
        /// 核心必须在伊卡洛斯所在星系被摧毁，伊卡洛斯有太空编队在外作战，过去 5 分钟没有读档，也没有损失太空战舰。
        /// 读档会重新开始计时（见 StartTracking），因此读档后不足 5 分钟不判定。
        /// The core must die in Icarus's current star system while an Icarus space fleet is deployed, with no save loaded and no space ship lost in the last 5 minutes.
        /// Loading restarts the clock (see StartTracking), so it is not judged within 5 minutes of a load.
        /// </summary>
        static void CheckCoreKills(long now)
        {
            List<int> stars = CombatEvents.TakeCoreKills();
            if (stars.Count == 0 || Achievements.WellFought.Unlocked)
            {
                return;
            }
            StarData local = GameMain.localStar;
            bool calm = now - trackingStartTick >= LossWindow && CombatEvents.LastMechaShipLossTick < now - LossWindow;
            if (local == null || !calm || !HasSpaceFleetOut(GameMain.mainPlayer.mecha))
            {
                return;
            }
            if (stars.Contains(local.id))
            {
                Achievements.WellFought.Unlock();
            }
        }

        static bool HasSpaceFleetOut(Mecha mecha)
        {
            CombatModuleComponent module = mecha.spaceCombatModule;
            if (module == null || !module.moduleEnabled || module.moduleFleets == null)
            {
                return false;
            }
            for (int i = 0; i < module.fleetCount && i < module.moduleFleets.Length; i++)
            {
                if (module.moduleFleets[i].fleetId > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 只跟踪伊卡洛斯所在星系、且从开头就看到的突袭。期间有任何按键/鼠标按键/滚轮、伊卡洛斯阵亡或离开星系，这一波就不算。
        /// Only assaults in Icarus's star system that were seen from their start are followed. Any key, mouse button or scroll, Icarus dying or leaving the system voids the wave.
        /// </summary>
        static void CheckAssaults(long now, GameData data)
        {
            SpaceSector sector = data.spaceSector;
            StarData star = GameMain.localStar;
            Player player = GameMain.mainPlayer;

            seen.Clear();
            if (sector?.dfHives != null && star != null && star.index >= 0 && star.index < sector.dfHives.Length)
            {
                for (EnemyDFHiveSystem hive = sector.dfHives[star.index]; hive != null; hive = hive.nextSibling)
                {
                    seen.Add(hive);
                    watches.TryGetValue(hive, out AssaultWatch watch);
                    if (hive.evolve.waveTicks > 0)
                    {
                        if (watch == null && !joinedLate.Contains(hive))
                        {
                            if (hive.evolve.waveTicks <= FreshWaveTicks)
                            {
                                watch = new AssaultWatch(now);
                                watches[hive] = watch;
                            }
                            else
                            {
                                joinedLate.Add(hive);
                            }
                        }
                        if (watch != null)
                        {
                            Judge(watch, player);
                            watch.Scan(sector, hive);
                        }
                    }
                    else
                    {
                        joinedLate.Remove(hive);
                        if (watch != null)
                        {
                            Finish(hive, watch, sector, player);
                        }
                    }
                }
            }

            leftBehind.Clear();
            foreach (EnemyDFHiveSystem hive in watches.Keys)
            {
                if (!seen.Contains(hive))
                {
                    leftBehind.Add(hive);
                }
            }
            foreach (EnemyDFHiveSystem hive in leftBehind)
            {
                AssaultWatch watch = watches[hive];
                if (hive.starData != star)
                {
                    watch.Failed = true;
                }
                Finish(hive, watch, sector, player);
            }
            joinedLate.RemoveWhere(h => !seen.Contains(h));

            if (watches.Count == 0)
            {
                CombatEvents.ClearKilledAssaultUnits();
            }
        }

        static void Judge(AssaultWatch watch, Player player)
        {
            if (lastInputTick >= watch.StartTick || !player.isAlive)
            {
                watch.Failed = true;
            }
        }

        static void Finish(EnemyDFHiveSystem hive, AssaultWatch watch, SpaceSector sector, Player player)
        {
            watches.Remove(hive);
            Judge(watch, player);
            if (!watch.Failed && sector != null)
            {
                watch.Scan(sector, hive);
            }
            if (watch.WipedOut)
            {
                Achievements.WellDone.Unlock();
            }
        }
    }
}
