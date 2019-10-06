using System;
using System.Reflection;
using Oxide.Core;

// Token: 0x02000002 RID: 2
internal static class EXTENSION
{
	// Token: 0x04000001 RID: 1
	public static VersionNumber Version = new VersionNumber((int)((ushort)Assembly.GetExecutingAssembly().GetName().Version.Major), (int)((ushort)Assembly.GetExecutingAssembly().GetName().Version.Minor), (int)((ushort)Assembly.GetExecutingAssembly().GetName().Version.Build));

	// Token: 0x04000002 RID: 2
	public const string Title = "RPGKill for Oxide";

	// Token: 0x04000003 RID: 3
	public const string Name = "RPGKill";

	// Token: 0x04000004 RID: 4
	public const string Author = "Мизантроп";
}
