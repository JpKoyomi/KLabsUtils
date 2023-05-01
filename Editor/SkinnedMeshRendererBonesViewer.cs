using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using System.Collections.Generic;

namespace KLabs.Utils
{
	internal sealed class SkinnedMeshRendererBonesViewer : EditorWindow
	{
		public const string TITLE = "Bones Viewer";

		[MenuItem("KLabs/Bones Viewer")]
		public static void EditorWindowShow()
		{
			var window = CreateWindow<SkinnedMeshRendererBonesViewer>();
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		public Vector2 ScrollPosition { get; set; }

		public SkinnedMeshRenderer Source { get; set; }

		private Mesh CacheMesh { get; set; }

		private float[] BoneWeights { get; set; }

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			Source = EditorGUILayout.ObjectField("Source", Source, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			try
			{
				if (Source == null)
				{
					CacheMesh = null;
					return;
				}
				if (CacheMesh != Source.sharedMesh)
				{
					CacheMesh = Source.sharedMesh;
					if (CacheMesh != null)
					{
						var sum = 0.0;
						var weights = CacheMesh.boneWeights;
						var boneWeights = new double[Source.bones.Length];
						for (int i = 0; i < weights.Length; i++)
						{
							var w = weights[i];
							boneWeights[w.boneIndex0] += w.weight0;
							boneWeights[w.boneIndex1] += w.weight1;
							boneWeights[w.boneIndex2] += w.weight2;
							boneWeights[w.boneIndex3] += w.weight3;
							sum += w.weight0 + w.weight1 + w.weight2 + w.weight3;
						}
						BoneWeights = new float[boneWeights.Length];
						for (int i = 0; i < boneWeights.Length; i++)
						{
							var value = boneWeights[i] / sum;
							BoneWeights[i] = boneWeights[i] == 0 ? 0 : value < 0.001 ? 0.001f : (float)System.Math.Round(value, 3);
						}
					}
					else
					{
						BoneWeights = null;
					}
				}

				var lineHeight = 20;

				var startIndex = (int)(ScrollPosition.y / lineHeight);
				GUILayout.Space(startIndex * lineHeight);

				var endIndex = System.Math.Min(startIndex + (int)(position.size.y / lineHeight), Source.bones.Length);

				for (int i = startIndex; i < endIndex; i++)
				{
					var before = Source.bones[i];
					using (new EditorGUILayout.HorizontalScope())
					{
						var prev = GUI.color;
						try
						{
							var isEditorOnly = false;
							var current = before;
							while (current != null)
							{
								isEditorOnly = current.tag == "EditorOnly";
								if (isEditorOnly)
								{
									break;
								}
								current = current.parent;
							}
							if (before == null || isEditorOnly)
							{
								GUI.color = BoneWeights[i] == 0 ? Color.yellow : Color.red;
							}
							var after = EditorGUILayout.ObjectField(before, typeof(Transform), true, GUILayout.Height(lineHeight)) as Transform;
							if (BoneWeights != null)
							{
								EditorGUILayout.FloatField(BoneWeights[i], GUILayout.Width(46), GUILayout.Height(lineHeight));
							}
							if (before != after)
							{
								var bones = (Transform[])Source.bones.Clone();
								bones[i] = after;
								Undo.RecordObject(Source, "SkinnedMeshRenderer bones edit");
								Source.bones = bones;
							}
						}
						finally
						{
							GUI.color = prev;
						}
					}
				}
				GUILayout.Space((Source.bones.Length - endIndex) * lineHeight);
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}
}
