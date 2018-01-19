using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Test : MonoBehaviour {

	InputField myInputField;



	// Use this for initialization
	void Start () {
		myInputField = GetComponent<InputField> ();
		InitInputField ();
		//Debug.Log (myInputField.text);

	}
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKey (KeyCode.A)) {
			myInputField.text = "Rewrite";
		}*/
	}

	void InitInputField(){
		myInputField.text = "Clear";
		myInputField.ActivateInputField ();
	}
}
