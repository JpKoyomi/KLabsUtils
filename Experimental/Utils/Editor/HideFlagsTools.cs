using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils
{
	internal sealed class HideFlagsToolsGUI : EditorWindow
	{
		public const string TITLE = "HideFlags Tools";

		[MenuItem("KLabs/HideFlags Tools")]
		public static void EditorWindowShow()
		{
			var window = (HideFlagsToolsGUI)GetWindow(typeof(HideFlagsToolsGUI));
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		public Vector2 ScrollPosition { get; set; }

		public GameObject Target { get; set; }

		public float Thickness { get; set; } = 0.2f;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

			Target = EditorGUILayout.ObjectField("Source", Target, typeof(GameObject), true) as GameObject;
			//if (GUILayout.Button("Toggle HideInHierarchy")) HideFlagsTools.HideInHierarchy(Target);
			if (GUILayout.Button("Hide In Hierarchy for EditorOnly children")) HideFlagsTools.HideInHierarchyForEditorOnlyChildren(Target.transform);
			if (GUILayout.Button("Show In Hierarchy for EditorOnly children")) HideFlagsTools.ShowInHierarchyForEditorOnlyChildren(Target.transform);

			EditorGUILayout.EndScrollView();
		}
	}

	public static class HideFlagsTools
	{
		public static void HideInHierarchyForEditorOnlyChildren(Transform target)
		{
			for (int i = 0; i < target.childCount; i++)
			{
				var child = target.GetChild(i);
				if (child.tag == "EditorOnly")
				{
					child.hideFlags |= HideFlags.HideInHierarchy;
				}
				else
				{
					HideInHierarchyForEditorOnlyChildren(child);
				}
			}
		}

		public static void ShowInHierarchyForEditorOnlyChildren(Transform target)
		{
			for (int i = 0; i < target.childCount; i++)
			{
				var child = target.GetChild(i);
				if (child.tag == "EditorOnly")
				{
					child.hideFlags &= ~HideFlags.HideInHierarchy;
				}
				else
				{
					ShowInHierarchyForEditorOnlyChildren(child);
				}
			}
		}

		public static void HideInHierarchy(GameObject target)
		{
			var maskedHideInHierarchy = target.hideFlags & HideFlags.HideInHierarchy;

			if (maskedHideInHierarchy > HideFlags.None)
			{
				target.hideFlags &= ~HideFlags.HideInHierarchy;
			}
			else
			{
				target.hideFlags |= HideFlags.HideInHierarchy;
			}
		}
	}
}
