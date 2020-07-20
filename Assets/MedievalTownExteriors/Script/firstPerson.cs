using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
public class firstPerson : MonoBehaviour {

	private bool freeLook;

	RaycastHit hit;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		freeLook = true;
	}

	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire2")) {
			if (Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
			} else {
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		if (Cursor.lockState == CursorLockMode.None) {
			return;
		}
		
		int layerMask = 1 << 8;
		layerMask = ~layerMask;

		//RaycastHit hit;
		Vector3 directionRegard, directionTorse;

		if (Input.GetKey("q") && !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, 1, layerMask)) {
			transform.Translate(20 * Vector3.left * Time.deltaTime);
		} else if (Input.GetKey("d")  && !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, 1, layerMask)) {
			transform.Translate(20 * Vector3.right * Time.deltaTime);
		}

		if (Input.GetKey("z")) {
			if (freeLook && !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 1, layerMask)) {
				transform.Translate(20 * Vector3.forward * Time.deltaTime);
			} else {
				directionRegard = transform.TransformDirection(Vector3.forward);
				directionTorse = new Vector3(directionRegard.x,0,directionRegard.z);

				if (!Physics.Raycast (transform.position, directionTorse, out hit, 1, layerMask)) {
					transform.position += 20 * (directionTorse) * Time.deltaTime;
				}
			}
		} else if (Input.GetKey("s")) {
			if (freeLook && !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, 1, layerMask)) {
				transform.Translate(20 * Vector3.back * Time.deltaTime);
			} else {
				directionRegard = transform.TransformDirection(Vector3.back);
				directionTorse = new Vector3(directionRegard.x,0,directionRegard.z);

				if (!Physics.Raycast (transform.position, directionTorse, out hit, 1, layerMask)) {
					transform.position += 20 * (directionTorse) * Time.deltaTime;
				}
			}
		}

		directionRegard = transform.TransformDirection(Vector3.forward);
		directionTorse = new Vector3(directionRegard.x,0,directionRegard.z);

		transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * 40 * Time.deltaTime);Quaternion oldRotate = transform.rotation;
		transform.RotateAround(transform.position, transform.TransformDirection (Vector3.left), Input.GetAxis ("Mouse Y") * 40 * Time.deltaTime);

		if (transform.eulerAngles.x>80 && transform.eulerAngles.x<270) {
			transform.rotation = oldRotate;
			//transform.RotateAround (transform.position, transform.TransformDirection (Vector3.left), Input.GetAxis ("Mouse Y") * -40 * Time.deltaTime);
		}
	}
}
