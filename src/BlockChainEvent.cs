using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public interface BlockChainEvent
    {
        public EventType EventType { get; }
    }
}
