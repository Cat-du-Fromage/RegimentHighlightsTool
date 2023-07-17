namespace KaizerWald
{
    public interface IRegimentState
    {
        
    }
    
    public class RegimentState : State<Regiment>, IRegimentState
    {
        public new static readonly EStates Default = State<Regiment>.Default;
        public static RegimentNullState Null { get; private set; } = new RegimentNullState();
        public RegimentState(Regiment objectAttach, EStates stateIdentity) : base(objectAttach, stateIdentity)
        {
            
        }
    }
}