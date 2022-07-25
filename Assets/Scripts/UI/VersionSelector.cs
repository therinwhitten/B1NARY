using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionSelector : MonoBehaviour
{
	[SerializeField]
	private string prefix;
	private Text text;
	private void Awake() => text = GetComponent<Text>();
	private void Start()
	{
		text.text = $"{prefix}. {Application.version}";
	}
}
