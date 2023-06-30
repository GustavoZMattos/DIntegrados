using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace DIntegrados.Controllers
{
    public class FifoSemaphore
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Queue<TaskCompletionSource<bool>> _waitingQueue;

        public FifoSemaphore(int initialCount)
        {
            _semaphore = new SemaphoreSlim(initialCount);
            _waitingQueue = new Queue<TaskCompletionSource<bool>>();
        }

        public async Task WaitAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            lock (_waitingQueue)
            {
                _waitingQueue.Enqueue(tcs);
            }

            await tcs.Task;
            await _semaphore.WaitAsync();
        }

        public void Release()
        {
            TaskCompletionSource<bool> tcs = null;

            lock (_waitingQueue)
            {
                if (_waitingQueue.Count > 0)
                {
                    tcs = _waitingQueue.Dequeue();
                }
            }

            if (tcs != null)
            {
                tcs.SetResult(true);
            }
            else
            {
                _semaphore.Release();
            }
        }
    }

}
