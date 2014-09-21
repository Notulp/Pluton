namespace Pluton {
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Timers;

	public class TimedEvent {

		private Dictionary<string, object> _args;
		private readonly string _name;
		private readonly System.Timers.Timer _timer;
		private long lastTick;
		private int _elapsedCount;

		public delegate void TimedEventFireDelegate(string name);
		public delegate void TimedEventFireArgsDelegate(string name, Dictionary<string, object> list);

		public event TimedEventFireDelegate OnFire;
		public event TimedEventFireArgsDelegate OnFireArgs;

		public TimedEvent(string name, double interval) {
			this._name = name;
			this._timer = new System.Timers.Timer();
			this._timer.Interval = interval;
			this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
			this._elapsedCount = 0;
		}

		public TimedEvent(string name, double interval, Dictionary<string, object> args)
			: this(name, interval) {
			this.Args = args;
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
			if (this.OnFire != null) {
				this.OnFire(this.Name);
			}
			if (this.OnFireArgs != null) {
				this.OnFireArgs(this.Name, this.Args);
			}

			this._elapsedCount += 1;
			this.lastTick = DateTime.UtcNow.Ticks;
		}

		public void Start() {
			this._timer.Start();
			this.lastTick = DateTime.UtcNow.Ticks;
		}

		public void Stop() {
			this._timer.Stop();
		}

		public Dictionary<string, object> Args {
			get { return this._args; }
			set { this._args = value; }
		}

		public double Interval {
			get { return this._timer.Interval; }
			set { this._timer.Interval = value; }
		}

		public string Name {
			get { return this._name; }
		}

		public double TimeLeft {
			get { return (this.Interval - ((DateTime.UtcNow.Ticks - this.lastTick) / 0x2710L)); }
		}

		public int ElapsedCount {
			get { return this._elapsedCount; }
		}
	}
}

