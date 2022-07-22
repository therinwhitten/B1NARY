using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct VoiceLine : IDisposable
{
	public readonly AudioClip voiceLine;
	public VoiceLine(string resourcesVAPath)
	{
		voiceLine = Resources.Load<AudioClip>(resourcesVAPath);
		if (voiceLine == null)
			throw new InvalidOperationException($"Voice Actor line '{resourcesVAPath}' " +
				"does not have an exact filename! Did you leave a space within the filename?");

	}

	// structs cannot contain destructors
	public void Dispose()
	{
		Resources.UnloadAsset(voiceLine);
	}
}
