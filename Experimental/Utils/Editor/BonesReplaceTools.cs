using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using KLabs.Utils;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils.Editor
{
	internal sealed class BonesReplaceTools : EditorWindow
	{
		public const string TITLE = "Bones Replace Tools";

		[MenuItem("KLabs/Bones Replace Tools")]
		public static void EditorWindowShow()
		{
			var window = (BonesReplaceTools)GetWindow(typeof(BonesReplaceTools));
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		public Vector2 ScrollPosition { get; set; }

		public HashSet<SkinnedMeshRenderer> Sources { get; set; } = new HashSet<SkinnedMeshRenderer>();
		public Transform Before { get; set; } = null;
		public Transform After { get; set; } = null;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				ButtonResetList();
				ButtonAddSelectObject();
			}
			ObjectFieldBeforeAndAfter();
			ButtonReplace();

			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			try
			{
				ObjectFieldSourceList();
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
		
		private void ButtonResetList()
		{
			if (GUILayout.Button("Reset List"))
			{
				Sources.Clear();
			}
		}

		private void ButtonAddSelectObject()
		{
			if (GUILayout.Button("Add Select Object"))
			{
				var selects = Selection.GetFiltered<SkinnedMeshRenderer>(SelectionMode.Unfiltered);
				for (int i = 0; i < selects.Length; i++)
				{
					Sources.Add(selects[i]);
				}
			}
		}

		private void ButtonReplace()
		{
			if (GUILayout.Button("Replace"))
			{
				foreach (var item in Sources)
				{
					for (int i = 0; i < item.bones.Length; i++)
					{
						if (item.Replace(Before, After, out var bones))
						{
							Undo.RecordObject(item, "SkinnedMeshRenderer bones edit");
							item.bones = bones;
						}
					}
				}
			}
		}

		private void ObjectFieldBeforeAndAfter()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				Before = EditorGUILayout.ObjectField(Before, typeof(Transform), true, GUILayout.ExpandWidth(true)) as Transform;
				EditorGUILayout.LabelField("->", GUILayout.ExpandWidth(false), GUILayout.Width(30));
				After = EditorGUILayout.ObjectField(After, typeof(Transform), true, GUILayout.ExpandWidth(true)) as Transform;
			}
		}

		private void ObjectFieldSourceList()
		{
			foreach (var item in Sources)
			{
				var value = EditorGUILayout.ObjectField(item, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
				if (value != item)
				{
					Sources.Remove(item);
					Sources.Add(value);
					return;
				}
			}
		}
	}
}
