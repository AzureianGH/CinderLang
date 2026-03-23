using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendInterface
{
    public interface IBlock
    {
        public void RemoveTerminator();
        public void ClearBlock();
    }
}
