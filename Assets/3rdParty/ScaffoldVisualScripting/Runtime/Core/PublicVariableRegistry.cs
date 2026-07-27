using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class PublicVariableRegistry : IPublicVariableRegistry
    {
        private readonly Dictionary<VariableAddress, VariableCellBase> cells = new Dictionary<VariableAddress, VariableCellBase>();

        public void Register(VariableAddress address, VariableCellBase cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            if (cells.ContainsKey(address))
            {
                throw new InvalidOperationException($"Public variable '{address}' is already registered.");
            }

            cells.Add(address, cell);
        }

        public void Unregister(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            List<VariableAddress> matches = FindAddresses(runtimeInstanceId);
            foreach (VariableAddress address in matches)
            {
                cells.Remove(address);
            }
        }

        public bool TryGet(VariableAddress address, out VariableCellBase cell)
        {
            return cells.TryGetValue(address, out cell);
        }

        private List<VariableAddress> FindAddresses(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            List<VariableAddress> matches = new List<VariableAddress>();
            foreach (VariableAddress address in cells.Keys)
            {
                if (address.RuntimeInstanceId == runtimeInstanceId)
                {
                    matches.Add(address);
                }
            }

            return matches;
        }
    }
}
