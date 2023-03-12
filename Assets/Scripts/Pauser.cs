namespace B1NARY
{
	using System;

	public sealed class Pauser
	{

		public sbyte Count { get; private set; } = 0;
		public bool ShouldPause
		{
			get
			{
				if (ForceState)
					return forcedState.Value;
				return Count > 0;
			}
			set
			{
				if (value)
					Pause();
				else
					Play();
			}
		}
		public void ManuallyForceState(bool pause)
		{
			forcedState = pause;
			m_forceState = true;
		}
		public bool ForceState
		{
			get => m_forceState;
			set
			{
				if (m_forceState == value)
					return;
				if (value)
					forcedState = ShouldPause;
				else
					forcedState = null;
				m_forceState = value;
			}
		}
		private bool m_forceState = false;
		private bool? forcedState;
		public void Pause() => Count = checked((sbyte)(Count + 1));
		public void Play() => Count = checked((sbyte)(Count - 1));
	}
}