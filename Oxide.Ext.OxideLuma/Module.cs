using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Oxide.Core;

namespace Luma.Oxide
{
	// Token: 0x0200000D RID: 13
	public class Module
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00003FA0 File Offset: 0x000021A0
		public static Module Install(byte[] rawData, Action<string, Exception> logHandler, JObject config)
		{
			OxideLumaExtension oxideLumaExtension = OxideLumaExtension.Instance;
			Module module = new Module();
			module.assembly = Assembly.Load(rawData);
			Version version = module.assembly.GetName().Version;
			module.assembly.GetName().Version = new Version(version.Major, version.Minor, version.Revision, Module.buildNumber++);
			module.type = module.assembly.GetType("Luma.Interface");
			EventInfo @event = module.type.GetEvent("OnLog");
			Delegate @delegate = Delegate.CreateDelegate(@event.EventHandlerType, logHandler.Target, logHandler.Method);
			module.logHandler = logHandler;
			module.instance = Activator.CreateInstance(module.type, new object[]
			{
				"oxide",
				Path.Combine(Interface.GetMod().DataDirectory, "luma")
			});
			@event.GetAddMethod().Invoke(module.instance, new object[]
			{
				@delegate
			});
			if (Module.Instance != null)
			{
				Module.Instance.Uninstall();
			}
			Module.Instance = module;
			module.CallEx("OnInstall", new object[]
			{
				config
			});
			return module;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000040D0 File Offset: 0x000022D0
		public bool Call(string method, params object[] args)
		{
			MethodInfo method2 = this.type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
			if (method2 == null)
			{
				return false;
			}
			bool result;
			try
			{
				method2.Invoke(this.instance, args);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004120 File Offset: 0x00002320
		public void CallEx(string method, params object[] args)
		{
			MethodInfo method2 = this.type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
			if (method2 == null)
			{
				throw new MissingMethodException(method);
			}
			try
			{
				method2.Invoke(this.instance, args);
			}
			catch (Exception ex)
			{
				throw ex.InnerException;
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004170 File Offset: 0x00002370
		public bool Call(string method, out object returnValue, params object[] args)
		{
			returnValue = null;
			MethodInfo method2 = this.type.GetMethod(method);
			if (method2 == null)
			{
				return false;
			}
			returnValue = method2.Invoke(this.instance, args);
			return true;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002301 File Offset: 0x00000501
		internal bool Uninstall()
		{
			return this.Call("OnUninstall", null);
		}

		// Token: 0x0400001E RID: 30
		internal static Module Instance;

		// Token: 0x0400001F RID: 31
		private static int buildNumber;

		// Token: 0x04000020 RID: 32
		private Assembly assembly;

		// Token: 0x04000021 RID: 33
		private Type type;

		// Token: 0x04000022 RID: 34
		private object instance;

		// Token: 0x04000023 RID: 35
		private Action<string, Exception> logHandler;
	}
}
