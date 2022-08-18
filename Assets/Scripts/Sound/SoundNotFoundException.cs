namespace B1NARY.Audio
{
	using System;
	using System.Runtime.Serialization;

	internal class SoundNotFoundException : Exception
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
}