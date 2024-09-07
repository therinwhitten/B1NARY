namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;

	public sealed class ScriptDocumentConfig
	{
		public bool stopOnAllLines = false;
		#region Attributes
		/// <summary>
		/// Any listeners listening for <c>[AttributeName]</c> where attributeName
		/// is the parameter.
		/// </summary>
		public event Action<string> AttributeListeners;
		internal void InvokeAttribute(string attribute) => AttributeListeners?.Invoke(attribute);
		internal void InvokeAttribute(ScriptLine attribute) => InvokeAttribute(ScriptLine.CastAttribute(attribute));
		#endregion
		#region Entry
		/// <summary>
		/// Any listeners listening for <c>[AttributeName]</c> where attributeName
		/// is the parameter.
		/// </summary>
		public event Action<string> EntryListeners;
		internal void InvokeEntry(string attribute) => EntryListeners?.Invoke(attribute);
		internal void InvokeEntry(ScriptLine attribute) => InvokeAttribute(ScriptLine.CastEntry(attribute));
		#endregion

		public CommandArray Commands { get; }
		#region Normal
		public event Action<ScriptLine> NormalLine;
		internal void InvokeNormal(ScriptLine line) => NormalLine?.Invoke(line);
		#endregion
		// temp change
		public void AddConstructor<TElement>(Predicate<List<ScriptLine>> predicate) 
			where TElement : ScriptElement
			=> AddConstructor(typeof(TElement), predicate);

		public void AddConstructor(Type elementType, Predicate<List<ScriptLine>> predicate)
		{
			if (!elementType.IsSubclassOf(typeof(ScriptElement)))
				throw new InvalidOperationException();
			ConstructorInfo info = elementType.GetConstructor(new Type[] { typeof(ScriptDocumentConfig), typeof(List<ScriptLine>) });
			if (info == null)
				throw new InvalidProgramException($"{elementType.Name} doesn't have a constructor with {nameof(ScriptDocumentConfig)} and {nameof(List<ScriptLine>)}!");
			var newConstructor = new ScriptElementConstructorInfo(predicate, info);
			if (!m_derivedElements.Contains(newConstructor))
				m_derivedElements.Add(newConstructor);
		}

		public ScriptElement GetDefinedElement(List<ScriptLine> lines)
		{
			object[] GetValues() => new object[] { this, lines };
			for (int ii = 0; ii < DerivedElements.Count; ii++)
			{
				if (DerivedElements[ii].predicate.Invoke(lines))
				{
					try { return (ScriptElement)DerivedElements[ii].constructor.Invoke(GetValues()); }
					catch (Exception ex) { Debug.LogException(ex); }
				}
			}
			return new ScriptElement(this, lines);
		}
		public IReadOnlyList<ScriptElementConstructorInfo> DerivedElements => m_derivedElements;
		private readonly List<ScriptElementConstructorInfo> m_derivedElements;

		public ScriptDocumentConfig()
		{
			Commands = new CommandArray();
			m_derivedElements = new List<ScriptElementConstructorInfo>();
		}
		public ScriptDocumentConfig(ScriptDocumentConfig config)
		{
			AttributeListeners = config.AttributeListeners;
			Commands = new CommandArray();
			Commands.AddRange(config.Commands.Commands);
			m_derivedElements = new List<ScriptElementConstructorInfo>();
		}
	}

	// Generated from C# engine
	public readonly struct ScriptElementConstructorInfo
	{
		public readonly Predicate<List<ScriptLine>> predicate;
		public readonly ConstructorInfo constructor;

		public ScriptElementConstructorInfo(Predicate<List<ScriptLine>> predicate, ConstructorInfo constructor)
		{
			this.predicate = predicate;
			this.constructor = constructor;
		}

		public override bool Equals(object obj)
		{
			return obj is ScriptElementConstructorInfo other &&
				   EqualityComparer<Predicate<List<ScriptLine>>>.Default.Equals(predicate, other.predicate) &&
				   EqualityComparer<ConstructorInfo>.Default.Equals(constructor, other.constructor);
		}

		public override int GetHashCode()
		{
			int hashCode = 991353629;
			hashCode = hashCode * -1521134295 + EqualityComparer<Predicate<List<ScriptLine>>>.Default.GetHashCode(predicate);
			hashCode = hashCode * -1521134295 + EqualityComparer<ConstructorInfo>.Default.GetHashCode(constructor);
			return hashCode;
		}

		public void Deconstruct(out Predicate<List<ScriptLine>> predicate, out ConstructorInfo constructor)
		{
			predicate = this.predicate;
			constructor = this.constructor;
		}

		public static implicit operator (Predicate<List<ScriptLine>> predicate, ConstructorInfo constructor)(ScriptElementConstructorInfo value)
		{
			return (value.predicate, value.constructor);
		}

		public static implicit operator ScriptElementConstructorInfo((Predicate<List<ScriptLine>> predicate, ConstructorInfo constructor) value)
		{
			return new ScriptElementConstructorInfo(value.predicate, value.constructor);
		}
	}
}