namespace Scaffold.VisualScripting
{
    public interface ITriggerCondition
    {
        bool Evaluate(TriggerExecutionContext context);
    }
}
