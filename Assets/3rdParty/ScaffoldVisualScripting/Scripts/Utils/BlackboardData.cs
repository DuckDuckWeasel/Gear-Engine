
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Serializable container for a string variable.
    /// </summary>
    [System.Serializable]
    public class StringVar
    {
        [SerializeField] protected string key;
        [SerializeField] protected string value;

        #region Public methods

        public string Key { get { return key; } set { key = value; } }
        public string Value { get { return value; } set { this.value = value; } }

        #endregion
    }

    /// <summary>
    /// Serializable container for an integer variable.
    /// </summary>
    [System.Serializable]
    public class IntVar
    {
        [SerializeField] protected string key;
        [SerializeField] protected int value;

        #region Public methods

        public string Key { get { return key; } set { key = value; } }
        public int Value { get { return value; } set { this.value = value; } }

        #endregion
    }

    /// <summary>
    /// Serializable container for a float variable.
    /// </summary>
    [System.Serializable]
    public class FloatVar
    {
        [SerializeField] protected string key;
        [SerializeField] protected float value;

        #region Public methods

        public string Key { get { return key; } set { key = value; } }
        public float Value { get { return value; } set { this.value = value; } }

        #endregion
    }

    /// <summary>
    /// Serializable container for a boolean variable.
    /// </summary>
    [System.Serializable]
    public class BoolVar
    {
        [SerializeField] protected string key;
        [SerializeField] protected bool value;

        #region Public methods

        public string Key { get { return key; } set { key = value; } }
        public bool Value { get { return value; } set { this.value = value; } }

        #endregion
    }

    /// <summary>
    /// Serializable container for encoding the state of a Blackboard's variables.
    /// </summary>
    [System.Serializable]
    public class BlackboardData
    {
        [SerializeField] protected string blackboardName;
        [SerializeField] protected List<StringVar> stringVars = new List<StringVar>();
        [SerializeField] protected List<IntVar> intVars = new List<IntVar>();
        [SerializeField] protected List<FloatVar> floatVars = new List<FloatVar>();
        [SerializeField] protected List<BoolVar> boolVars = new List<BoolVar>();

        #region Public methods

        /// <summary>
        /// Gets or sets the name of the encoded Blackboard.
        /// </summary>
        public string BlackboardName { get { return blackboardName; } set { blackboardName = value; } }

        /// <summary>
        /// Gets or sets the list of encoded string variables.
        /// </summary>
        public List<StringVar> StringVars { get { return stringVars; } set { stringVars = value; } }

        /// <summary>
        /// Gets or sets the list of encoded integer variables.
        /// </summary>
        public List<IntVar> IntVars { get { return intVars; } set { intVars = value; } }

        /// <summary>
        /// Gets or sets the list of encoded float variables.
        /// </summary>
        public List<FloatVar> FloatVars { get { return floatVars; } set { floatVars = value; } }

        /// <summary>
        /// Gets or sets the list of encoded boolean variables.
        /// </summary>
        public List<BoolVar> BoolVars { get { return boolVars; } set { boolVars = value; } }

        /// <summary>
        /// Encodes the data in a Blackboard into a structure that can be stored by the save system.
        /// </summary>
        public static BlackboardData Encode(Blackboard blackboard)
        {
            var blackboardData = new BlackboardData();

            blackboardData.BlackboardName = blackboard.name;

            for (int i = 0; i < blackboard.Variables.Count; i++) 
            {
                var v = blackboard.Variables[i];

                // Save string
                var stringVariable = v as StringVariable;
                if (stringVariable != null)
                {
                    var d = new StringVar();
                    d.Key = stringVariable.Key;
                    d.Value = stringVariable.Value;
                    blackboardData.StringVars.Add(d);
                }

                // Save int
                var intVariable = v as IntegerVariable;
                if (intVariable != null)
                {
                    var d = new IntVar();
                    d.Key = intVariable.Key;
                    d.Value = intVariable.Value;
                    blackboardData.IntVars.Add(d);
                }

                // Save float
                var floatVariable = v as FloatVariable;
                if (floatVariable != null)
                {
                    var d = new FloatVar();
                    d.Key = floatVariable.Key;
                    d.Value = floatVariable.Value;
                    blackboardData.FloatVars.Add(d);
                }

                // Save bool
                var boolVariable = v as BooleanVariable;
                if (boolVariable != null)
                {
                    var d = new BoolVar();
                    d.Key = boolVariable.Key;
                    d.Value = boolVariable.Value;
                    blackboardData.BoolVars.Add(d);
                }
            }

            return blackboardData;
        }

        /// <summary>
        /// Decodes a BlackboardData object and uses it to restore the state of a Blackboard in the scene.
        /// </summary>
        public static void Decode(BlackboardData blackboardData)
        {
            var go = GameObject.Find(blackboardData.BlackboardName);
            if (go == null)
            {
                Debug.LogError("Failed to find blackboard object specified in save data");
                return;
            }

            var blackboard = go.GetComponent<Blackboard>();
            if (blackboard == null)
            {
                Debug.LogError("Failed to find blackboard object specified in save data");
                return;
            }

            for (int i = 0; i < blackboardData.BoolVars.Count; i++)
            {
                var boolVar = blackboardData.BoolVars[i];
                blackboard.SetBooleanVariable(boolVar.Key, boolVar.Value);
            }
            for (int i = 0; i < blackboardData.IntVars.Count; i++)
            {
                var intVar = blackboardData.IntVars[i];
                blackboard.SetIntegerVariable(intVar.Key, intVar.Value);
            }
            for (int i = 0; i < blackboardData.FloatVars.Count; i++)
            {
                var floatVar = blackboardData.FloatVars[i];
                blackboard.SetFloatVariable(floatVar.Key, floatVar.Value);
            }
            for (int i = 0; i < blackboardData.StringVars.Count; i++)
            {
                var stringVar = blackboardData.StringVars[i];
                blackboard.SetStringVariable(stringVar.Key, stringVar.Value);
            }
        }

        #endregion
    }
}