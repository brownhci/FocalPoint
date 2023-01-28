using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//new
using System;
using System.IO;
using OpenCVForUnity;

/* detect basic 3 gestures */

public class GestureControl : MonoBehaviour {
	//Left hand finger declare
	private GameObject palm;

	//poseDetectorMLtrainning
	LogisticRegression logis_reg_model;
	static SVM svm_model;
	int gesture_num = 5;
	int mat_n = 30;

	//poseDetector buffer
	int[] gesture_buff;
	int gesture_buff_len = 5;
	int gesture_buff_idx = 0; 

	GameObject dataMgr;

    //Flag if it's left hand
    private bool bIsLeftHand;

	//Gesture dictionary
	Dictionary<int, string> gesture_dict = new Dictionary<int, string>();

	// Use this for initialization
	void Start () {
		dataMgr = GameObject.Find ("gDataManager");
		gesture_buff_len = dataMgr.GetComponent<DataManager> ().gestBuffer;

        palm = this.transform.GetChild (5).gameObject;
		gesture_buff = new int[gesture_buff_len];

		//Gesture dicitonary establishes
		gesture_dict.Add(0, "palm");
		gesture_dict.Add(1, "pinch");
		gesture_dict.Add(2, "paint");
		gesture_dict.Add(3, "fist");
		gesture_dict.Add(4, "undefined");

       HandManager hm = GetComponent<HandManager>();
        if (hm != null)
            bIsLeftHand = hm.bIsLeftHand;
        else
            bIsLeftHand = true;
    }
	
	// Update is called once per frame
	void Update () {
        //Update gesture buffer array
		if (svm_model != null) {

			gesture_buff [gesture_buff_idx++] = gestureDetectorMLpredict ();
           // Debug.Log(gesture_buff[gestureDetectorMLpredict()]);
            gesture_buff_idx = (gesture_buff_idx) % gesture_buff_len;
		}
	}

    void OnEnable() {
        // When OnEnable, if SVM isn't set up, set up it
        if (svm_model == null) {
            readSVM();
        }
    }
    
	/* 	gestureDetectorMLpredict
	*	Input: None
	*	Output: None
	*	Summary: 1. Collect current hand data 2. Generate an instant prediction for current gesture
	*/
	private int gestureDetectorMLpredict(){

        /* initial satte */
        if (svm_model == null)
            return 0;
		//initialize/declare necessary normals and vector array
		Vector3[] vec_bone2 = new Vector3[5];
		Vector3[] vec_bone1 = new Vector3[5];
		float[] cur_data_array = new float[30];
		Mat cur_data_mat = Mat.zeros(1,mat_n,CvType.CV_32F);


		Vector3 palm_plane_norm = palm.transform.forward;
		Vector3 palm_plane_up = palm.transform.up;
        Vector3 palm_plane_right = palm.transform.right;

        /*if (!bIsLeftHand) {
            palm_plane_right = -palm_plane_right;
        }*/

		//collect current hand data
		for (int i = 0; i < 5; i++) {
			Vector3 vec_palm_bone2 = this.transform.GetChild (i).GetChild (2).position - palm.transform.position;
			Vector3 vec_palm_bone1 = this.transform.GetChild (i).GetChild (1).position - palm.transform.position;
            /*if (!bIsLeftHand) {
                vec_palm_bone1.x = -vec_palm_bone1.x;
                vec_palm_bone2.x = -vec_palm_bone2.x;
            }*/
            vec_bone2 [i].x = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_right).magnitude;
			vec_bone2 [i].y = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_norm).magnitude;
			vec_bone2 [i].z = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_up).magnitude;
            //if (i == 0) {
            //    Debug.Log(bIsLeftHand + " vec_bone2:" + vec_bone2[i].x + "," + vec_bone2[i].y + "," + vec_bone2[i].z);
            //}
			vec_bone1 [i].x = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_right).magnitude;
			vec_bone1 [i].y = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_norm).magnitude;
			vec_bone1 [i].z = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_up).magnitude;
			cur_data_array [i * 6] = vec_bone2 [i].x;
			cur_data_array [i * 6 + 1] = vec_bone2 [i].y;
			cur_data_array [i * 6 + 2] = vec_bone2 [i].z;
			cur_data_array [i * 6 + 3] = vec_bone1 [i].x;
			cur_data_array [i * 6 + 4] = vec_bone1 [i].y;
			cur_data_array [i * 6 + 5] = vec_bone1 [i].z;
		}
		cur_data_mat.put (0, 0, cur_data_array);
        
		//predict
		Mat result = Mat.ones(1,1,CvType.CV_32S);


		svm_model.predict (cur_data_mat, result, 0);

		//Debug usage
		//string current_gesture = gesture_dict[((int)result.get (0, 0) [0])];

		return ((int)result.get (0, 0) [0]);
	}

	/* 	bufferedGesture
	*	Input: None
	*	Output: None
	*	Summary: Output mode gesture in last detector_buff_len frames to reduce noise
	*/
	public string bufferedGesture(){
		int[] gesture_hist = new int[gesture_dict.Count];
		for (int i = 0; i < gesture_buff_len; i++) {
			gesture_hist [gesture_buff [i]] += 1;
		}

		int modeGesture = 0;
		for (int i = 0; i < gesture_hist.Length; i++){
			if (gesture_hist[i] >= gesture_hist[modeGesture])
				modeGesture = i;
		}
		return gesture_dict [modeGesture];
	}


	/* 	handDataGenerator
	*	Input: None
	*	Output: None
	*	Summary: Output current gesture data into a .txt file
	*/
	private void handDataGenerator(){
		Vector3[] vec_bone2 = new Vector3[5];
		Vector3 palm_plane_norm, palm_plane_up, palm_plane_right;
		palm_plane_norm = palm.transform.forward;
		palm_plane_up = palm.transform.up;
		palm_plane_right = palm.transform.right;

		string temp = "";
		for (int i = 0; i < 5; i++) {
			Vector3 vec_palm_bone2 = this.transform.GetChild (i).GetChild (2).position - palm.transform.position;
			vec_bone2 [i].x = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_right).magnitude;
			vec_bone2 [i].y = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_norm).magnitude;
			vec_bone2 [i].z = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_up).magnitude;
			temp += vec_bone2 [i].x.ToString ("F10") + "," + vec_bone2 [i].y.ToString ("F10") + "," + vec_bone2 [i].z.ToString ("F10") ;
			if (i < 4)
				temp += ",";
			else
				temp += "\n";
		}
		//System.IO.File.AppendAllText(System.IO.Path.Combine(Application.persistentDataPath, "handDataG_new.txt"), temp);
	}

    private void readSVM() {
        StartCoroutine(readSVMCo());
    }

    private IEnumerator readSVMCo() {
        Debug.Log("Start SVM coroutine");
        // for Android, we need to use UnityWeb to get access to streamingAssetsPath.
        if (svm_model == null) {

            // For Android platform, streamingAssetsPath is inaccessible. So we use web request to get it.
#if UNITY_ANDROID
            using (WWW svm_reader = new WWW(System.IO.Path.Combine(Application.streamingAssetsPath, "svm.xml"))) {
                yield return svm_reader;
                System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "svm.xml"), svm_reader.text);
                svm_model = OpenCVForUnity.SVM.load(System.IO.Path.Combine(Application.persistentDataPath, "svm.xml"));
            }
#else
            svm_model = OpenCVForUnity.SVM.load(System.IO.Path.Combine(Application.streamingAssetsPath, "svm.xml"));
            yield return svm_model;
#endif
            Debug.Log(svm_model.empty());
        }
    }
}
