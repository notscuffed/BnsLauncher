using System;
using System.Threading;

namespace BnsLauncher
{
    public static class Debouncer
    {
        public static Action Debounce(Action action, TimeSpan timeSpan)
        {
            Timer timer = null;

            return () =>
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }

                timer = new Timer(_ => action(), null, timeSpan, TimeSpan.FromMilliseconds(-1));
            };
        }
    }
}