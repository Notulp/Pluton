using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pluton
{
	public abstract class Module : IDisposable
	{
		public virtual string ModuleFolder
		{
			get;
			set;
		}

		public virtual string Name
		{
			get
			{
				return "None";
			}
		}

		public virtual Version Version
		{
			get
			{
				return new Version(1, 0);
			}
		}

		public virtual string Author
		{
			get
			{
				return "None";
			}
		}

		public virtual string Description
		{
			get
			{
				return "None";
			}
		}

		public virtual bool Enabled
		{
			get;
			set;
		}

		public virtual uint Order
		{
			get { return uint.MaxValue; }
		}

		public virtual string UpdateURL
		{
			get
			{
				return "";
			}
		}

		~Module()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public virtual void DeInitialize()
		{
		}

		public abstract void Initialize();
	}
}