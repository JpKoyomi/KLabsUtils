using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils.Experimental.Editor
{
	internal sealed class AnimatorToolsGUI : EditorWindow
	{
		public const string TITLE = "Animator Tools";

		[MenuItem("KLabs/Animator Tools/Commons")]
		public static void EditorWindowShow()
		{
			var window = CreateWindow<AnimatorToolsGUI>();
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		private void OnEnable()
		{
			Selection.selectionChanged += Repaint;
		}

		private void OnDisable()
		{
			Selection.selectionChanged -= Repaint;
		}


		public Vector2 ScrollPosition { get; set; }

		public AnimatorController Source { get; set; }

		public bool HasExitTime { get; set; }

		public float TransitionDuration { get; set; }

		private AnimatorController srcAnimatorController;
		private AnimatorController dstAnimatorController;
		private string[] srcLayerSelection;
		private string[] dstLayerSelection;

		public AnimatorController SrcAnimatorController { get => srcAnimatorController; set => ChangeValueWithAnimatorController(value, ref srcAnimatorController, ref srcLayerSelection); }

		public AnimatorController DstAnimatorController { get => dstAnimatorController; set => ChangeValueWithAnimatorController(value, ref dstAnimatorController, ref dstLayerSelection); }

		private static void ChangeValueWithAnimatorController(AnimatorController value, ref AnimatorController dst, ref string[] selection)
		{
			if (value != null)
			{
				if (dst != value)
				{
					selection = value.layers.Select((e) => e.name).ToArray();
				}
			}
			else
			{
				selection = EMPTY_SELECTION;
			}
			dst = value;
		}

		public int SelectedSrcLayer { get; set; }

		public int SelectedDstLayer { get; set; }

		private string[] SrcLayerSelection
		{
			get => srcLayerSelection;
			set
			{
				SelectedSrcLayer = srcLayerSelection != value ? 0 : SelectedSrcLayer;
				srcLayerSelection = value;
			}
		}

		private string[] DstLayerSelection
		{
			get => dstLayerSelection;
			set
			{
				SelectedDstLayer = dstLayerSelection != value ? 0 : SelectedDstLayer;
				dstLayerSelection = value;
			}
		}

		public Transform BoneSourceA { get; set; }

		public Transform BoneSourceB { get; set; }

		public Transform UpTarget { get; set; }

		public List<AnimatorCondition> AnimatorConditionSettings { get; } = new List<AnimatorCondition>();

		private static readonly string[] EMPTY_SELECTION = new string[] { string.Empty };

		private static readonly GUILayoutOption W50 = GUILayout.Width(50);
		private static readonly GUILayoutOption W90 = GUILayout.Width(90);

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			//Source = EditorGUILayout.ObjectField("Source", Source, typeof(AnimatorController), false) as AnimatorController;
			//if (GUILayout.Button("Change Hide Flags")) AnimatorTools.ChangeHideFlags(Source);
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			try
			{
				EditorGUILayout.LabelField("AnimatorStateTransition");
				var transitions = Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.Unfiltered);
				using (new EditorGUI.DisabledGroupScope(transitions.Length == 0))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						HasExitTime = EditorGUILayout.Toggle("Has Exit Time", HasExitTime);
						if (GUILayout.Button("Set", W50)) transitions.SetHasExitTime(HasExitTime);
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						TransitionDuration = Mathf.Clamp(EditorGUILayout.FloatField("Transition Duration", TransitionDuration), 0, float.MaxValue);
						if (GUILayout.Button("Set", W50)) transitions.SetTransitionDuration(TransitionDuration);
					}
					using (new EditorGUI.DisabledGroupScope(transitions.Length != 1))
					{
						if (GUILayout.Button("Copy Condition Settings"))
						{
							AnimatorConditionSettings.Clear();
							AnimatorConditionSettings.AddRange(transitions[0].conditions);
						}
					}
					for (int i = 0; i < transitions.Length; i++)
					{
						EditorGUILayout.ObjectField($"{i}", transitions[i], typeof(AnimatorStateTransition), false);
					}
				}

				EditorGUILayout.LabelField("AnimatorState");
				var animatorStates = Selection.GetFiltered<AnimatorState>(SelectionMode.Unfiltered);
				using (new EditorGUI.DisabledGroupScope(animatorStates.Length == 0))
				{
					if (GUILayout.Button("Add Exit Transition"))
					{
						animatorStates.AddExitTransition(AnimatorConditionSettings);
					}
				}
				using (new EditorGUI.DisabledGroupScope(AnimatorConditionSettings.Count == 0))
				{
					if (GUILayout.Button("Clear Condition Settings"))
					{
						AnimatorConditionSettings.Clear();
					}
				}

				for (int i = 0; i < animatorStates.Length; i++)
				{
					EditorGUILayout.ObjectField($"{i}", animatorStates[i], typeof(AnimatorState), false);
				}

				EditorGUILayout.LabelField("AnimatorLayerCopy");
				SrcAnimatorController = EditorGUILayout.ObjectField("SrcAnimatorController", SrcAnimatorController, typeof(AnimatorController), false) as AnimatorController;
				using (new EditorGUI.DisabledGroupScope(SrcAnimatorController == null))
				{
					SelectedSrcLayer = EditorGUILayout.Popup("Layer", SelectedSrcLayer, SrcLayerSelection);
				}

				DstAnimatorController = EditorGUILayout.ObjectField("DstAnimatorController", DstAnimatorController, typeof(AnimatorController), false) as AnimatorController;
				using (new EditorGUI.DisabledGroupScope(DstAnimatorController == null))
				{
					SelectedDstLayer = EditorGUILayout.Popup("Layer", SelectedDstLayer, DstLayerSelection);
				}

				using (new EditorGUI.DisabledGroupScope(SrcAnimatorController == null || DstAnimatorController == null || SrcAnimatorController == DstAnimatorController))
				{
					if (GUILayout.Button("Copy")) transitions.SetTransitionDuration(TransitionDuration);
				}

				EditorGUILayout.Space();

				BoneSourceA = EditorGUILayout.ObjectField("BoneSourceA", BoneSourceA, typeof(Transform), true) as Transform;
				BoneSourceB = EditorGUILayout.ObjectField("BoneSourceB", BoneSourceB, typeof(Transform), true) as Transform;
				UpTarget = EditorGUILayout.ObjectField("UpTarget", UpTarget, typeof(Transform), true) as Transform;
				using (new EditorGUI.DisabledGroupScope(BoneSourceA == null || BoneSourceB == null))
				{
					if (GUILayout.Button("Generate Tip Bone")) AnimatorTools.GenerateTipBone(BoneSourceA, BoneSourceB, UpTarget);
				}
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}
}
namespace KLabs.Utils.Experimental
{

	public static class AnimatorTools
	{
		public static void AddExitTransition(this AnimatorState[] self, IEnumerable<AnimatorCondition> conditions, bool hasExitTime = true, float duration = 0.3f)
		{
			for (int i = 0; i < self.Length; i++)
			{
				var transition = self[i].AddExitTransition();
				foreach (var condition in conditions)
				{
					transition.hasExitTime = hasExitTime;
					transition.duration = duration;
					transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
				}
			}
		}

		public static void SetHasExitTime(this AnimatorStateTransition[] self, bool value)
		{
			for (int i = 0; i < self.Length; i++)
			{
				self[i].hasExitTime = value;
			}
		}

		public static void SetTransitionDuration(this AnimatorStateTransition[] self, float value)
		{
			for (int i = 0; i < self.Length; i++)
			{
				self[i].duration = value;
			}
		}

		public static Transform GenerateTipBone(Transform a, Transform b, Transform UpTarget)
		{
			var c = new GameObject("TipBone").transform;
			c.parent = b;
			c.position = b.position + (b.position - a.position);
			c.LookAt(b, UpTarget != null ? (UpTarget.position - b.position).normalized : Vector3.up);
			return c;
		}
	}
}
