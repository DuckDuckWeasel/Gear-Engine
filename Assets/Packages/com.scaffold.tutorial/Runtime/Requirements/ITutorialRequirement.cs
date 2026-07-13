using System;

namespace Scaffold.Tutorial.Requirements
{
    public interface ITutorialRequirement
    {
        Cysharp.Threading.Tasks.UniTask WaitUntilMetAsync();
    }
}
