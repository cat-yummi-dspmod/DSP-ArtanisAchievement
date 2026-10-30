using System.Collections.Generic;

namespace ArtanisAchievement
{
    /// <summary>
    /// 跟踪一个蜂巢的一次太空突袭。每个参与突袭的单位离开突袭状态时，必须是被击毁的；
    /// 如果只是飞回巢穴（停战、调低攻击性等），这一波就不算全灭。
    /// Follows one space assault from one hive. Every assaulting unit must leave the assault by being destroyed;
    /// if any just flies home (truce, lowered aggression, ...), the wave does not count as wiped out.
    /// </summary>
    internal sealed class AssaultWatch
    {
        internal readonly long StartTick;
        internal bool Failed;
        internal int Killed;

        readonly HashSet<int> units = new HashSet<int>();
        readonly List<int> gone = new List<int>();

        internal AssaultWatch(long startTick)
        {
            StartTick = startTick;
        }

        internal bool WipedOut => !Failed && Killed > 0 && units.Count == 0;

        internal void Scan(SpaceSector sector, EnemyDFHiveSystem hive)
        {
            EnemyData[] pool = sector.enemyPool;
            EnemyUnitComponent[] buffer = hive.units?.buffer;
            if (pool == null)
            {
                return;
            }
            if (buffer != null)
            {
                int cursor = hive.units.cursor;
                for (int i = 1; i < cursor && i < buffer.Length; i++)
                {
                    if (buffer[i].id != i)
                    {
                        continue;
                    }
                    int enemyId = buffer[i].enemyId;
                    if (IsAssaulting(pool, enemyId))
                    {
                        units.Add(enemyId);
                    }
                }
            }

            gone.Clear();
            foreach (int enemyId in units)
            {
                if (!IsAssaulting(pool, enemyId))
                {
                    gone.Add(enemyId);
                }
            }
            foreach (int enemyId in gone)
            {
                units.Remove(enemyId);
                if (CombatEvents.TakeKilledAssaultUnit(enemyId))
                {
                    Killed++;
                }
                else
                {
                    Failed = true;
                }
            }
        }

        static bool IsAssaulting(EnemyData[] pool, int enemyId)
        {
            return enemyId > 0 && enemyId < pool.Length && pool[enemyId].id == enemyId && pool[enemyId].isAssaultingUnit;
        }
    }
}
