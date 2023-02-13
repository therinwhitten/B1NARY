namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	[Serializable]
	public class ScriptElement : ScriptNode, IEnumerable<ScriptNode>
	{
		/// <summary>
		/// Lines that are contained in the scriptNode. This variation is for 
		/// all the lines, even with ones that are part of another <see cref="ScriptElement"/>
		/// </summary>
		public IReadOnlyList<ScriptNode> Lines { get; }
		public int ToGlobal(int localPoint)
		{
			return Lines[localPoint].PrimaryLine.Index + localPoint;
		}
		public int ToLocal(int globalPoint)
		{
			return globalPoint - Lines[0].PrimaryLine.Index;
		}


		/// <summary>
		/// Lines that are contained in the scriptNode, separated with other
		/// <see cref="ScriptElement"/>s. Useful for systems like <see cref="IEnumerator{T}"/>.
		/// </summary>
		public IReadOnlyList<ScriptNode> LinesWithElements { get; }
		public override int Length { get; }

		internal ScriptElement(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(blockNodeData[0])
		{
			const int startBracketSkip = 2,
				endBracketSkip = 1;
			List<ScriptNode> lines = new List<ScriptNode>(blockNodeData.Count - startBracketSkip - endBracketSkip);
			List<ScriptNode> linesWithElements = new List<ScriptNode>();
			// Index as 2 for skipping starting line & bracket
			for (int i = startBracketSkip; i < blockNodeData.Count - endBracketSkip;)
			{
				ScriptNode node = new ScriptNode(blockNodeData[i]) { Parent = this };
				// Creating element instead of node.
				if (i + 1 < blockNodeData.Count && blockNodeData[i + 1].Type == ScriptLine.LineType.BeginIndent)
				{
					List<ScriptLine> subLines = new List<ScriptLine>(startBracketSkip + endBracketSkip)
					{
						blockNodeData[0],
						blockNodeData[1],
					};
					for (int layers = 1, ii = 2; layers > 0; ii++)
					{
						subLines.Add(blockNodeData[i + ii]);
						ScriptLine LastLine() => subLines[subLines.Count - 1];
						if (LastLine().Type == ScriptLine.LineType.BeginIndent)
							layers++;
						else if (LastLine().Type == ScriptLine.LineType.EndIndent)
							layers--;
					}
					ScriptElement element = config.GetDefinedElement(subLines);
					for (int ii = 0; ii < element.Lines.Count; ii++)
					{
						lines.Add(new ScriptElementPointer(element, ii, subLines[ii + 2]));
					}
					i += element.Length;
					node = element;
				}
				else
				{
					lines.Add(node);
				}
				linesWithElements.Add(node);
				i += node.Length;
			}
			LinesWithElements = linesWithElements;
			Lines = lines;
			Length = Lines.Count
				// Start Element
				+ 1
				// End Element
				+ 1
				// Root Line
				+ 1;
		}

		public virtual IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			for (int i = localIndex; i < LinesWithElements.Count; i++)
			{
				if (LinesWithElements[i] is ScriptElement element)
				{
					using (var enumerator = element.EnumerateThrough(0))
						while (enumerator.MoveNext())
							yield return enumerator.Current;
					continue;
				}
				yield return LinesWithElements[i];
			}
		}

		public override string ToString()
		{
			return $"{PrimaryLine}\n{{\n{string.Join("\n\t", Lines)}\n}}";
		}

		public virtual IEnumerator<ScriptNode> GetEnumerator()
		{
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines[i] is ScriptElement element)
					using (var subEnumerator = element.GetEnumerator())
						while (subEnumerator.MoveNext())
							yield return subEnumerator.Current;
				yield return Lines[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class ScriptElementPointer : ScriptNode
	{
		public readonly ScriptElement target;
		public readonly int localPoint;

		internal ScriptElementPointer(ScriptElement targetElement, int localPoint, ScriptLine rootLine) : base(rootLine)
		{
			target = targetElement;
			this.localPoint = localPoint;
		}

		public int GlobalPoint => target.Lines[localPoint].PrimaryLine.Index;
	}
}
