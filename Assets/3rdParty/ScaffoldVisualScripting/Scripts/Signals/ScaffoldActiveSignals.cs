
namespace Scaffold
{
    /// <summary>
    /// Scaffold Priority event signalling system.
    /// A common point for Scaffold core systems and user Commands to signal to external code that a
    /// Scaffold system is currently doing something important.
    /// 
    /// One intended use case for this system is to have your code listen to this to know when to 
    /// stop player movement or camera movement etc. when the player is engaged in a conversation 
    /// with an NPC.
    /// </summary>
    public static class ScaffoldPrioritySignals
    {
        #region Public members
        /// <summary>
        /// used by increase and decrease active depth functions.
        /// </summary>
        private static int activeDepth;

        public static int CurrentPriorityDepth
        {
            get
            {
                return activeDepth;
            } 
        }

        public static event ScaffoldPriorityStartHandler OnScaffoldPriorityStart;
        public delegate void ScaffoldPriorityStartHandler();

        public static event ScaffoldPriorityEndHandler OnScaffoldPriorityEnd;
        public delegate void ScaffoldPriorityEndHandler();


        public static event ScaffoldPriorityChangeHandler OnScaffoldPriorityChange;
        public delegate void ScaffoldPriorityChangeHandler(int previousActiveDepth, int newActiveDepth);
        
        /// <summary>
        /// Adds 1 to the theactiveDepth. If it was zero causes the OnScaffoldPriorityStart
        /// </summary>
        public static void DoIncreasePriorityDepth()
        {
            if(activeDepth == 0)
            {
                if (OnScaffoldPriorityStart != null)
                {
                    OnScaffoldPriorityStart();
                }
            }
            if(OnScaffoldPriorityChange != null)
            {
                OnScaffoldPriorityChange(activeDepth, activeDepth + 1);
            }
            activeDepth++;
        }

        /// <summary>
        /// Subtracts 1 to the theactiveDepth. If it reaches zero causes the OnScaffoldPriorityEnd
        /// </summary>
        public static void DoDecreasePriorityDepth()
        {
            if (OnScaffoldPriorityChange != null)
            {
                OnScaffoldPriorityChange(activeDepth, activeDepth - 1);
            }
            if(activeDepth == 1)
            {
                if(OnScaffoldPriorityEnd != null)
                {
                    OnScaffoldPriorityEnd();
                }
            }
            activeDepth--;
        }

        /// <summary>
        /// Forces active depth back to 0. If already 0 fires no signals.
        /// </summary>
        public static void DoResetPriority()
        {
            if (activeDepth == 0)
                return;

            if (OnScaffoldPriorityChange != null)
            {
                OnScaffoldPriorityChange(activeDepth, 0);
            }
            if (OnScaffoldPriorityEnd != null)
            {
                OnScaffoldPriorityEnd();
            }
            activeDepth = 0;
        }
        #endregion
    }
}
