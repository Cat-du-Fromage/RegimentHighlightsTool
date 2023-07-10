using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public sealed class MoveRegimentOrder : RegimentOrder
    {
        public readonly Vector3 LeaderDestination;
        public FormationData FormationDestination { get; private set; }
        public MoveRegimentOrder(Regiment regiment, int widthGoal, Vector3 leaderLeaderDestination) : base(regiment)
        {
            FormationDestination = regiment.CurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            FormationDestination.SetDirection(leaderLeaderDestination - regiment.transform.position);
            LeaderDestination = leaderLeaderDestination;
        }
        
        public MoveRegimentOrder(Regiment regiment, int widthGoal, Vector3 firstUnitFirstRow, Vector3 lastUnitFirstRow) : base(regiment)
        {
            FormationDestination = regiment.CurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            Vector3 direction = math.normalizesafe(math.cross(math.down(), lastUnitFirstRow - firstUnitFirstRow));
            FormationDestination.SetDirection(direction);
            LeaderDestination = (firstUnitFirstRow + lastUnitFirstRow) * 0.5f + direction * regiment.CurrentFormation.DistanceUnitToUnit.y;
        }
    }
}