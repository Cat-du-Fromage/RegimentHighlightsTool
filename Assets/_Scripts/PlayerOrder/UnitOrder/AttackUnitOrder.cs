namespace KaizerWald
{
    public class AttackUnitOrder : UnitOrder
    {
        public Unit EnemyTarget { get; private set; }
        
        protected AttackUnitOrder(Unit receiver, EStates state) : base(receiver, state)
        {
        }

        protected AttackUnitOrder(Unit receiver, RegimentOrder baseOrder) : base(receiver, baseOrder)
        {
        }
        
        public AttackUnitOrder(Unit receiver, Unit unitTarget) : base(receiver, EStates.Fire)
        {
            EnemyTarget = unitTarget;
        }
    }
}