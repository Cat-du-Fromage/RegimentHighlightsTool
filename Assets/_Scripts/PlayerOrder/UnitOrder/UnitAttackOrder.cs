namespace KaizerWald
{
    public class UnitAttackOrder : UnitOrder
    {
        public Unit EnemyTarget { get; private set; }
        
        protected UnitAttackOrder(RegimentOrder baseOrder) : base(baseOrder)
        {
        }
        
        public UnitAttackOrder(Unit unitTarget) : base(EStates.Fire)
        {
            EnemyTarget = unitTarget;
        }
    }
}