using System;
using System.Diagnostics;

namespace Pluton
{
    public class Stopper : CountedInstance
    {
        string Name;
        float WarnTime;
        Stopwatch stopper = new Stopwatch();
        string Category;


        public Stopper(string categ, string name, float warnSecs = 1)
        {
            Category = categ;
            Name = name;
            WarnTime = warnSecs;
        }

        public Stopper Start()
        {
            if (!pluton.stopper)
                return this;

            stopper.Reset();
            stopper.Start();
            return this;
        }

        public void Stop()
        {
            if (!pluton.stopper)
                return;

            stopper.Stop();
            if ((float)stopper.Elapsed.Seconds > WarnTime) {
                Logger.LogWarning(String.Format("{0}.{1}: Took: {2}",
                    Category,
                    Name,
                    stopper.Elapsed
                ));
            }
        }
    }
}

