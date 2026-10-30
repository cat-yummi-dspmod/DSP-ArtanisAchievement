using System.Collections.Generic;

namespace ArtanisAchievement
{
    /// <summary>
    /// 补丁里收集的战斗事件。击毁/护盾回复可能在逻辑线程上发生，这里只记录，判定都在主线程的 Tracker 里做。
    /// Combat events collected by the patches. Kills and shield regen may happen on logic threads; this only records them, and the Tracker judges them on the main thread.
    /// </summary>
    internal static class CombatEvents
    {
        internal struct ShieldGain
        {
            public long Tick;
            public long Energy;
        }

        static readonly object sync = new object();

        static readonly Dictionary<int, Queue<long>> fighterLosses = new Dictionary<int, Queue<long>>
        {
            { Achievements.PrecisionDrone, new Queue<long>() },
            { Achievements.AttackDrone, new Queue<long>() },
            { Achievements.Corvette, new Queue<long>() },
            { Achievements.Destroyer, new Queue<long>() },
        };
        static readonly Queue<ShieldGain> shieldGains = new Queue<ShieldGain>();
        static readonly List<int> coreKillStars = new List<int>();
        static readonly HashSet<int> killedAssaultUnits = new HashSet<int>();
        static long lastMechaShipLossTick = long.MinValue;

        internal static void Reset()
        {
            lock (sync)
            {
                foreach (Queue<long> q in fighterLosses.Values)
                {
                    q.Clear();
                }
                shieldGains.Clear();
                coreKillStars.Clear();
                killedAssaultUnits.Clear();
                lastMechaShipLossTick = long.MinValue;
            }
        }

        internal static void OnFighterLost(int protoId, bool mechaSpaceShip)
        {
            long tick = GameMain.gameTick;
            lock (sync)
            {
                if (fighterLosses.TryGetValue(protoId, out Queue<long> q))
                {
                    q.Enqueue(tick);
                }
                if (mechaSpaceShip)
                {
                    lastMechaShipLossTick = tick;
                }
            }
        }

        internal static void OnShieldGain(long energy)
        {
            long tick = GameMain.gameTick;
            lock (sync)
            {
                shieldGains.Enqueue(new ShieldGain { Tick = tick, Energy = energy });
            }
        }

        internal static void OnCoreKilled(int starId)
        {
            lock (sync)
            {
                coreKillStars.Add(starId);
            }
        }

        internal static void OnAssaultUnitKilled(int enemyId)
        {
            lock (sync)
            {
                killedAssaultUnits.Add(enemyId);
            }
        }

        // ---------------- 主线程读取 / main-thread reads ----------------

        /// <summary>丢掉 window 之前的阵亡记录，返回窗口内的数量。
        /// Drops losses older than the window and returns how many remain.</summary>
        internal static int FighterLossesSince(int protoId, long fromTick)
        {
            lock (sync)
            {
                Queue<long> q = fighterLosses[protoId];
                while (q.Count > 0 && q.Peek() < fromTick)
                {
                    q.Dequeue();
                }
                return q.Count;
            }
        }

        internal static long ShieldEnergySince(long fromTick)
        {
            lock (sync)
            {
                while (shieldGains.Count > 0 && shieldGains.Peek().Tick < fromTick)
                {
                    shieldGains.Dequeue();
                }
                long sum = 0;
                foreach (ShieldGain g in shieldGains)
                {
                    sum += g.Energy;
                }
                return sum;
            }
        }

        internal static long LastMechaShipLossTick
        {
            get
            {
                lock (sync)
                {
                    return lastMechaShipLossTick;
                }
            }
        }

        internal static List<int> TakeCoreKills()
        {
            lock (sync)
            {
                var list = new List<int>(coreKillStars);
                coreKillStars.Clear();
                return list;
            }
        }

        internal static bool TakeKilledAssaultUnit(int enemyId)
        {
            lock (sync)
            {
                return killedAssaultUnits.Remove(enemyId);
            }
        }

        internal static void ClearKilledAssaultUnits()
        {
            lock (sync)
            {
                killedAssaultUnits.Clear();
            }
        }
    }
}
