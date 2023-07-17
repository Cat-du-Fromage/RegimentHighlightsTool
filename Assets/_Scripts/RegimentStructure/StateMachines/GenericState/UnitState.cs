namespace KaizerWald
{
/*
    public interface IUnitState
    {
        public static UnitNullState Null { get; private set; } = new UnitNullState();
        
        public State<Regiment> ParentState { get; }
    }
*/
    public class UnitState : State<Unit>
    {
        public new static readonly EStates Default = State<Unit>.Default;
        public static UnitNullState Null { get; private set; } = new UnitNullState();
        protected UnitState(Unit unitAttach, EStates stateIdentity, State<Regiment> parentState) : base(unitAttach, stateIdentity)
        {
            
        }
    }
    
}