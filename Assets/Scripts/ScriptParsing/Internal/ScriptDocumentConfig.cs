namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	public sealed class ScriptDocumentConfig
	{
		public bool stopOnAllLines = false;
		/// <summary>
		/// Any listeners listening for <c>[AttributeName]</c> where attributeName
		/// is the parameter.
		/// </summary>
		public event Action<string> AttributeListeners;
		internal void InvokeAttribute(string attribute) => AttributeListeners?.Invoke(attribute);
		internal void InvokeAttribute(ScriptLine attribute) => InvokeAttribute(ScriptLine.CastAttribute(attribute));
		public CommandArray Commands { get; }
		public event Action NormalLine;
		internal void InvokeNormal() => NormalLine?.Invoke();
		// temp change
		public void AddConstructor<TElement>(Func<List<ScriptLine>, bool> predicate) 
			where TElement : ScriptElement
			=> AddConstructor(typeof(TElement), predicate);

		public void AddConstructor(Type elementType, Func<List<ScriptLine>, bool> predicate)
		{
			if (!elementType.IsSubclassOf(typeof(ScriptElement)))
				throw new InvalidOperationException();
			ConstructorInfo info = elementType.GetConstructor(new Type[] { typeof(ScriptDocumentConfig), typeof(List<ScriptLine>) });
			var newConstructor = new ScriptElementConstructorInfo(predicate, info);
			if (!m_derivedElements.Contains(newConstructor))
				m_derivedElements.Add(newConstructor);
		}

		public ScriptElement GetDefinedElement(List<ScriptLine> lines)
		{
			for (int ii = 0; ii < DerivedElements.Count; ii++)
			{
				if (DerivedElements[ii].predicate.Invoke(lines))
					return (ScriptElement)DerivedElements[ii].constructor.Invoke(new object[] { this, lines });
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
		public readonly Func<List<ScriptLine>, bool> predicate;
		public readonly ConstructorInfo constructor;

		public ScriptElementConstructorInfo(Func<List<ScriptLine>, bool> predicate, ConstructorInfo constructor)
		{
			this.predicate = predicate;
			this.constructor = constructor;
		}

		public override bool Equals(object obj)
		{
			return obj is ScriptElementConstructorInfo other &&
				   EqualityComparer<Func<List<ScriptLine>, bool>>.Default.Equals(predicate, other.predicate) &&
				   EqualityComparer<ConstructorInfo>.Default.Equals(constructor, other.constructor);
		}

		public override int GetHashCode()
		{
			int hashCode = 991353629;
			hashCode = hashCode * -1521134295 + EqualityComparer<Func<List<ScriptLine>, bool>>.Default.GetHashCode(predicate);
			hashCode = hashCode * -1521134295 + EqualityComparer<ConstructorInfo>.Default.GetHashCode(constructor);
			return hashCode;
		}

		public void Deconstruct(out Func<List<ScriptLine>, bool> predicate, out ConstructorInfo constructor)
		{
			predicate = this.predicate;
			constructor = this.constructor;
		}

		public static implicit operator (Func<List<ScriptLine>, bool> predicate, ConstructorInfo constructor)(ScriptElementConstructorInfo value)
		{
			return (value.predicate, value.constructor);
		}

		public static implicit operator ScriptElementConstructorInfo((Func<List<ScriptLine>, bool> predicate, ConstructorInfo constructor) value)
		{
			return new ScriptElementConstructorInfo(value.predicate, value.constructor);
		}
	}
}