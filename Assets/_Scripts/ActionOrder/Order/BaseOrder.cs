namespace KaizerWald
{
    public abstract class BaseOrder
    {
        public readonly Regiment Regiment;

        protected BaseOrder(Regiment regiment)
        {
            Regiment = regiment;
        }
    }
}