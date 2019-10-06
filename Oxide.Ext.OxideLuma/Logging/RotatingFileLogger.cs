using System;
using System.IO;
using Oxide.Core;
using Oxide.Core.Logging;

namespace Luma.Oxide.Logging
{
	// Token: 0x02000019 RID: 25
	public class RotatingFileLogger : ThreadedLogger
	{
		// Token: 0x0600008F RID: 143 RVA: 0x00002696 File Offset: 0x00000896
		private string GetLogFilename(DateTime date)
		{
			return Path.Combine(Interface.GetMod().LogDirectory, string.Format("rpgkill_{0:dd-MM-yyyy}.txt", date));
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000026B7 File Offset: 0x000008B7
		protected override void BeginBatchProcess()
		{
			this.writer = new StreamWriter(new FileStream(this.GetLogFilename(DateTime.Now), FileMode.Append, FileAccess.Write));
		}

		// Token: 0x06000091 RID: 145 RVA: 0x000026D6 File Offset: 0x000008D6
		protected override void ProcessMessage(Logger.LogMessage message)
		{
			this.writer.WriteLine(message.ConsoleMessage);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000026E9 File Offset: 0x000008E9
		protected override void FinishBatchProcess()
		{
			this.writer.Close();
			this.writer.Dispose();
			this.writer = null;
		}

		// Token: 0x0400006D RID: 109
		private StreamWriter writer;
	}
}
