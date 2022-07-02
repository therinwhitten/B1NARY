using System;
using System.Runtime.Serialization;

public class SoundNotFoundException : Exception
{
	public SoundNotFoundException()
	{

	}

	public SoundNotFoundException(string message) : base(message)
	{

	}

	public SoundNotFoundException(string message, Exception innerException) : base(message, innerException)
	{

	}

	protected SoundNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{

	}
}