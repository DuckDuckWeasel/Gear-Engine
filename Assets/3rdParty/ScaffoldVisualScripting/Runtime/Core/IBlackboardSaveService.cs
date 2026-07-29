using System.Threading;
using System.Threading.Tasks;

namespace Scaffold.VisualScripting
{
    public interface IBlackboardSaveService
    {
        Task SaveAsync(string slot, BlackboardSaveData data, CancellationToken cancellationToken);

        Task<BlackboardSaveData> LoadAsync(string slot, BlackboardRuntimeInstanceId runtimeInstanceId, CancellationToken cancellationToken);

        Task DeleteAsync(string slot, BlackboardRuntimeInstanceId runtimeInstanceId, CancellationToken cancellationToken);
    }
}
