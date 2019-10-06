using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Luma.Oxide
{
	// Token: 0x0200000E RID: 14
	internal class Patching
	{
		// Token: 0x06000052 RID: 82 RVA: 0x000041A4 File Offset: 0x000023A4
		public void PathLog(OxideLuma plg)
		{
			CilPathing cilPathing = new CilPathing(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", null, null);
			if (cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.FirstOrDefault((Instruction o) => o.OpCode == OpCodes.Ldstr && o.Operand.ToString() == "OnServerUpdateInventory") == null)
			{
				Debug.LogError("Патчим OnServerUpdateInventory...");
				this.rest++;
				Instruction target = cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>();
				TypeReference variableType = new ArrayType(cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition));
				VariableDefinition item = new VariableDefinition(cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition));
				VariableDefinition item2 = new VariableDefinition(variableType);
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethod("UpdateSteamInventory").Body.Variables.Add(item);
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethod("UpdateSteamInventory").Body.Variables.Add(item2);
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ret));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Brfalse_S, target));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldloc_2));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Stloc_2));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Call, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Oxide.Core.dll", "Oxide.Core.Interface").SelectMethod("CallHook"))));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldloc_3));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Stelem_Ref));
				TypeDefinition type = cilPathing.SelectType(cilPathing._original, ".BaseEntity").tmp_typeDefinition.NestedTypes.FirstOrDefault((TypeDefinition o) => o.Name == "RPCMessage");
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Box, cilPathing._original.MainModule.Import(type)));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldarg_1));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldc_I4_0));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldloc_3));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Stloc_3));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Newarr, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition)));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldc_I4_1));
				cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").InsertBefore(cilPathing.SelectType(cilPathing._original, ".SteamInventory").SelectMethodIL("UpdateSteamInventory").Body.Instructions.First<Instruction>(), Instruction.Create(OpCodes.Ldstr, "OnServerUpdateInventory"));
				cilPathing.Save(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", delegate(Exception ex)
				{
					Debug.LogError(ex.Message);
				});
			}
            try
            {
                this.PathConnect();
            }
            catch (Exception ex)
            {
                Debug.Log("[PathConnect] " + ex);
            }
            try
            {
                this.PathEac();
            }
            catch (Exception ex)
            {
                Debug.Log("[PathEac] " + ex);
            }
            if (this.rest > 0)
			{
				//ConsoleSystem.Run(ConsoleSystem.Option.Server, "restart 1", new object[0]);
			}
            Debug.Log("Попытка патчинга!");
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004980 File Offset: 0x00002B80
		public void PathConnect()
		{
            CilPathing dd = new CilPathing(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", null, null);
            try
            {
                Debug.Log(dd.SelectType(dd._original, ".ServerMgr").SelectMethod("JoinGame").FullName);
            }
            catch(Exception ex)
            {
                Debug.Log("[FullName] " + ex);
            }
			CilPathing cilPathing = new CilPathing(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", null, null);
			if (cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").Body.Instructions.FirstOrDefault((Instruction o) => o.OpCode == OpCodes.Ldstr && o.Operand.ToString() == "OnConnectionApproved") == null)
			{
				Debug.LogError("Патчим OnConnectionApproved...");
				this.rest++;
				Instruction instruction = cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").Body.Instructions.FirstOrDefault((Instruction o) => o.OpCode == OpCodes.Stfld && o.Operand.ToString() == "System.Boolean Network.Connection::connected");
				VariableDefinition item = new VariableDefinition(new ArrayType(cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition)));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethod("JoinGame").Body.Variables.Add(item);
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ret));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Beq_S, instruction.Next.Next));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldnull));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Call, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Oxide.Core.dll", "Oxide.Core.Interface").SelectMethod("CallHook"))));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldloc_1));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldstr, "OnConnectionApproved"));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Stelem_Ref));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldarg_1));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldc_I4_0));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldloc_1));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Stloc_1));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Newarr, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition)));
				cilPathing.SelectType(cilPathing._original, ".ServerMgr").SelectMethodIL("JoinGame").InsertAfter(instruction, Instruction.Create(OpCodes.Ldc_I4_1));
				cilPathing.Save(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", delegate(Exception ex)
				{
					Debug.LogError(ex.Message);
				});
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004DDC File Offset: 0x00002FDC
		public void PathEac()
		{
			CilPathing cilPathing = new CilPathing(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", null, null);
			if (cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").Body.Instructions.FirstOrDefault((Instruction o) => o.OpCode == OpCodes.Ldstr && o.Operand.ToString() == "OnHandleClientUpdate") == null)
			{
				Debug.LogError("Патчим OnHandleClientUpdate...");
				this.rest++;
				Instruction instruction = cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").Body.Instructions.FirstOrDefault((Instruction o) => o.OpCode == OpCodes.Leave);
				VariableDefinition variableDefinition = new VariableDefinition(new ArrayType(cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition)));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethod("HandleClientUpdate").Body.Variables.Add(variableDefinition);
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ret));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Beq_S, instruction.Next.Next));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldnull));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Call, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Oxide.Core.dll", "Oxide.Core.Interface").SelectMethod("CallHook"))));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldloc_S, variableDefinition));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldstr, "OnHandleClientUpdate"));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Stelem_Ref));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldarg_1));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldc_I4_0));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldloc_S, variableDefinition));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Stloc_S, variableDefinition));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Newarr, cilPathing._original.MainModule.Import(cilPathing.SelectType(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\mscorlib.dll", "System.Object").tmp_typeDefinition)));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").InsertAfter(instruction, Instruction.Create(OpCodes.Ldc_I4_1));
				cilPathing.SelectType(cilPathing._original, ".EACServer").SelectMethodIL("HandleClientUpdate").Replace(instruction.Previous, Instruction.Create(OpCodes.Brtrue, instruction.Next));
				cilPathing.Save(Path.GetDirectoryName(Assembly.Load("Assembly-CSharp.dll").Location) + "\\Assembly-CSharp.dll", delegate(Exception ex)
				{
					Debug.LogError(ex.Message);
				});
			}
		}

		// Token: 0x04000024 RID: 36
		private int rest;
	}
}
