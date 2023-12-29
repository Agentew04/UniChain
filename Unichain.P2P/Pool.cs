using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P {
    internal class Pool<T>(Func<T> factory, int maxPoolSize) {
        private readonly List<T> pool;

        public T Get() {
            if (pool.Count == 0) {
                return factory();
            }

            T item = pool[0];
            pool.RemoveAt(0);
            return item;
        }

        public void Return(T item) {
            if (pool.Count < maxPoolSize) {
                pool.Add(item);
            }
        }
        
    }
}
