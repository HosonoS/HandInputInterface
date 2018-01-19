/**********************************************************************************************
 *   UnlimitedHand for Unity Plugin ver 0039
 *   This code is used with an Arduino Code 
 *   "Serial_4Unity_4ProcessingQuaternion" in UH4Arduino0039.zip.
 *   
 *   [Notification 1]
 *   Please setup the Compatible Level to ".NET 2.0" on your Unity project to use Serial Connection.
 *   
 *   [Notification 2]
 *   If you use windos PC, Please change Port Name at Line 201: return "COM3"; : in this code.
 * 
 *   [Notification 3]
 *   In this version, code for Quaternion is added.
 *   If you want to use Euler angles, Please change the code in "void Update()".
 * 
 *   [Notification 4]
 *   A sample unity project tutorial with this plugin is published.
 *   Please check http://dev.unlimitedhand.com
 * ******************************************************************************************/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.IO.Ports;
using System.Threading;

public class UH : MonoBehaviour {
	//Serial Connection
	public string PortName = "";		// Port Name
	private SerialPort _serialPort;
	public int Baud = 115200;			// Baud Rate
	public int RebootDelay = 10;		// Amount of time to wait after opening connection for arduino to reboot before sending firmata commands
	
	public bool AutoConnect = true;		// Connect automatically
	public bool Connected { get; private set; }// true when the device is connected
	
	//UH instance
	private static UH instance = null;
	public static UH global { get { return instance; } }
	
	public enum READ_TYPE {HIGH_SPEED, LOW_SPEED};
	
	//Objects: Accel and Gyro
	public float[] UHGyro = new float[3];
	public float[] UHAccel = new float[3];
	public float[] UHGyroAccelData = new float[7];
	
	//Objects: Quaternion
	public float[] UHQuaternion = new float[4];
	
	//Object: Foream angle
	public int[] UHAngle = new int[3];
	public bool updateAngleFlg;
	
	//Object: Photo-reflectors' value to detect hand movements
	public int[] UHPR = new int[8];
	public bool updatePhotoSensorsFlg;
	
	/////////////////////////////////////////////////////
	/// AWAKE
	/////////////////////////////////////////////////////
	void Awake () {
		Debug.Log("UnlimitedHand awake");
		if (instance == null) instance = this;
		DontDestroyOnLoad(this);
		
		//Serial Connection
		if (AutoConnect){
			Debug.Log("AutoConnecting...");
			if (PortName == null || PortName.Length == 0 && UH.guessPortName().Length > 0){
				PortName = UH.guessPortName();
			}
		Connect();
		}

		//Initializes objects to get the forearm angle data
		for (int i=0; i<7; i++) {UHGyroAccelData [i] = 0.0f;}
		updateAngleFlg = true;

		//Initializes objects to get the hand movements data
		for (int i=0; i<8; i++) { UHPR [i] = 0;}
		updatePhotoSensorsFlg = false;
	}
	
	
	/////////////////////////////////////////////////////
	/// UPDATE
	/////////////////////////////////////////////////////
	void Update(){


	}

	void FixedUpdate(){

		if(_serialPort  !=  null  &&  _serialPort.IsOpen) {
			//updateAnglePR (); // code for Euler angles
			updateQuaternion (); //code for quaternion. you can use an object "UHQuaternion[4]".
			updatePhotoSensors();
			updateAngle ();

		}

	}
	
	public void updateQuaternion(){
		try{
			_serialPort.WriteLine ("q");
			//Debug.Log("in Func readQuaternion:");
			string QuaData = _serialPort.ReadLine ();
			//Debug.Log(_serialPort.ReadLine());

			//Debug.Log(QuaData + " " + "クォーターニオン");
			if(QuaData!=null && QuaData.Contains("+")) {
				if(QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None).Length  != 4){
					return;
				}
				UHQuaternion[0] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [0]);
				UHQuaternion[1] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [1]);
				UHQuaternion[2] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [2]);
				UHQuaternion[3] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [3]);
			}
		}catch(Exception){
			//Debug.Log("updateQuaternion Error!!");
		}
		
	}

	public void readQuaternion(){
		try{
			//Debug.Log("in Func readQuaternion:");
			string QuaData = _serialPort.ReadLine ();
			
			//Debug.Log(QuaData);
			if(QuaData!=null && QuaData.Contains("+")) {
				if(QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None).Length  != 4){
					return;
				}
				UHQuaternion[0] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [1]);
				UHQuaternion[1] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [0]);
				UHQuaternion[2] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [2]);
				UHQuaternion[3] = float.Parse (QuaData.Split (new string[]{"+"}, System.StringSplitOptions.None) [3]);
			}
		}catch(Exception){
			
		}
	} 
		
	public void resetQuaternion(){
		try{
			_serialPort.WriteLine ("r"); 
		}catch(Exception){				

		}
	}

	/////////////////////////////////////////////////////
	/// SERIAL CONNECTION  : Start the Connection
	/////////////////////////////////////////////////////
	public void Connect(){
		Debug.Log ("Connecting to UnlimitedHand at " + PortName + "...");
		
		_serialPort = new SerialPort(PortName, Baud);
		
		_serialPort.DtrEnable = true; // win32 hack to try to get DataReceived event to fire
		_serialPort.RtsEnable = true; 
		_serialPort.PortName = PortName;
		_serialPort.BaudRate = Baud;
		
		_serialPort.DataBits = 8;
		_serialPort.Parity = Parity.None;
		_serialPort.StopBits = StopBits.One;
		_serialPort.ReadTimeout = 1; // since on windows we *cannot* have a separate read thread
		_serialPort.WriteTimeout = 1000;
		
		_serialPort.Open ();
		
		if (_serialPort.IsOpen){ Thread.Sleep(RebootDelay);}
		
	}


	// return port name
	public static string guessPortName(){
		switch (Application.platform){
		case RuntimePlatform.OSXPlayer:
			return guessPortNameUnix ();
		case RuntimePlatform.OSXEditor:
			return guessPortNameUnix ();
		//case RuntimePlatform.OSXDashboardPlayer:
		//	return guessPortNameUnix ();
		case RuntimePlatform.LinuxPlayer:
			return guessPortNameUnix();
			
		default:
			return guessPortNameWindows();
		}
	}
	
	public static string guessPortNameWindows(){
		var devices = System.IO.Ports.SerialPort.GetPortNames();
		
		if (devices.Length == 0){
			return "COM3"; //please write the port name in your windows enviroment
		}else{
			return devices[0];
		}
	}
	
	public static string guessPortNameUnix(){
		var devices = System.IO.Ports.SerialPort.GetPortNames();
		
		if (devices.Length ==0) //try manual enumeration
		{
			devices = System.IO.Directory.GetFiles("/dev/");
			Debug.Log("Read the devices");
		}
		string dev = "";
		foreach (var d in devices){
			Debug.Log (d);
			
			if(d.StartsWith ("/dev/cu.usb") || d.StartsWith ("/dev/tty.usb")){
				dev = d;
				Debug.Log ("Guessing that UnlimitedHand is device " + dev);
				return dev;
			}
		}
		
		foreach (var d in devices){
			if (d.StartsWith ("/dev/cu.RNBT") || d.StartsWith ("/dev/tty.RNBT")) {
				dev = d;
				Debug.Log ("Guessing that UnlimitedHand is device " + dev);
				return dev;
			}
		}
		return dev;
	}
	
	
	/////////////////////////////////////////////////////
	/// SERIAL CONNECTION: Close the Connection
	/////////////////////////////////////////////////////
	void OnDestroy(){
		Disconnect();
	}
	
	public void Disconnect(){
		Connected = false;
		Close ();
	}
	
	protected void Close(){
		if (_serialPort != null) {
			_serialPort.Close ();
		}
	}

	//////////////////////////////////////////////////////////
	/// READ Hand Movements(Photo-reflector sensors values) 
	///                               via Serial Connection
	//////////////////////////////////////////////////////////
	public int[] readPhotoSensors(){
		return UHPR;
	}

	public void updatePhotoSensors(){
		try{
			_serialPort.WriteLine ("c"); 
			string data = _serialPort.ReadLine (); 
			//Debug.Log(data);

			if(data!=null && data.Contains("_")){
				
				if(data.Split (new string[]{"_"}, System.StringSplitOptions.None).Length != 8){
					//Debug.Log(data);
					//Debug.Log(UHPR[0]);
					return;
				}

				for(int i=0;i<8;i++){
					int prVal = int.Parse (data.Split (new string[]{"_"}, System.StringSplitOptions.None) [i]);
							if(0<= prVal && prVal<1024) UHPR [i] = prVal;
					//Debug.Log(UHPR[0]);
				}
			}

			//Debug.Log(UHPR[0]);

		}catch(TimeoutException){
			//UHPR[0] = 0;
			//UHPR[1] = 0;
			//UHPR[2] = 0;
			//UHPR[3] = 0;
			//UHPR[4] = 0;
			//UHPR[5] = 0;
			//UHPR[6] = 0;
			//UHPR[7] = 0;
		}

	}

	//////////////////////////////////////////////////////////
	/// READ Forearm Angles via Serial Connection
	//////////////////////////////////////////////////////////
	public void updateAngle(){
		try{
			//byte a = 0xA;
			_serialPort.WriteLine ("A"); 
			//Debug.Log ("Send a message : A");
			string data2 = _serialPort.ReadLine ();
			//Debug.Log(data2);
			if(data2!=null && data2.Contains("+")) {
				if(data2.Split (new string[]{"+"}, System.StringSplitOptions.None).Length  != 7){
					return;
				}
				for(int i=0;i<3;i++){
					int ang = int.Parse (data2.Split (new string[]{"+"}, System.StringSplitOptions.None) [i]);
					if(-180<= ang && ang<=180) UHAngle [i] = ang;
				}
			}
		}catch(Exception){
			//UHAngle[0] = 0;
			//UHAngle[1] = 0;
			//UHAngle[2] = 0;
		}
	} 

	//////////////////////////////////////////////////////////
	/// READ Forearm Angles and also Photo-Reflector's Values via Serial Connection
	//////////////////////////////////////////////////////////
	public void updateAnglePR(){
		try{
			_serialPort.WriteLine ("C"); 
			string data3 = _serialPort.ReadLine (); 
			if(data3!=null && data3.Contains("+")) {
				if(data3.Split (new string[]{"+"}, System.StringSplitOptions.None).Length  != 11){
					return;
				}
				for(int i=0;i<3;i++){
					int ang = int.Parse (data3.Split (new string[]{"+"}, System.StringSplitOptions.None) [i]);
					UHAngle [i] = ang;
				}
				for(int i=3;i<11;i++){
					UHPR[i-3] = int.Parse (data3.Split (new string[]{"+"}, System.StringSplitOptions.None) [i]);
					
				}
			}
		}catch(Exception){
			//UHAngle[0] = 0;
			//UHAngle[1] = 0;
			//UHAngle[2] = 0;
		}
	} 

	//////////////////////////////////////////////////////////
	/// READ Gyro Accel data  via Serial Connection
	//////////////////////////////////////////////////////////
	public float[] readUH3DAccel(){
		UHAccel [0] = UHGyroAccelData [0];
		UHAccel [1] = UHGyroAccelData [1];
		UHAccel [2] = UHGyroAccelData [2]; 
		return UHAccel;
	}

	public float[] readGyro (){
		UHGyro [0] = UHGyroAccelData [4];
		UHGyro [1] = UHGyroAccelData [5];
		UHGyro [2] = UHGyroAccelData [6];
		return UHGyro;
	}

	public  void updateUH3DGyroAccel(){
		try{
			_serialPort.WriteLine ("a");
			//Debug.Log ("Send a message a . ");
			string data = _serialPort.ReadLine ();
			//Debug.Log (data );
			if(data!=null && data.Contains("+") ) {
				if(data.Split (new string[]{"+"}, System.StringSplitOptions.None).Length != 7){
					return;
				}
				UHGyroAccelData [0] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None) [0]);
				UHGyroAccelData [1] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None) [1]);
				UHGyroAccelData [2] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None) [2]);
				
				UHGyroAccelData [3]    = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None) [3]);
				
				UHGyroAccelData [4] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None)[4]);
				UHGyroAccelData [5] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None)[5]);
				UHGyroAccelData [6] = float.Parse (data.Split (new string[]{"+"}, System.StringSplitOptions.None)[6]);
			}
		}catch(TimeoutException){
			//UHGyroAccelData[0] = 0.0f;
			//UHGyroAccelData[1] = 0.0f;
			//UHGyroAccelData[2] = 0.0f;
			//UHGyroAccelData[3] = 0.0f;
			//UHGyroAccelData[4] = 0.0f;
			//UHGyroAccelData[5] = 0.0f;
			//UHGyroAccelData[6] = 0.0f;
		}
	}
	

	/////////////////////////////////////////////////////
	/// EMS(Electri Muscle Stimulation) functions to move the user's hand
	/////////////////////////////////////////////////////
    
	public void stimulate(int padNum){
        try{
			bool pastUpdateAngleFlg = updateAngleFlg;
			bool pastUpdatePhotoSensorsFlg = updatePhotoSensorsFlg;
			updateAngleFlg=false; 
			updatePhotoSensorsFlg=false;

			_serialPort.WriteLine (padNum.ToString());
			Thread.Sleep(1);

			updateAngleFlg = pastUpdateAngleFlg;
			updatePhotoSensorsFlg = pastUpdatePhotoSensorsFlg;
		}catch(TimeoutException){
			
		}
	}
	
	public void setLevelUp(){
		try{
			_serialPort.WriteLine ("h"); 
		}catch(TimeoutException){
			
		}
	}

	public void setLevelDown(){
		try{
			_serialPort.WriteLine ("l"); 
		}catch(TimeoutException){
			
		}
	}


	/////////////////////////////////////////////////////
	/// Vibration function
	/////////////////////////////////////////////////////
	public void vibrate(){
		try{
			_serialPort.WriteLine ("b");
		}catch(TimeoutException){
			
		}
	}
	public float readUHtemp(){
		float cels = 0.0f;
		string data = _serialPort.ReadLine ();
		cels = float.Parse (data.Split (new string[]{" "}, System.StringSplitOptions.None)[3]);
		return cels;
	}
}