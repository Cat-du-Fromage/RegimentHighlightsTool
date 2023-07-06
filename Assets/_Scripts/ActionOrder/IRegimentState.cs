namespace KaizerWald
{
    public interface IRegimentState
    {
        public IRegimentState UpdateState(Regiment regiment);
        public void OnEnter(Regiment regiment);
        public void OnUpdate();
        public void OnExit(Regiment regiment);
    }
}