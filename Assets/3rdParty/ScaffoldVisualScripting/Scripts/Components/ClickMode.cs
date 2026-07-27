namespace Scaffold
{
    /// <summary>
    /// Supported modes for clicking through a Say Dialog.
    /// </summary>
    public enum ClickMode
    {
        /// <summary> Clicking disabled. </summary>
        Disabled,

        /// <summary> Click anywhere on screen to advance. </summary>
        ClickAnywhere,

        /// <summary> Click anywhere on Say Dialog to advance. </summary>
        ClickOnDialog,

        /// <summary> Click on continue button to advance. </summary>
        ClickOnButton
    }
}
