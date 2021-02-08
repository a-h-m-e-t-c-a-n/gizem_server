using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WebRTCServer
{
    public class DataBus
    {
        ConcurrentDictionary<Object, ActionBlock<Object>> contexts = new ConcurrentDictionary<Object, ActionBlock<Object>>();

        public async Task<bool> WriteAsync<T>(Object id, T data, CancellationToken cancellationToken = default)
        {
            if (contexts.TryGetValue(id, out ActionBlock<Object> block))
            {
                await block.SendAsync(data, cancellationToken);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Complete(Object id)
        {
            if (contexts.TryRemove(id, out ActionBlock<Object> old))
            {
                old.Complete();
            }
            else
            {
                throw new Exception("DataBus>Read TryRemove");

            }

        }
       
        public Task ReadAsync<T>(Object id, Action<T> onReceive, CancellationToken cancellationToken = default)
        {
            var block = new ActionBlock<Object>((obj) =>
            {
                onReceive.Invoke((T)obj);
            });
            cancellationToken.Register(() =>
            {
                Complete(id);
            });
            contexts.TryAdd(id, block);
            return block.Completion;
        }
    }
}
