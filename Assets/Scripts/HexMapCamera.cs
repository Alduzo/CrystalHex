using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that controls the singleton camera that navigates the hex map.
/// </summary>
public class HexMapCamera : MonoBehaviour
{
	[SerializeField]
	float stickMinZoom, stickMaxZoom;

	[SerializeField]
	float swivelMinZoom, swivelMaxZoom;

	[SerializeField]
	float moveSpeedMinZoom, moveSpeedMaxZoom;

	[SerializeField]
	float rotationSpeed;

	Transform swivel, stick;

	float zoom = 1f;
	float rotationAngle;
	static HexMapCamera instance;

	public static bool Locked
	{
		set => instance.enabled = !value;
	}

	public static void ValidatePosition() => instance.AdjustPosition(0f, 0f);

	void Awake()
	{
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);

		// Posición inicial forzada
		swivel.localPosition = new Vector3(0f, 0f, 10f);
		swivel.localRotation = Quaternion.Euler(30f, 0f, 0f);
		stick.localPosition = new Vector3(0f, 0f, -12f);
	}

	void OnEnable()
	{
		instance = this;
		ValidatePosition();
	}

	void Update()
	{
		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f)
		{
			AdjustZoom(zoomDelta);
		}

		float rotationDelta = 0f;
		if (Input.GetKey(KeyCode.Q)) rotationDelta = -1f;
		if (Input.GetKey(KeyCode.E)) rotationDelta = 1f;
		if (rotationDelta != 0f)
		{
			AdjustRotation(rotationDelta);
		}

		float xDelta = 0f;
		if (Input.GetKey(KeyCode.A)) xDelta = -1f;
		if (Input.GetKey(KeyCode.D)) xDelta = 1f;

		float zDelta = 0f;
		if (Input.GetKey(KeyCode.W)) zDelta = 1f;
		if (Input.GetKey(KeyCode.S)) zDelta = -1f;

		if (xDelta != 0f || zDelta != 0f)
		{
			AdjustPosition(xDelta, zDelta);
		}
		if (Input.GetKey(KeyCode.C))
		{
			Transform player = GameObject.FindWithTag("Player")?.transform;
			if (player != null)
			{
				Vector3 pos = player.position;
				pos.y = transform.position.y;
				transform.position = pos;
			}
		}

	}

	void AdjustZoom(float delta)
	{
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	void AdjustRotation (float delta)
	{
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f)
		{
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f)
		{
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

	void AdjustPosition(float xDelta, float zDelta)
	{
		Vector3 direction = new Vector3(xDelta, 0f, zDelta).normalized;
		float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
		float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = position;
	}
}