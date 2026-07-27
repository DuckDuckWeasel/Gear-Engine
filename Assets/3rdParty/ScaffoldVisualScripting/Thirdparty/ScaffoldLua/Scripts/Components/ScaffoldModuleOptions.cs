namespace Scaffold
{
    /// <summary>
    /// Options for using the Lua Scaffold module.
    /// </summary>
    public enum ScaffoldModuleOptions
    {
        /// <summary>Expose Scaffold helpers as global variables.</summary>
        UseGlobalVariables,

        /// <summary>Expose Scaffold helpers through the scaffold global variable.</summary>
        UseScaffoldVariable,

        /// <summary>Do not load the Scaffold helper module.</summary>
        NoScaffoldModule
    }
}
