using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pluton
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ApiVersionAttribute : Attribute {
		public Version ApiVersion;
		public ApiVersionAttribute(Version version) {
			this.ApiVersion = version;
		}
		public ApiVersionAttribute(int major, int minor) : this(new Version(major, minor)) { }
	}

	public class ModuleManager {

		public static readonly Version ApiVersion = new Version(1, 0, 0, 0);
		public static string ModulesFolder = Config.GetModulesFolder();
		public static string PublicFolder = Config.GetPublicFolder();

		//private static bool IsIgnoreVersion = true;
		private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
		public static readonly List<ModuleContainer> Modules = new List<ModuleContainer>();

		public static ReadOnlyCollection<ModuleContainer> Plugins {
			get { return new ReadOnlyCollection<ModuleContainer>(Modules); }
		}

		internal static void LoadModules() {
			Logger.Log("[Modules] Loading modules...");
			string IgnoredPluginsFilePath = Path.Combine(ModulesFolder, "ignoredmodules.txt");

			List<string> IgnoredModules = new List<string>();
			if (File.Exists(IgnoredPluginsFilePath))
				IgnoredModules.AddRange(File.ReadAllLines(IgnoredPluginsFilePath));

			DirectoryInfo[] DirectoryInfos = new DirectoryInfo(ModulesFolder).GetDirectories();
			foreach (DirectoryInfo DirInfo in DirectoryInfos) {
				FileInfo FileInfo = new FileInfo(Path.Combine(DirInfo.FullName, DirInfo.Name + ".dll"));
				if (!FileInfo.Exists)
					continue;

				if (Array.IndexOf(Config.FougeriteConfig.EnumSection("Modules"), DirInfo.Name) == -1) {
					Logger.LogDebug(string.Format("[Modules] {0} is not configured to be loaded.", DirInfo.Name));
					continue;
				}

				Logger.LogDebug("[Modules] Module Found: " + FileInfo.Name);
				string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FileInfo.Name);
				if (IgnoredModules.Contains(FileNameWithoutExtension)) {
					Logger.LogWarning(string.Format("[Modules] {0} was ignored from being loaded.", FileNameWithoutExtension));
					continue;
				}

				try {
					Logger.LogDebug("[Modules] Loading assembly: " + FileInfo.Name);
					Assembly Assembly;
					// The plugin assembly might have been resolved by another plugin assembly already, so no use to
					// load it again, but we do still have to verify it and create plugin instances.
					if (!LoadedAssemblies.TryGetValue(FileNameWithoutExtension, out Assembly)) {
						Assembly = Assembly.Load(File.ReadAllBytes(FileInfo.FullName));
						LoadedAssemblies.Add(FileNameWithoutExtension, Assembly);
					}

					foreach (Type Type in Assembly.GetExportedTypes()) {
						if (!Type.IsSubclassOf(typeof(Module)) || !Type.IsPublic || Type.IsAbstract)
							continue;
						Logger.LogDebug("[Modules] Checked " + Type.FullName);

						Module PluginInstance = null;
						try {
							PluginInstance = (Module)Activator.CreateInstance(Type);
							Logger.LogDebug("[Modules] Instance created: " + Type.FullName);
						} catch (Exception ex) {
							// Broken plugins better stop the entire server init.
							Logger.LogError(string.Format("[Modules] Could not create an instance of plugin class \"{0}\". {1}", Type.FullName, ex));
						}
						if (PluginInstance != null) {
							ModuleContainer Container = new ModuleContainer(PluginInstance);
							Container.Plugin.ModuleFolder = Path.Combine(PublicFolder, Config.GetValue("Modules", DirInfo.Name).TrimStart(new char[]{'\\','/'}).Trim());
							Modules.Add(Container);
							Logger.LogDebug("[Modules] Module added: " + FileInfo.Name);
							break;
						}
					}
				} catch (Exception ex) {
					// Broken assemblies better stop the entire server init.
					Logger.LogError(string.Format("[Modules] Failed to load assembly \"{0}\". {1}", FileInfo.Name, ex));
				}
			}

			IOrderedEnumerable<ModuleContainer> OrderedModuleSelector =
				from x in Plugins
				orderby x.Plugin.Order, x.Plugin.Name
			select x;

			foreach (ModuleContainer CurrentModule in OrderedModuleSelector) {
				try {
					CurrentModule.Initialize();
				} catch (Exception ex) {
					// Broken modules better stop the entire server init.
					Logger.LogError(string.Format(
						"[Modules] Module \"{0}\" has thrown an exception during initialization. {1}", CurrentModule.Plugin.Name, ex));
				}

				Logger.Log(string.Format(
					"[Modules] Module {0} v{1} (by {2}) initiated.", CurrentModule.Plugin.Name, CurrentModule.Plugin.Version, CurrentModule.Plugin.Author));
			}

			Hooks.ModulesLoaded();
		}

		internal static void UnloadModules() {
			foreach (ModuleContainer ModuleContainer in Modules) {
				try {
					ModuleContainer.DeInitialize();
				} catch (Exception ex) {
					Logger.LogError(string.Format(
						"[Modules] Module \"{0}\" has thrown an exception while being deinitialized:\n{1}", ModuleContainer.Plugin.Name, ex));
				}
			}

			foreach (ModuleContainer ModuleContainer in Modules) {
				try {
					ModuleContainer.Dispose();
				} catch (Exception ex) {
					Logger.LogError(string.Format(
						"[Modules] Module \"{0}\" has thrown an exception while being disposed:\n{1}", ModuleContainer.Plugin.Name, ex));
				}
			}
			Modules.Clear();
			Logger.LogDebug("All modules unloaded!");
		}

		internal static void ReloadModules() {
			UnloadModules();
			LoadModules();
		}
	}
}