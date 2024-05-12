using Assets.draco18s.bulletboss.input;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public static InputManager Instance;
	private PlayerInputActions _input;

	public static PlayerInputActions.PatternEditorActions PatternEditor => Instance._input.PatternEditor;

	protected void Awake()
	{
		Instance = this;
		_input = new PlayerInputActions();
		PatternEditor.Enable();
	}
}
