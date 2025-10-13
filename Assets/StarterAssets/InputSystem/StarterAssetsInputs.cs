using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Runtime State")]
		[SerializeField] private bool isFrozen;
		public bool IsFrozen => isFrozen;

		[Header("Buffered Inputs (applied on unfreeze)")]
		[SerializeField] private Vector2 bufferedMove;
		[SerializeField] private Vector2 bufferedLook;
		[SerializeField] private bool bufferedSprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			Vector2 v = value.Get<Vector2>();
			bufferedMove = v;
			if (isFrozen) return;
			MoveInput(v);
		}

		public void OnLook(InputValue value)
		{
			Vector2 v = value.Get<Vector2>();
			bufferedLook = v;
			if(!isFrozen && cursorInputForLook)
			{
				LookInput(v);
			}
		}

		public void OnJump(InputValue value)
		{
			if (isFrozen) return;
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			bool pressed = value.isPressed;
			bufferedSprint = pressed;
			if (isFrozen) return;
			SprintInput(pressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			if (isFrozen) return;
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			if (isFrozen) return;
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			if (isFrozen) return;
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			if (isFrozen) return;
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
		public void SetCursorLock(bool newState, bool isCursorCenter = true)
		{
			cursorLocked = newState;
			cursorInputForLook = newState;
			SetCursorState(newState);
			Cursor.visible = !newState;

			if(isCursorCenter)
			{
				Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
				Mouse.current.WarpCursorPosition(screenCenter);
			}
		}

		public void FreezePlayerInput(bool freeze)
		{
			isFrozen = freeze;
			if(freeze)
			{
				move = Vector2.zero;
				look = Vector2.zero;
				jump = false;
				sprint = false;
				
				cursorInputForLook = false;
				// SetCursorState(false);
				Cursor.visible = true;
			}
			else
			{
				cursorInputForLook = true;
				// SetCursorState(true);
				Cursor.visible = false;
				// Apply buffered inputs so held keys take effect immediately
				move = bufferedMove;
				look = bufferedLook;
				sprint = bufferedSprint;
			}
		}
	}
	
}