namespace Pluton
{
    using System;
    using System.Collections.Generic;
    using System.Timers;

    public class TimedEvent : CountedInstance
    {

        readonly string _name;
        readonly Timer _timer;
        long lastTick;
        int _elapsedCount;

        public delegate void TimedEventFireDelegate(TimedEvent evt);

        public event TimedEventFireDelegate OnFire;

        public TimedEvent(string name, double interval)
        {
            _name = name;
            _timer = new Timer();
            _timer.Interval = interval;
            _timer.Elapsed += _timer_Elapsed;
            _elapsedCount = 0;
        }

        public TimedEvent(string name, double interval, Dictionary<string, object> args)
            : this(name, interval)
        {
            Args = args;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (OnFire != null) {
                OnFire(this);
            }

            _elapsedCount += 1;
            lastTick = DateTime.UtcNow.Ticks;
        }

        public void Start()
        {
            _timer.Start();
            lastTick = DateTime.UtcNow.Ticks;
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Kill()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public Dictionary<string, object> Args {
            get;
            set;
        }

        public double Interval {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public string Name {
            get { return _name; }
        }

        public double TimeLeft {
            get { return (Interval - ((DateTime.UtcNow.Ticks - lastTick) / 0x2710L)); }
        }

        public int ElapsedCount {
            get { return _elapsedCount; }
        }
    }
}

