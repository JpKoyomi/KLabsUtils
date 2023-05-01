using System;
using System.Text;
using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils
{
	public class RotationConvertorGUI : EditorWindow
	{
		public const string TITLE = "Rotation Convertor GUI";

		[MenuItem("KLabs/Rotation Convertor")]
		public static void EditorWindowShow()
		{
			var window = (RotationConvertorGUI)GetWindow(typeof(RotationConvertorGUI));
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		//public Vector2 ScrollPosition { get; set; }

		public Vector3 Euler { get; set; }
		public Quaternion Quaternion { get; set; }

		public float RotateAxis { get; set; }

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			//ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

			Euler = EditorGUILayout.Vector3Field("Euler", Euler);
			var vec4 = EditorGUILayout.Vector4Field("Quaternion", new Vector4(Quaternion.x, Quaternion.y, Quaternion.z, Quaternion.w));
			Quaternion = new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);

			if (GUILayout.Button("Euler To Quaternion"))
			{
				Quaternion = Quaternion.Euler(Euler);
			}
			if (GUILayout.Button("Quaternion To Euler"))
			{
				Euler = Quaternion.eulerAngles;
			}
			if (GUILayout.Button("Normalize Quaternion"))
			{
				Quaternion = Quaternion.normalized;
			}
			EditorGUILayout.LabelField("Rotate AngleAxis");
			RotateAxis = EditorGUILayout.FloatField("Value", RotateAxis);
			if (GUILayout.Button("Rotate X Axis"))
			{
				Quaternion = Quaternion.AngleAxis(RotateAxis, Vector3.left) * Quaternion;
			}
			if (GUILayout.Button("Rotate Y Axis"))
			{
				Quaternion = Quaternion.AngleAxis(RotateAxis, Vector3.up) * Quaternion;
			}
			if (GUILayout.Button("Rotate Z Axis"))
			{
				Quaternion = Quaternion.AngleAxis(RotateAxis, Vector3.forward) * Quaternion;
			}

			//EditorGUILayout.EndScrollView();
		}
	}
}
