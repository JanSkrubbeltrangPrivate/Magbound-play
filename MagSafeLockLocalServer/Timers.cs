using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagSafeLockServer
{
    public class Timers: IDisposable
    {
        private CancellationTokenSource _source;
        private CancellationToken _token;
        public Timers()
        {
            _source = new CancellationTokenSource();
            _token = _source.Token;
        }

        public void EndlessTimer(TimeSpan Delay, Action preAction, Action action, Action postAction) {
            Task.Run(async () => 
            {
                try {
                    preAction?.Invoke();
                    PeriodicTimer timer = new(Delay);
                    while(await timer.WaitForNextTickAsync(_token)) {
                        action?.Invoke();
                    }
                }
                catch (TaskCanceledException ) {
                    postAction?.Invoke();
                }

            });
        }

        public void TimedTimer(TimeSpan Delay, TimeSpan Runtime, Action preAction, Action action, Action postAction) {
            Task.Run(async () => 
            {
                try {
                    preAction?.Invoke();
                    PeriodicTimer timer = new(Delay);
                    var startTime = DateTime.Now;

                    while(await timer.WaitForNextTickAsync(_token) && DateTime.Now < startTime + Runtime) {
                        action?.Invoke();
                    }
                    postAction?.Invoke();
                }
                catch (TaskCanceledException ) {
                    postAction?.Invoke();
                }
            });
        }

        public void Cancel() 
        {
            _source.Cancel();
        }

        public void Dispose()
        {
            _source.Cancel();
        }
    }
}