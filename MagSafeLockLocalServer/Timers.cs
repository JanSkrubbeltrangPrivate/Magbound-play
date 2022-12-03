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

        public PeriodicTimer EndlessTimer(TimeSpan Delay, Action preAction, Action action, Action postAction) {
            PeriodicTimer timer = new(Delay);
            Task.Run(async () => 
            {
                try {
                    preAction?.Invoke();
                    while(await timer.WaitForNextTickAsync(_token)) {
                        action?.Invoke();
                    }
                }
                catch (TaskCanceledException ) {
                    postAction?.Invoke();
                }
            });
            return timer;
        }

        public PeriodicTimer TimedTimer(TimeSpan Delay, TimeSpan Runtime, Action preAction, Action action, Action postAction) {
            PeriodicTimer timer = new(Delay);
            Task.Run(async () => 
            {
                try {
                    preAction?.Invoke();
                    
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
            return timer;
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