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

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			if (isFrozen) return;
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(!isFrozen && cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (isFrozen) return;
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			if (isFrozen) return;
			SprintInput(value.isPressed);
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
			}
		}
	}
	
}