using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public interface IBlockConnectionSource
    {
        void GetConnectedBlockNames(ICollection<string> blockNames);
    }
}
