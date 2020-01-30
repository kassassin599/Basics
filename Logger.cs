using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour {

	public bool showLog;
	Vector2 scrollPos;

	static Logger instance;
	public static Logger Instance {
		get {
			if (instance == null) {
				var item = new GameObject();
				item.AddComponent<Logger>();
				item.gameObject.name = "Logger";
				instance = item.GetComponent<Logger>();
			}
			return instance;
		}
	}

	private void Awake() {
		if (instance != null) {
			if (instance != this) {
				Destroy(gameObject);
			}
		} else {
			instance = this;
		}
	}

	private void Start() {
		DontDestroyOnLoad(gameObject);
	}

	private void OnGUI() {
		if (!showLog) {
			return;
		}

		GUILayout.BeginVertical();
		if (GUILayout.Button("Clear Log", GUILayout.Height(Screen.height * 0.03f),GUILayout.MaxWidth(Screen.width*0.25f))) {
			Quick.ClearLog();
		}
		GUILayout.EndVertical();
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		Quick.ShowLog();
		GUILayout.EndScrollView();
	}
}