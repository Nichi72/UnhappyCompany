using Cinemachine;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		// public GameObject PlayerCameraRoot;

		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;


		public StarterAssetsInputs _input;


		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// 
		float weightSpeedAmount = 0.01f;
		float weightSpeed = 1f;
		public CinemachineVirtualCamera cinemachineVirtualCamera;


#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private PlayerStatus _playerStatus;

		[Header("Hand Follow")]
		public Transform pivotNoneModelHandTransform;   // 손 오브젝트의 Transform
		public Transform pivotModelHandTransform;   // 왼손 오브젝트의 Transform
		[ReadOnly] [SerializeField] private Transform currentPivotHandTransform;   // 손 오브젝트의 Transform
		public float handSmoothSpeed = 5.0f;  // 손 회전의 부드러움 정도

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private float originalMoveSpeed;
		private float originalSprintSpeed;

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerStatus = GetComponent<PlayerStatus>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			originalMoveSpeed = MoveSpeed;
			originalSprintSpeed = SprintSpeed;
		}

		private void Update()
		{
			if(!_input.cursorInputForLook)
			{
				_input.look = Vector2.zero;
			}
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
			HandFollowCamera(); // 손 회전 업데이트
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// if sprinting, reduce stamina
			if (_input.sprint && _playerStatus != null && _playerStatus.CanRunOrJump())
			{
				_playerStatus.ReduceStamina(_playerStatus.StaminaReduce * Time.deltaTime);
			}
			else
			{
				_playerStatus.StopConsumingStamina();
				targetSpeed = MoveSpeed; // reduce speed if stamina is depleted
			}

			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * ((_speed * weightSpeed) * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}
		private void JumpAndGravity()
		{
			if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;

				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				if (_input.jump && _jumpTimeoutDelta <= 0.0f && _playerStatus.CanRunOrJump())
				{
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
					_playerStatus.ReduceStamina(_playerStatus.StaminaJumpReduce);
				}

				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				_jumpTimeoutDelta = JumpTimeout;
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				_input.jump = false;
			}

			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
		public void SetSpeedBasedOnWeight(float weight)
		{
			// 10이면 속도를 1/10으로 만든다.
			float weightSpeedTemp = weight * weightSpeedAmount;
			weightSpeed = Mathf.Lerp(1, 0, weightSpeedTemp);
			// 10 * 0.01f = 0.1
			// 50 * 0.01f = 0.5
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		
		public void TakeDamage(float damage)
		{
			if (_playerStatus != null)
			{
				_playerStatus.ReduceHealth(damage);
			}
		}

		/// <summary>
		/// Changes the target that the Cinemachine camera follows with smooth transition.
		/// </summary>
		/// <param name="newTarget">The new GameObject for the camera to follow.</param>
		public void SmoothChangeCinemachineCameraTarget(GameObject newTarget)
		{
			if (newTarget != null)
			{
				// Get the Cinemachine3rdPersonFollow component
				var thirdPersonFollow = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
				if (thirdPersonFollow != null)
				{
					// Set damping for smooth transition
					thirdPersonFollow.Damping.x = 1.0f;
					thirdPersonFollow.Damping.y  = 1.0f;
					thirdPersonFollow.Damping.z  = 1.0f;
				}

				// Change the follow target
				cinemachineVirtualCamera.Follow = newTarget.transform;
			}
			else
			{
				Debug.LogWarning("New target is null. CinemachineCameraTarget not changed.");
			}
		}
		public void ResetCinemachineCameraDamping()
		{
			var thirdPersonFollow = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
			if (thirdPersonFollow != null)
			{
				thirdPersonFollow.Damping.x = 0f;
				thirdPersonFollow.Damping.y  = 0f;
				thirdPersonFollow.Damping.z  = 0f;
			}
		}

		/// <summary>
		/// Smoothly rotates the camera to the specified rotation.
		/// </summary>
		/// <param name="targetRotation">The target rotation in Euler angles.</param>
		/// <param name="duration">Duration of the rotation in seconds.</param>
		/// <param name="onComplete">Optional callback when rotation is complete.</param>
		public void SmoothRotateCinemachineCamera(Vector3 targetRotation, float duration = 1.0f, System.Action onComplete = null)
		{
			StartCoroutine(SmoothRotateCameraCoroutine(targetRotation, duration, onComplete));
		}

		/// <summary>
		/// Smoothly rotates the camera to the specified rotation using quaternion.
		/// </summary>
		/// <param name="targetRotation">The target rotation as a Quaternion.</param>
		/// <param name="duration">Duration of the rotation in seconds.</param>
		/// <param name="onComplete">Optional callback when rotation is complete.</param>
		public void SmoothRotateCinemachineCamera(Quaternion targetRotation, float duration = 1.0f, System.Action onComplete = null)
		{
			StartCoroutine(SmoothRotateCameraCoroutine(targetRotation, duration, onComplete));
		}

		private IEnumerator SmoothRotateCameraCoroutine(Vector3 targetRotation, float duration, System.Action onComplete)
		{
			// 시작 회전값
			Vector3 startRotation = CinemachineCameraTarget.transform.localRotation.eulerAngles;
			
			// 시작 피치 값 계산
			float startPitch = startRotation.x;
			if (startPitch > 180f) startPitch -= 360f; // 180도 이상일 경우 음수로 변환
			
			// 타겟 피치 값 계산
			float targetPitch = targetRotation.x;
			if (targetPitch > 180f) targetPitch -= 360f; // 180도 이상일 경우 음수로 변환
			targetPitch = Mathf.Clamp(targetPitch, BottomClamp, TopClamp); // 카메라 각도 제한 적용
			
			float startTime = Time.time;
			float endTime = startTime + duration;
			
			while (Time.time < endTime)
			{
				float t = (Time.time - startTime) / duration;
				float smoothT = Mathf.SmoothStep(0, 1, t); // 더 부드러운 보간을 위한 SmoothStep
				
				// x축(피치) 회전 값을 _cinemachineTargetPitch에 저장하여 CameraRotation 메서드와 동기화
				_cinemachineTargetPitch = Mathf.Lerp(startPitch, targetPitch, smoothT);
				// _cinemachineTargetPitch 값 제한
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				
				// 카메라 피치 적용
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				
				// y축(요) 회전 적용 - 플레이어 자체를 회전
				Vector3 currentYaw = transform.eulerAngles;
				float newYaw = Mathf.Lerp(currentYaw.y, targetRotation.y, smoothT);
				transform.rotation = Quaternion.Euler(currentYaw.x, newYaw, currentYaw.z);
				
				yield return null;
			}
			
			// 최종 회전값 정확히 설정
			_cinemachineTargetPitch = targetPitch;
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
			
			// y축 회전 최종 적용
			Vector3 finalYaw = transform.eulerAngles;
			transform.rotation = Quaternion.Euler(finalYaw.x, targetRotation.y, finalYaw.z);
			
			// 콜백 호출
			onComplete?.Invoke();
		}

		private IEnumerator SmoothRotateCameraCoroutine(Quaternion targetRotation, float duration, System.Action onComplete)
		{
			// 시작 회전값
			Quaternion startCameraRotation = CinemachineCameraTarget.transform.localRotation;
			Quaternion startPlayerRotation = transform.rotation;
			
			// 타겟 회전값에서 오일러 각도를 추출
			Vector3 targetEuler = targetRotation.eulerAngles;
			
			// 카메라 피치 값 (_cinemachineTargetPitch를 위해)
			float targetPitch = targetEuler.x;
			if (targetPitch > 180f) targetPitch -= 360f; // 180도 이상일 경우 음수로 변환
			targetPitch = Mathf.Clamp(targetPitch, BottomClamp, TopClamp); // 카메라 각도 제한 적용
			
			// 현재 피치 값 (시작점) 계산
			float startPitch = startCameraRotation.eulerAngles.x;
			if (startPitch > 180f) startPitch -= 360f; // 180도 이상일 경우 음수로 변환
			
			// 플레이어 요(Yaw) 회전 (y축)용 쿼터니언
			Quaternion targetPlayerRotation = Quaternion.Euler(0f, targetEuler.y, 0f);
			
			// 시간 관련 변수
			float startTime = Time.time;
			float endTime = startTime + duration;
			
			// 부드러운 회전 적용
			while (Time.time < endTime)
			{
				float t = (Time.time - startTime) / duration;
				float smoothT = Mathf.SmoothStep(0, 1, t); // 더 부드러운 보간을 위한 SmoothStep
				
				// 현재 피치 값 계산 및 적용 (직접 오일러 각도를 보간하지 않고 시작과 목표 피치 값을 보간)
				_cinemachineTargetPitch = Mathf.Lerp(startPitch, targetPitch, smoothT);
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				
				// 카메라 타겟 회전 적용 (피치만)
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);
				
				// 플레이어 회전 적용 (요만)
				transform.rotation = Quaternion.Slerp(startPlayerRotation, targetPlayerRotation, smoothT);
				
				yield return null;
			}
			
			// 최종 회전값 정확히 설정
			_cinemachineTargetPitch = targetPitch;
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);
			transform.rotation = targetPlayerRotation;
			
			// 콜백 호출
			onComplete?.Invoke();
		}

		public float handClampAngle = 45f;
		private void HandFollowCamera()
		{
			if (currentPivotHandTransform != null && _mainCamera != null)
			{
				// Quaternion targetRotation = _mainCamera.transform.rotation * Quaternion.Euler(new Vector3(0f, -34.203f, 0f));
				Quaternion targetRotation = _mainCamera.transform.rotation;
				//카메라의 회전 각도를 제한
				Vector3 eulerRotation = targetRotation.eulerAngles;
				if (eulerRotation.x > 180f) eulerRotation.x -= 360f; // 180도 이상일 경우 360도를 빼서 음수로 변환
				eulerRotation.x = Mathf.Clamp(eulerRotation.x, -handClampAngle, handClampAngle); // 위아래 회전 각도 제한
				targetRotation = Quaternion.Euler(eulerRotation);

				currentPivotHandTransform.rotation = Quaternion.Slerp(currentPivotHandTransform.rotation, targetRotation, handSmoothSpeed * Time.deltaTime);
			}
		}

		public void ModifySpeed(float multiplier)
		{
			MoveSpeed = originalMoveSpeed * multiplier;
			SprintSpeed = originalSprintSpeed * multiplier;
		}

		public void ResetSpeed()
		{
			MoveSpeed = originalMoveSpeed;
			SprintSpeed = originalSprintSpeed;
		}

		public void SetPivotHandTransform(bool isModelHandAnimation)
		{
			if(isModelHandAnimation)
			{
				currentPivotHandTransform = pivotModelHandTransform;
			}
			else
			{
				currentPivotHandTransform = pivotNoneModelHandTransform;
			}
		}


		/// <summary>
		/// 현재 카메라 방향을 기준으로 특정 방향을 바라보도록 회전합니다.
		/// x축 회전은 0으로 초기화하여 수평을 유지합니다.
		/// </summary>
		public void LookAtWithQuaternion()
		{
			// 현재 카메라 회전에서 Quaternion 생성
			Quaternion currentRotation = _mainCamera.transform.rotation;
			
			// 카메라의 현재 오일러 각도 가져오기
			Vector3 eulerAngles = currentRotation.eulerAngles;
			
			// x축 회전(피치)을 0으로 설정하여 수평을 유지
			eulerAngles.x = 0f;
			
			// 수정된 오일러 각도로 새 쿼터니언 생성
			Quaternion targetRotation = Quaternion.Euler(eulerAngles);
			
			// 쿼터니언 기반 회전 메서드 호출
			SmoothRotateCinemachineCamera(targetRotation, 1f);
		}
		
		public IEnumerator LookAtCoroutine(Vector3 targetEulerAngles)
		{
			Quaternion startRotation = _mainCamera.transform.rotation;
			Quaternion endRotation = Quaternion.Euler(targetEulerAngles);
			
			float rotationSpeed = 500f;
			float threshold = 0.01f;
			
			while (Quaternion.Angle(_mainCamera.transform.rotation, endRotation) > threshold)
			{
				_mainCamera.transform.rotation = Quaternion.Slerp(_mainCamera.transform.rotation, endRotation, rotationSpeed * Time.deltaTime);
				Debug.Log("LookAtCoroutine: " + _mainCamera.transform.rotation.eulerAngles);
				yield return null;
			}
			
			// 최종 회전값 정확히 설정
			_mainCamera.transform.rotation = endRotation;
		}
	}

	

}
