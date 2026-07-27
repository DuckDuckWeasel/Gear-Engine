
namespace Scaffold
{
    /// <summary>
    /// Interface for Blackboard components which can be updated when the 
    /// scene loads in the editor. This is used to maintain backwards 
    /// compatibility with earlier versions of Scaffold.
    /// </summary>
    interface IUpdateable
    {
        void UpdateToVersion(int oldVersion, int newVersion);
    }
}