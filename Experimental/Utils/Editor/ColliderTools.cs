using UnityEditor;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils
{
    internal sealed class ColliderToolsGUI : EditorWindow
    {
        public const string TITLE = "Collider Tools";

        [MenuItem("KLabs/Collider Tools")]
        public static void EditorWindowShow()
        {
            var window = (ColliderToolsGUI)GetWindow(typeof(ColliderToolsGUI));
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        public Vector2 ScrollPosition { get; set; }

        public BoxCollider Source { get; set; }

        public float Thickness { get; set; } = 0.2f;

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnGUI()
        {
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

            Source = EditorGUILayout.ObjectField("Source", Source, typeof(BoxCollider), true) as BoxCollider;
            Thickness = EditorGUILayout.FloatField("Thickness", Thickness);
            if (GUILayout.Button("Generate Inbound colliders")) ColliderTools.GenerateInboundCollider(Source);

            EditorGUILayout.EndScrollView();
        }
    }

    public static class ColliderTools
    {
        public static GameObject GenerateInboundCollider(BoxCollider source, float thickness = 0.2f)
        {
            if (source == null) throw new System.ArgumentNullException();
            var go = new GameObject(source.name + "(InboundCollider)");
            go.transform.SetParent(source.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var t = thickness;
            {
                var x = source.size.x;
                var y = source.size.y;
                var z = source.size.z;

                var px = go.AddComponent<BoxCollider>();
                var nx = go.AddComponent<BoxCollider>();
                var py = go.AddComponent<BoxCollider>();
                var ny = go.AddComponent<BoxCollider>();
                var pz = go.AddComponent<BoxCollider>();
                var nz = go.AddComponent<BoxCollider>();

                px.center = new Vector3((x + t) / 2, 0, 0) + source.center;
                nx.center = new Vector3(-(x + t) / 2, 0, 0) + source.center;
                py.center = new Vector3(0, (y + t) / 2, 0) + source.center;
                ny.center = new Vector3(0, -(y + t) / 2, 0) + source.center;
                pz.center = new Vector3(0, 0, (z + t) / 2) + source.center;
                nz.center = new Vector3(0, 0, -(z + t) / 2) + source.center;

                px.size = new Vector3(t, y + t, z + t);
                nx.size = px.size;
                py.size = new Vector3(x + t, t, z + t);
                ny.size = py.size;
                pz.size = new Vector3(x + t, y + t, t);
                nz.size = pz.size;
            }
            return go;
        }
    }
}
