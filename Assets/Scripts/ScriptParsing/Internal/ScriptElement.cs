namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using UnityEngine;

	public class ScriptElement : ScriptNode, IPointerConverter
	{
		/// <summary>
		/// Lines that are contained in the scriptNode. This variation is for 
		/// all the lines, even with ones that are part of another <see cref="ScriptElement"/>
		/// </summary>
		public IReadOnlyList<ScriptNode> Lines { get; }
		public virtual int ToGlobal(int localPoint)
		{
			return Lines[localPoint].PrimaryLine.Index + localPoint + 1;
		}
		public virtual int ToLocal(int globalPoint)
		{
			return PrimaryLine.Index - globalPoint - 1;
		}

		public virtual IReadOnlyList<ScriptNode> StartBracketSkip { get; }
		public virtual IReadOnlyList<ScriptNode> EndBracketSkip { get; }

		/// <summary>
		/// Lines that are contained in the scriptNode, separated with other
		/// <see cref="ScriptElement"/>s. Useful for systems like <see cref="IEnumerator{T}"/>.
		/// </summary>
		public IReadOnlyList<ScriptNode> LinesWithElements { get; }
		public IReadOnlyList<ScriptElement> Elements { get; }
		public override int Length { get; }

		internal protected ScriptElement(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(blockNodeData[0])
		{
			// Defining brackets
			StartBracketSkip = new List<ScriptNode>(2)
			{
				new ScriptNode(blockNodeData[0]) { Parent = this },
				new ScriptNode(blockNodeData[1]) { Parent = this },
			};
			EndBracketSkip = new List<ScriptNode>(1)
			{
				new ScriptNode(blockNodeData[blockNodeData.Count - 1]) { Parent = this }
			};
			// Adding internal lines.
			List<ScriptNode> lines = new List<ScriptNode>(blockNodeData.Count - StartBracketSkip.Count - EndBracketSkip.Count);
			List<ScriptNode> linesWithElements = new List<ScriptNode>();
			List<ScriptElement> elements = new List<ScriptElement>();
			// Index as 2 for skipping starting line & bracket
			// Count - 1 as to skip the end bracket
			for (int i = StartBracketSkip.Count; i < blockNodeData.Count - EndBracketSkip.Count;)
			{
				// Creating element instead of node. Does this by getting the 
				// - startBracket 1 line in front of it. 
				if (i + 1 < blockNodeData.Count && blockNodeData[i + 1].Type == ScriptLine.LineType.BeginIndent)
				{
					// Gets all contents of the node; The starting line, the brackets,
					// - and the contents of the brackets.
					List<ScriptLine> subLines = new List<ScriptLine>(StartBracketSkip.Count + EndBracketSkip.Count)
					{
						// Adding first two mandatory lines
						blockNodeData[i],
						blockNodeData[i + 1]
					};
					// Assuming all brackets are in line, this will count the
					// - layer count n stuff. Includes the last end bracket
					for (int layers = 1, ii = 2; layers > 0; ii++)
					{
						ScriptLine currentLine = blockNodeData[i + ii];
						subLines.Add(currentLine);
						if (currentLine.Type == ScriptLine.LineType.BeginIndent)
							layers++;
						else if (currentLine.Type == ScriptLine.LineType.EndIndent)
							layers--;
					}
					ScriptElement subElement = config.GetDefinedElement(subLines);
					subElement.Parent = this;
					List<ScriptNode> lineCollection = new List<ScriptNode>{subElement};
					if (subElement.StartBracketSkip.Count > 1)
						lineCollection.AddRange(subElement.StartBracketSkip.Skip(1));
					// Adds pointers to the element stored in the lines.
					for (int ii = 0; ii < subElement.Lines.Count; ii++)
					{
						var pointer = new ScriptElementPointer(subElement, ii, subElement.Lines[ii].PrimaryLine)
						{
							Parent = this
						};
						lineCollection.Add(pointer);
					}
					lineCollection.AddRange(subElement.EndBracketSkip);
					lines.AddRange(lineCollection);
					linesWithElements.Add(subElement);
					elements.Add(subElement);
					i += lineCollection.Count;
				}
				else
				{
					ScriptNode node = new ScriptNode(blockNodeData[i]) { Parent = this };
					lines.Add(node);
					linesWithElements.Add(node);
					i += node.Length;
				}
			}
			LinesWithElements = linesWithElements;
			Lines = lines;
			Elements = elements;
			Length = Lines.Count
				// Start Element w/ Rootline
				+ StartBracketSkip.Count
				// End Element
				+ EndBracketSkip.Count;
		}

		public virtual IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			if (localIndex < 0)
				throw new ArgumentOutOfRangeException($"'{localIndex}' is negative!");
			if (localIndex > Lines.Count)
				throw new ArgumentOutOfRangeException($"'{localIndex}' is higher than the amount of lines of '{Lines.Count}'!");
			if (Lines[localIndex] is ScriptElementPointer pointer)
			{
				using (var enumerator = pointer.target.EnumerateThrough(pointer.localPoint))
					while (enumerator.MoveNext())
						yield return enumerator.Current;
				while (Lines.Count > localIndex && !ReferenceEquals(Lines[localIndex].Parent, this))
					localIndex++;
			}
			for (int i = localIndex; i < LinesWithElements.Count; i++)
			{
				if (LinesWithElements[i] is null)
				{
					throw new InvalidDataException($"Line {ToGlobal(i)} is null.");
				}
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
		/*
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
				if (InvokeLine(currentNode.PrimaryLine) && !documentConfig.stopOnAllLines)
					continue;
				yield return LinesWithElements[i];
			}
		}
		*/

		public override string ToString()
		{
			return $"ELEMENT: {base.ToString()}";
		}
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
