using UnityEditor;
using UnityEngine;

namespace RD
{
	[CustomEditor(typeof(LerpTransform))]
	public class LerpTransformEditor : UnityEditor.Editor
	{
		override public void OnInspectorGUI()
		{
			var LU = target as LerpTransform;
			LU._lerpStyle = (LerpTransform.LerpStyle)EditorGUILayout.EnumPopup("Lerp Style",LU._lerpStyle);
			EditorGUILayout.Space();
			LU.lerpPosition = GUILayout.Toggle(LU.lerpPosition, "Lerp Position");
			if (LU.lerpPosition)
			{
				LU._LocalPosition = EditorGUILayout.Toggle("Local Position", LU._LocalPosition);
				LU._animationCurvePosX = EditorGUILayout.CurveField("X Curve", LU._animationCurvePosX);
				LU._animationCurvePosXisAllAxis = GUILayout.Toggle(LU._animationCurvePosXisAllAxis, "Use X Curve for all axis");
				if (!LU._animationCurvePosXisAllAxis)
				{
					LU._animationCurvePosY = EditorGUILayout.CurveField("Y Curve", LU._animationCurvePosY);
					LU._animationCurvePosZ = EditorGUILayout.CurveField("Z Curve", LU._animationCurvePosZ);
				}
				LU._lerpSpeedPosition = EditorGUILayout.FloatField("Speed", LU._lerpSpeedPosition);
				LU._lerpStartPosition = EditorGUILayout.Vector3Field("Start Position", LU._lerpStartPosition);
				LU._lerpEndPosition = EditorGUILayout.Vector3Field("End Position", LU._lerpEndPosition);
			}
			EditorGUILayout.Space();
			LU.lerpRotation = GUILayout.Toggle(LU.lerpRotation, "Lerp Rotation");
			if (LU.lerpRotation)
			{
				LU._LocalRotation = EditorGUILayout.Toggle("Local Rotation", LU._LocalRotation);
				LU._animationCurveRot = EditorGUILayout.CurveField("Curve", LU._animationCurveRot);
				LU._lerpSpeedRotation = EditorGUILayout.FloatField("Speed", LU._lerpSpeedRotation);
				LU._lerpStartRotation = EditorGUILayout.Vector3Field("Start Rotation", LU._lerpStartRotation);
				LU._lerpEndRotation = EditorGUILayout.Vector3Field("End Rotation", LU._lerpEndRotation);
				LU._lerpRotOnlyZ = EditorGUILayout.Toggle("Lerp Only Z-Axis", LU._lerpRotOnlyZ) ;
			}
			EditorGUILayout.Space();
			LU.lerpScale = GUILayout.Toggle(LU.lerpScale, "Lerp Scale");
			if (LU.lerpScale)
			{
				LU._animationCurveScale = EditorGUILayout.CurveField("Curve", LU._animationCurveScale);
				LU._lerpSpeedScale = EditorGUILayout.FloatField("Speed", LU._lerpSpeedScale);
				LU._lerpStartScale = EditorGUILayout.Vector3Field("Start Scale", LU._lerpStartScale);
				LU._lerpEndScale = EditorGUILayout.Vector3Field("End Scale", LU._lerpEndScale);
			}
			if (GUI.changed) { EditorUtility.SetDirty(LU.gameObject); }
		}
	}
}
