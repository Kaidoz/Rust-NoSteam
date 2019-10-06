using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Luma.Oxide
{
	// Token: 0x02000003 RID: 3
	public class CilPathing
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002074 File Offset: 0x00000274
		public string _pathOriginal { get; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x0000207C File Offset: 0x0000027C
		public string _pathReference { get; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000004 RID: 4 RVA: 0x00002084 File Offset: 0x00000284
		public AssemblyDefinition _original { get; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000005 RID: 5 RVA: 0x0000208C File Offset: 0x0000028C
		public AssemblyDefinition _reference { get; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002094 File Offset: 0x00000294
		// (set) Token: 0x06000007 RID: 7 RVA: 0x0000209C File Offset: 0x0000029C
		public TypeDefinition tmp_typeDefinition { get; set; }

		// Token: 0x06000008 RID: 8 RVA: 0x00002768 File Offset: 0x00000968
		public CilPathing(string PathOriginal, string PathReference, Action<Exception> CallBack_Exception = null)
		{
			try
			{
				this._pathOriginal = PathOriginal;
				this._original = AssemblyDefinition.ReadAssembly(PathOriginal);
				if (PathReference != null)
				{
					this._pathReference = PathReference;
					this._reference = AssemblyDefinition.ReadAssembly(this._pathReference);
					this.SetReference(this._reference);
				}
			}
			catch (Exception obj)
			{
                CallBack_Exception?.Invoke(obj);
            }
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020A5 File Offset: 0x000002A5
		public void SetReference(AssemblyDefinition assemblyDefinition)
		{
			this._original.MainModule.AssemblyReferences.Add(new AssemblyNameReference(assemblyDefinition.Name.Name, assemblyDefinition.Name.Version));
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002800 File Offset: 0x00000A00
		public CilPathing SelectType(AssemblyDefinition assemblyDefinition, string path)
		{
			path = path.Replace("\n", "").Replace("\r", "").Replace(" ", "");
			if (assemblyDefinition == null)
			{
				return this;
			}
			if (!this._cache_type.ContainsKey(string.Format("{0}.{1}", assemblyDefinition.Name.Name, path)))
			{
				this._cache_type[string.Format("{0}.{1}", assemblyDefinition.Name.Name, path)] = assemblyDefinition.MainModule.Types.FirstOrDefault((TypeDefinition t) => string.Format("{0}.{1}", t.Namespace, t.Name) == path);
			}
			this.tmp_typeDefinition = this._cache_type[string.Format("{0}.{1}", assemblyDefinition.Name.Name, path)];
			return this;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000020D7 File Offset: 0x000002D7
		public CilPathing SelectType(string assemblyPath, string path)
		{
			return this.SelectType(AssemblyDefinition.ReadAssembly(assemblyPath), path);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000028F0 File Offset: 0x00000AF0
		public MethodDefinition SelectMethod(string name)
		{
			name = name.Replace("\n", "").Replace("\r", "").Replace(" ", "");
			if (this.tmp_typeDefinition == null)
			{
				return null;
			}
			if (!this._cache_method.ContainsKey(string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)))
			{
				this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] = this.tmp_typeDefinition.Methods.FirstOrDefault((MethodDefinition m) => m.Name == name);
				this._cache_ILProcessor[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] = ((this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] != null) ? this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)].Body.GetILProcessor() : null);
			}
			return this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)];
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002AA4 File Offset: 0x00000CA4
		public ILProcessor SelectMethodIL(string name)
		{
			name = name.Replace("\n", "").Replace("\r", "").Replace(" ", "");
			if (this.tmp_typeDefinition == null)
			{
				return null;
			}
			if (!this._cache_ILProcessor.ContainsKey(string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)))
			{
				this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] = this.tmp_typeDefinition.Methods.FirstOrDefault((MethodDefinition m) => m.Name == name);
				this._cache_ILProcessor[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] = ((this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] != null) ? this._cache_method[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)].Body.GetILProcessor() : null);
			}
			return this._cache_ILProcessor[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)];
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002C58 File Offset: 0x00000E58
		public FieldDefinition SelectField(string name)
		{
			if (this.tmp_typeDefinition == null)
			{
				return null;
			}
			if (!this._cache_field.ContainsKey(string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)))
			{
				this._cache_field[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)] = this.tmp_typeDefinition.Fields.FirstOrDefault((FieldDefinition m) => m.Name == name);
			}
			return this._cache_field[string.Format("{0}.{1}.{2}", this.tmp_typeDefinition.Namespace, this.tmp_typeDefinition.Name, name)];
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002D30 File Offset: 0x00000F30
		public bool Save(string EndPath, Action<Exception> CallBack_Exception = null)
		{
			try
			{
				this._original.Write(EndPath);
				return true;
			}
			catch (Exception obj)
			{
                CallBack_Exception?.Invoke(obj);
            }
			return false;
		}

		// Token: 0x0400000A RID: 10
		private Dictionary<string, TypeDefinition> _cache_type = new Dictionary<string, TypeDefinition>();

		// Token: 0x0400000B RID: 11
		private Dictionary<string, MethodDefinition> _cache_method = new Dictionary<string, MethodDefinition>();

		// Token: 0x0400000C RID: 12
		private Dictionary<string, ILProcessor> _cache_ILProcessor = new Dictionary<string, ILProcessor>();

		// Token: 0x0400000D RID: 13
		private Dictionary<string, FieldDefinition> _cache_field = new Dictionary<string, FieldDefinition>();
	}
}
