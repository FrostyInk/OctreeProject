using UnityEngine;

public class Mover : MonoBehaviour {

	public float cameraMoveSpeed = 15f;
	public float cameraRotationSpeed = 120f;

	public bool lockMouse = false;
	public bool hideMouse = false;

	public Color stock;
	public Color highlighted;

	Material recentCubeMaterial;
	Transform recentCubesTransform;

	void Start () {
		Cursor.lockState = lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = hideMouse ? false : true;
	}
	
	void Update () {
		MoveCamera();

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			GameObject newCube = Instantiate(Resources.Load("OctCube")) as GameObject;
			newCube.transform.position = this.transform.position + transform.forward * 5f;
		}

		RaycastHit hit;

		if(Physics.Raycast(transform.position, transform.forward, out hit, 100f)) {
			if(hit.collider.tag == "OctCube") {

				if (recentCubeMaterial != null)
					recentCubeMaterial.color = stock;

				GameObject caught = hit.collider.gameObject;
				Rigidbody caughtRigid = caught.GetComponent<Rigidbody>();
				recentCubeMaterial = caught.GetComponent<Renderer>().material;
				recentCubeMaterial.color = highlighted;

				if (Input.GetKeyDown(KeyCode.Mouse1)) {
					if (recentCubesTransform != null)
						recentCubesTransform.SetParent(null);

					caughtRigid.isKinematic = true;
					recentCubesTransform = caught.transform;
					recentCubesTransform.SetParent(transform);
				}

				if (Input.GetKeyUp(KeyCode.Mouse1)) {
					caughtRigid.isKinematic = false;
					if (recentCubesTransform != null)
					recentCubesTransform.SetParent(null);
				}

				if (Input.GetKeyUp(KeyCode.E)) {
					Destroy(caught);
				}


				if (Input.GetKeyUp(KeyCode.R)) {
					caughtRigid.AddForce(transform.forward * 25f, ForceMode.Impulse);
				}
			}

			return;
		}

		if(recentCubeMaterial != null)
		recentCubeMaterial.color = stock;

		if (recentCubesTransform != null)
			recentCubesTransform.SetParent(null);
	}

	void MoveCamera() {
		transform.Translate(Input.GetAxisRaw("Horizontal") * Time.deltaTime * (cameraMoveSpeed / 2), 0f, Input.GetAxis("Vertical") * Time.deltaTime * cameraMoveSpeed, Space.Self);
		transform.Rotate(0f, Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraRotationSpeed, 0f, Space.World);
		transform.Rotate(-Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraRotationSpeed, 0f, 0f, Space.Self);

		if (Input.GetKey(KeyCode.Space)) {
			transform.Translate(0f, 1f * Time.deltaTime * cameraMoveSpeed, 0f, Space.World);
		}

		if (Input.GetKey(KeyCode.LeftShift)) {
			transform.Translate(0f, -1f * Time.deltaTime * cameraMoveSpeed, 0f, Space.World);
		}
	}
}
