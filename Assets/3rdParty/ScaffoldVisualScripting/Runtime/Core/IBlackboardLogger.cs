using System;

namespace Scaffold.VisualScripting
{
    public interface IBlackboardLogger
    {
        void Info(string message);

        void Warning(string message);

        void Error(string message, Exception exception = null);
    }
}
