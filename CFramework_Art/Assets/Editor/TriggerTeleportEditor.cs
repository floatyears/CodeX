using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriggerTeleport))]
public class TriggerTeleportEditor : Editor {

	SerializedProperty trigger;

	SerializedProperty destPos;

	static int s_SelectedID;

	static int s_SelectedPoint = -1;
	static Color s_HandleColor = new Color(255f, 167f, 39f, 210f) / 255;
	static Color s_HandleColorDisabled = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;

	void OnEnable(){
		trigger = serializedObject.FindProperty("tigger");
		destPos = serializedObject.FindProperty("destPos");
	}

	[DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
	static void RenderBoxGizmo(TriggerTeleport teleport, GizmoType gizmoType){
		if(!EditorApplication.isPlaying){

		}

		var color = s_HandleColor;
		if(!teleport.enabled){
			color = s_HandleColorDisabled;
		}
		if(teleport.trigger == null){
			teleport.trigger = teleport.GetComponent<BoxCollider>();
		}

		var oldColor = Gizmos.color;
		var oldMatrix = Gizmos.matrix;

		Gizmos.matrix = UnScaledLocalToWorldMatrix(teleport.transform);
		Gizmos.color = color;
		DrawTeleport(teleport);
	}

	static Matrix4x4 UnScaledLocalToWorldMatrix(Transform t){
		return Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
	}

	static void DrawTeleport(TriggerTeleport teleport){
		Handles.DrawLine(teleport.transform.localToWorldMatrix.MultiplyPoint(teleport.trigger.center) , teleport.transform.localToWorldMatrix.MultiplyPoint(teleport.destPos));
	}

	public void OnSceneGUI(){
		var teleport = (TriggerTeleport)target;
		if(teleport.trigger == null){
			teleport.trigger = teleport.GetComponent<BoxCollider>();
		}
		if(!teleport.enabled){
			return;
		}

		var oldColor = Handles.color;

		var mat = UnScaledLocalToWorldMatrix(teleport.transform);
		var startPt = mat.MultiplyPoint(teleport.trigger.center);
		var startSize = HandleUtility.GetHandleSize(startPt);

		var destPt = mat.MultiplyPoint(teleport.destPos);
		var destSize = HandleUtility.GetHandleSize(destPt);

		var zup = Quaternion.FromToRotation(Vector3.forward, Vector3.up);

		Vector3 pos;
		if(teleport.GetInstanceID() == s_SelectedID && s_SelectedPoint == 0){
			EditorGUI.BeginChangeCheck();
			Handles.CubeHandleCap(0, destPt, zup, 0.1f * destSize, Event.current.type);
			pos = Handles.PositionHandle(destPt, teleport.transform.rotation);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(teleport,"move dest pos");
				teleport.destPos = mat.inverse.MultiplyPoint(pos);
			}
		}else{
			if(Handles.Button(destPt, zup, 0.1f * destSize, 0.1f * destSize, Handles.CubeHandleCap)){
				s_SelectedPoint = 0;
				s_SelectedID = teleport.GetInstanceID();
			}
		}

		// if(teleport.GetInstanceID() == s_SelectedID && s_SelectedPoint == 1){
		// 	EditorGUI.BeginChangeCheck();
		// 	Handles.CubeHandleCap(0, startPt, zup, 0.1f * startSize, Event.current.type);
		// 	pos = Handles.PositionHandle(startPt, teleport.transform.rotation);
		// 	if(EditorGUI.EndChangeCheck()){
		// 		Undo.RecordObject(teleport,"move start pos");
		// 		teleport.trigger.center = mat.inverse.MultiplyPoint(pos);
		// 	}
		// }else{
		// 	if(Handles.Button(startPt, zup, 0.1f * startSize, 0.1f * startSize, Handles.CubeHandleCap)){
		// 		s_SelectedPoint = 1;
		// 		s_SelectedID = teleport.GetInstanceID();
		// 	}
		// }

		Handles.color = oldColor;
	}



}
