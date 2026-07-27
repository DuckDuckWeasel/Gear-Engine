namespace Scaffold
{
    /// <summary>
    /// Supported display operations for Stage.
    /// </summary>
    public enum StageDisplayType
    {
        /// <summary> No operation. </summary>
        None,

        /// <summary> Show the stage and all portraits. </summary>
        Show,

        /// <summary> Hide the stage and all portraits. </summary>
        Hide,

        /// <summary> Swap the stage and all portraits with another stage. </summary>
        Swap,

        /// <summary> Move stage to the front. </summary>
        MoveToFront,

        /// <summary> Undim all portraits on the stage. </summary>
        UndimAllPortraits,

        /// <summary> Dim all non-speaking portraits on the stage. </summary>
        DimNonSpeakingPortraits
    }
}
