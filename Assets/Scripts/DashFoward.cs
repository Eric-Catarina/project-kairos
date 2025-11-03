using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DashForwardInputSystem : MonoBehaviour
{
	[Header("Dash Settings")]
	public float dashForce = 10f;
	public float dashCooldown = 1f;
	public float dashDuration = 0.2f;

	[Header("Input")]
	public InputActionReference dashActionReference;
	public string dashActionName = "Dash";

	[Header("Camera")]
	public Transform cameraTransform;

	private InputAction dashAction;
	private Rigidbody rb;
	private bool isDashing = false;
	private float dashEndTime = 0f;
	private float lastDash = -999f;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();

		if (dashActionReference != null && dashActionReference.action != null)
		{
			dashAction = dashActionReference.action;
		}
		else
		{
			var pi = GetComponent<PlayerInput>();
			if (pi != null && pi.actions != null)
				dashAction = pi.actions.FindAction(dashActionName);
		}
	}

	void OnEnable()
	{
		if (dashAction != null)
		{
			dashAction.started += OnDashTriggered;
			dashAction.Enable();
		}
	}

	void OnDisable()
	{
		if (dashAction != null)
		{
			dashAction.started -= OnDashTriggered;
			dashAction.Disable();
		}
	}

	private void OnDashTriggered(InputAction.CallbackContext ctx)
	{
		if (Time.time >= lastDash + dashCooldown)
			StartDash();
	}

	void Update()
	{
		if (isDashing && Time.time >= dashEndTime)
			EndDash();
	}

	void StartDash()
	{
		isDashing = true;
		lastDash = Time.time;
		dashEndTime = Time.time + dashDuration;

		Vector3 dashDir;
		if (cameraTransform != null)
		{
			dashDir = cameraTransform.forward;
			dashDir.y = 0f;
			dashDir.Normalize();
		}
		else
		{
			dashDir = transform.forward;
		}

		rb.linearVelocity = Vector3.zero;
		rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);
	}

	void EndDash()
	{
		isDashing = false;
		rb.linearVelocity = Vector3.zero;
	}
}
