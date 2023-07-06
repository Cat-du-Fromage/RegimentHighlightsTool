using UnityEngine;

namespace KaizerWald
{
    public class MoveOrder : BaseOrder
    {
        public readonly Regiment Regiment;
        public readonly Vector3 RegimentDestination;
        public readonly Vector3[] UnitsDestination;
        public readonly FormationData TargetFormation;
        public readonly Vector3 LeftUnitFirstRow;
        public readonly Vector3 RightUnitFirstRow;

        public MoveOrder(Regiment regiment, Vector3 regimentDestination, Vector3[] unitsDestination, FormationData targetFormation): base(regiment)
        {
            RegimentDestination = regimentDestination;
            UnitsDestination = unitsDestination;
            TargetFormation = targetFormation;
            LeftUnitFirstRow = unitsDestination[0];
            RightUnitFirstRow = unitsDestination[TargetFormation.Width];
        }
    }
}