using System.Linq;
using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using System.Collections.Generic;

namespace KLabs.Utils.Editor
{
	internal sealed class SkinnedMeshDeleter : EditorWindow
	{
		public const string TITLE = "Skinned Mesh Deleter";

		[MenuItem("KLabs/Skinned Mesh Deleter")]
		public static void EditorWindowShow()
		{
			var window = GetWindow<SkinnedMeshDeleter>();
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		public Vector2 ScrollPosition { get; set; }

		public SkinnedMeshRenderer Source { get; set; }

		public BoxCollider Area { get; set; }

		public bool Inverse { get; set; }

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			Source = EditorGUILayout.ObjectField("Source", Source, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
			Area = EditorGUILayout.ObjectField("Area", Area, typeof(BoxCollider), true) as BoxCollider;
			Inverse = EditorGUILayout.Toggle("Inverse", Inverse);
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			try
			{
				using (new EditorGUI.DisabledGroupScope(Source == null || Area == null))
				{
					if (GUILayout.Button("Delete"))
					{
						var path = EditorUtility.SaveFilePanelInProject("Save Mesh", Source.sharedMesh.name, "mesh", "Select save mesh path.");
						if (string.IsNullOrEmpty(path))
						{
							return;
						}
						EditorUtility.DisplayProgressBar("Skinned Mesh Deleter", "Mesh deleting.", 0);
						try
						{
							var mesh = Source.DeleteMesh(Area, Inverse);
							EditorUtility.DisplayProgressBar("Skinned Mesh Deleter", "Mesh deleting.", 90);
							AssetDatabase.CreateAsset(mesh, path);
							AssetDatabase.SaveAssets();
							Undo.RecordObject(Source, "SkinnedMeshRenderer sharedMesh edit");
							Source.sharedMesh = mesh;
						}
						finally
						{
							EditorUtility.ClearProgressBar();
						}
					}
				}
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}
}