using System;
using System.Text;
using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils
{
	public class ReverseDomainToolsGUI : EditorWindow
	{
		public const string TITLE = "Reverse Domain Tools GUI";

		private const string HTTPS_SCHEME = "https://";

		[MenuItem("KLabs/Booth URL Reverse Domain Maker")]
		public static void EditorWindowShow()
		{
			var window = (ReverseDomainToolsGUI)GetWindow(typeof(ReverseDomainToolsGUI));
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		//public Vector2 ScrollPosition { get; set; }

		public string Source { get; set; }
		public string Destination { get; set; }

		


		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			//ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

			Source = EditorGUILayout.TextField("Source", Source);

			if (GUILayout.Button("Make"))
			{
				Destination = Source.Substring(HTTPS_SCHEME.Length);
				if (Destination.IndexOf(HTTPS_SCHEME) == 0)
				{
					Destination = Destination.Substring(HTTPS_SCHEME.Length);
				}
				var builder = new StringBuilder();
				var splited = Destination.Split('/');
				if (splited.Length == 3)
				{
					builder.Append(splited[2]);
					builder.Append('.');
					builder.Append(splited[0]);
				}
				Destination = ReverseDomainTools.Reverse(builder.ToString());
			}

			EditorGUILayout.TextField("Destination", Destination);

			//EditorGUILayout.EndScrollView();
		}
	}

	public static class ReverseDomainTools
	{
		public static string Reverse(string text)
		{
			var s = text.Split('.');
			Array.Reverse(s);
			var builder = new StringBuilder(text.Length);
			builder.Append(s[0]);
			for (int i = 1; i < s.Length; i++)
			{
				builder.Append('.');
				builder.Append(s[i]);
			}
			return builder.ToString();
		}
	}
}
