using Scaffold.CloudCode;
using System.Threading.Tasks;

namespace Scaffold.Tutorial.CloudCode
{
    public sealed class CompleteTutorialOptimisticHandler : IRequestHandler<CompleteTutorialRequest, CompleteTutorialResponse>, IOptimisticCloudCodeHandler
    {
        public bool TryMatch(string module, string endpoint, CompleteTutorialRequest request)
        {
            return endpoint == "GameApi";
        }

        public CompleteTutorialResponse GetOptimisticResponse(CompleteTutorialRequest request)
        {
            // For now, we assume the tutorial completion always succeeds on the client side optimistically
            var response = new CompleteTutorialResponse
            {
                success = true,
                errorMessage = null,
                completedTutorials = new System.Collections.Generic.List<string> { request.id }
            };
            return response;
        }

        public void Validate(CompleteTutorialResponse serverResponse, CompleteTutorialResponse optimisticResponse)
        {
            // Here you would validate if the server rejected the tutorial completion
        }

        public System.Type RequestClrType => typeof(CompleteTutorialRequest);
        public System.Type ResponseClrType => typeof(CompleteTutorialResponse);
    }
}
