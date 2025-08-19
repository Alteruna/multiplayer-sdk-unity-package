using UnityEngine;
using Alteruna.Multiplayer;

[RequireComponent(typeof(NewInputSync))]
public class NewInputSyncTest : MonoBehaviour
{
	public float MoveSpeed = 5f;

	// Type specific action
	private NewInputSync.InputActionSync.Action<float> _horizontal;

	// Unspecified type action
	private NewInputSync.InputActionSync _vertical;

	private void Start()
	{
		var inputSync = GetComponent<NewInputSync>();
		// Get casted action
		_horizontal = inputSync.FindAction<float>("Horizontal");
		// Get unspecified type action
		_vertical = inputSync.FindAction("Vertical");
	}

	private void Update()
	{
		transform.Translate(
			// Get value directly from the action
			_horizontal.GetValue() * MoveSpeed * Time.deltaTime,
			// Attempt to get value from the unspecified type action as float
			_vertical.GetValue<float>() * MoveSpeed * Time.deltaTime,
			0
		);
	}
}