namespace KaizerWald
{
    public sealed class RegimentNullState : RegimentState
    {
        public RegimentNullState(Regiment objectAttach = null) : base(objectAttach, EStates.None) { }
    }
}