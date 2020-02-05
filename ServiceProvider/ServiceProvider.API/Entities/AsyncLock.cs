using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceProvider.API.Entities
{
    public class AsyncLock
    {
        private Task _unlockedTask = Task.CompletedTask;

        public async Task<AsyncUnlockOnDispose> Lock()
        {
            var tcs = new TaskCompletionSource<object>();

            await Interlocked.Exchange(ref _unlockedTask, tcs.Task);

            return new AsyncUnlockOnDispose(() => tcs.SetResult(null));
        }
    }
}
