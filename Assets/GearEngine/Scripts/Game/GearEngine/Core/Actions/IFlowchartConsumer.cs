using Scaffold;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Implemented by IActions that require access to the parent Flowchart 
    /// (e.g. for variable resolution).
    /// </summary>
    public interface IFlowchartConsumer
    {
        void SetFlowchart(Flowchart flowchart);
    }
}
