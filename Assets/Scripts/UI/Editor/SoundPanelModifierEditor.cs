namespace B1NARY.Editor
{
	using B1NARY.Audio;
	using UnityEditor;

	[CustomEditor(typeof(SoundPanelModifier))]
	public sealed class SoundPanelModifierEditor : Editor
	{
		private SoundPanelModifier _modifier;
		private void Awake()
		{
			_modifier = (SoundPanelModifier)target;
		}
		private void OnEnable() => EditorUtility.SetDirty(_modifier);
		private void OnDisable()
		{
			if (_modifier != null)
				EditorUtility.ClearDirty(_modifier);
			}
		public override void OnInspectorGUI()
		{
			_modifier.UnityAudioToggle = EditorGUILayout.Toggle("Unity Audiolisteners", _modifier.UnityAudioToggle);
			_modifier.GlobalVolume = EditorGUILayout.Slider("Global Volume", _modifier.GlobalVolume, 0f, 1f);
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SoundPanelModifier.fullMixerGroup)));
			serializedObject.ApplyModifiedProperties();
		}
	}
}