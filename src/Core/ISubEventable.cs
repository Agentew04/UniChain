using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.Core
{
    public interface ISubEventable<T>
    {
        public T SubEvent { get; set; }

    }
}
