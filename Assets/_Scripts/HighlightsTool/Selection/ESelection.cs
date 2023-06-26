namespace KaizerWald
{
    public enum ESelection : int
    {
        Preselection = 0,
        Selection = 1
    }

    public static class ESelectionExtensions
    {
        public static int GetIndex(this ESelection value) => (int)value;

        public static readonly int PreselectionIndex = 0;
        //public static implicit operator int(ESelection value) => (int)value;
    }
}