using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;

public class InputString : MonoBehaviour {

	public UH uhand;
	bool openCloseCheckFlag = true;
	bool indexCheckFlag = true;


	bool doubleCheckFlag = false;

	GameObject screenObj;
	GameObject screenObjBuffer;
	Text inputText;
	Text inputTextBuffer;

	//手の開閉
	public int sum;


	//Use this for initialization
	void Start () {
		
		screenObj = GameObject.FindGameObjectWithTag("InputTarget");
		inputText = screenObj.GetComponent<Text>();

	}
	
	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate(){
		
		//Debug.Log (indexCheck(uhand.UHPR[0],uhand.UHPR[1]));
		//transform.rotation = new Quaternion (100, 10, 10, 40);
		//Debug.Log (uhand.UHQuaternion[0]);
		openCloseCheck(uhand.UHPR[0],uhand.UHPR[1]);
		indexCheck (uhand.UHPR[0],uhand.UHPR[1]);
		InputCharacter();

	}

	//1つのフォトリフレクタの値をz値に変換する関数
	float standardize(float x,float mean,float variance){
		//mean_aとvariance_aは決め打ちで入力

		float z = (x - mean) / variance;
		return z;

	}

	//手の開閉をチェックする関数
	bool openCloseCheck(float PHa,float PHb){
		//openならtrue、closeならfalseとする

		//分散と平均を打ち込む
		float PHa_std = standardize (PHa,235.6875f,128.8398f);
		float PHb_std = standardize (PHb,252.05f, 72.285f);

		float w_0 = 0.0306945861636f;
		float w_1 = 0.770256347546f;
		float w_2 = -0.342680451038f;

		float check_y = (-w_1 * PHa_std - w_0)/w_2;


		if (PHb_std < check_y) {
			openCloseCheckFlag = false;
		} else {
			openCloseCheckFlag = true;
		}

		doubleCheckFlag = false;

		return openCloseCheckFlag;
	}


	bool indexCheck(float PHa,float PHb){

		//第２,３引数は適当に打ったからあとで調整
		float PHa_std = standardize (PHa,212.18f,340.88f);
		float PHb_std = standardize (PHb,257.87f,5.44f);

		float w_0 = -0.08f;
		float w_1 = -0.88f;
		float w_2 = -0.05f;

		float check_y = (-w_1 * PHa_std - w_0) / w_2;

		//Debug.Log (PHb_std + " " + check_y);

		if (PHb_std < check_y) {

			indexCheckFlag = false;

		} else {
			indexCheckFlag = true;
		}

		return indexCheckFlag;
	
	}
		
	string inputone;
	int i = 0;
	bool inputFlag = true;


	void InputCharacter(){

		string[] inputChar = new string[] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"} ;
		Debug.Log (indexCheckFlag + " " + doubleCheckFlag + " " + openCloseCheckFlag);
		if (indexCheckFlag == true && doubleCheckFlag == false && openCloseCheckFlag == true) {
			
			inputone = inputChar[i];
			i += 1;
			inputFlag = false;
			doubleCheckFlag = true;

			if (i >= inputChar.Length) {
			
				i = 0;
			
			}

		}
		Debug.Log (inputone);
		//Debug.Log (inputone);

			/*if(openClosecheckFlag == false){
				//inputText.text += inputone;
				inputFlag = true;

			}*/
		if (openCloseCheckFlag == false) {
			inputText.text += inputone;
		}
		
	}
}