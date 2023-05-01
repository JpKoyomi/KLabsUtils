using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils
{
	internal sealed class Toolsbar : EditorWindow
	{
		public const string TITLE = "Toolsbar";

		[MenuItem("KLabs/Toolsbar")]
		public static void EditorWindowShow()
		{
			var window = GetWindow<Toolsbar>();
			window.titleContent = new GUIContent(TITLE);
			window.minSize = new Vector2(50, 50);
			window.Show();
		}

		Texture2D vrcSdkTex;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			if (vrcSdkTex == null)
			{
				vrcSdkTex = Resources.Load<Texture2D>("vrcSdkHeader");
			}
			if (vrcSdkTex != null)
			{
				if (GUILayout.Button(vrcSdkTex, GUILayout.Width(46), GUILayout.Height(46)))
				{
					var asm = System.Reflection.Assembly.Load("VRC.SDKBase.Editor");
					var type = asm.GetType("VRCSdkControlPanel");
					if (type != null && type.IsSubclassOf(typeof(UnityEditor.EditorWindow)))
					{
						var window = GetWindow(type);
						window.Show();
					}

					//var window = Resources.FindObjectsOfTypeAll<EditorWindow>();
					//Debug.Log(window);
				}
			}
		}
	}
}
