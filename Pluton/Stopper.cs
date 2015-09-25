using System;
using System.Diagnostics;

namespace Pluton
{
    public class Stopper : CountedInstance, IDisposable
    {
        readonly string Type;
        readonly string Method;
        readonly long WarnTimeMS;
        readonly Stopwatch stopper;

        public Stopper(string type, string method, float warnSecs = 0.1f)
        {
            if (!pluton.stopper)
                return;
            Type = type;
            Method = method;
            WarnTimeMS = (long)(warnSecs * 1000);
            stopper = Stopwatch.StartNew();
        }

        void IDisposable.Dispose()
        {
            if (!pluton.stopper)
                return;

            if (stopper.ElapsedMilliseconds > WarnTimeMS) {
                Logger.LogWarning(String.Format("[{0}.{1}] Took: {2}s ({3}ms)",
                    Type,
                    Method,
                    stopper.Elapsed.Seconds,
                    stopper.ElapsedMilliseconds
                ));
            }
        }
    }
}

