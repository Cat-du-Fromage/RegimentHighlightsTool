namespace KaizerWald
{
    /// <summary>
    /// COMPOSITE HIGHLIGHT
    /// </summary>
    public abstract class CompositeSystem : HighlightSystem
    {
        public abstract CombinedController Controller { get; protected set; }
        public abstract HighlightSubSystem SubSystem1 { get; }
        public abstract HighlightSubSystem SubSystem2 { get; }
        
        public override void AddRegiment(ISelectableRegiment regiment)
        {
            SubSystem1.Register.RegisterRegiment(regiment);
            SubSystem2.Register.RegisterRegiment(regiment);
        }

        public override void RemoveRegiment(ISelectableRegiment regiment)
        {
            SubSystem1.Register.UnregisterRegiment(regiment);
            SubSystem2.Register.UnregisterRegiment(regiment);
        }
    }
}