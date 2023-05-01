using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace KLabs.Utils.Experimental
{
	internal sealed class AddTransitionGUI : EditorWindow
	{
		public const string TITLE = "Add Transition Tool";

		[MenuItem("KLabs/Animator Tools/Add Transition")]
		public static void EditorWindowShow()
		{
			var window = CreateWindow<AddTransitionGUI>();
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

		public bool HasExitTime { get; set; }

		public float TransitionDuration { get; set; }

		public AnimatorState StartState { get; set; }

		public List<ConditionGenerator> AnimatorConditionGenerators { get; } = new List<ConditionGenerator>();

		private ConditionGeneratorMaker ConditionGeneratorMaker { get; set; } = new ConditionGeneratorMaker();

		private float MakeConditionThresholdExecute { get; set; } = 0;


		private static readonly string[] EMPTY_SELECTION = new string[] { string.Empty };

		private static readonly GUILayoutOption W50 = GUILayout.Width(50);
		private static readonly GUILayoutOption W90 = GUILayout.Width(90);

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
		private void OnGUI()
		{
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			try
			{
				var animatorStates = Selection.GetFiltered<AnimatorState>(SelectionMode.Unfiltered);
				EditorGUILayout.ObjectField(StartState, typeof(AnimatorState), false);
				using (new EditorGUI.DisabledGroupScope(animatorStates.Length != 1))
				{
					if (GUILayout.Button("Set Start State"))
					{
						StartState = animatorStates[0];
					}
				}
				HasExitTime = EditorGUILayout.Toggle("Has Exit Time", HasExitTime);
				TransitionDuration = Mathf.Clamp(EditorGUILayout.FloatField("Transition Duration", TransitionDuration), 0, float.MaxValue);

				if (GUILayout.Button("Add Condition Setting"))
				{
					AnimatorConditionGenerators.Add(ConditionGeneratorMaker.Make());
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					ConditionGeneratorMaker.Parameter = EditorGUILayout.TextField(ConditionGeneratorMaker.Parameter);
					ConditionGeneratorMaker.Mode = (AnimatorConditionMode)EditorGUILayout.EnumPopup(ConditionGeneratorMaker.Mode, W90);
					ConditionGeneratorMaker.Threshold = EditorGUILayout.FloatField(ConditionGeneratorMaker.Threshold, W90);
				}
				ConditionGeneratorMaker.IsIncremental = EditorGUILayout.Toggle("Incremental", ConditionGeneratorMaker.IsIncremental);
				for (int i = 0; i < AnimatorConditionGenerators.Count; i++)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField(AnimatorConditionGenerators[i].Parameter);
						EditorGUILayout.LabelField(AnimatorConditionGenerators[i].Mode.ToString(), W90);
						if (AnimatorConditionGenerators[i] is IncrementalThresholdConditionGenerator)
						{
							EditorGUILayout.LabelField($"{AnimatorConditionGenerators[i].Threshold}++");
						}
						else
						{
							EditorGUILayout.LabelField($"{AnimatorConditionGenerators[i].Threshold}");
						}

					}
				}
				using (new EditorGUI.DisabledGroupScope(AnimatorConditionGenerators.Count == 0))
				{
					if (GUILayout.Button("Clear Condition Settings"))
					{
						AnimatorConditionGenerators.Clear();
					}
				}

				using (new EditorGUI.DisabledGroupScope(StartState == null || animatorStates.Length == 0))
				{
					if (GUILayout.Button("Add Transition"))
					{
						for (int i = 0; i < animatorStates.Length; i++)
						{
							var trans = StartState.AddTransition(animatorStates[i]);
							trans.hasExitTime = HasExitTime;
							trans.duration = TransitionDuration;
							foreach (var generator in AnimatorConditionGenerators)
							{
								var cond = generator.Generate();
								trans.AddCondition(cond.mode, cond.threshold, cond.parameter);
							}
						}
					}
				}

				using (new EditorGUI.DisabledGroupScope(animatorStates.Length == 0))
				{
					if (GUILayout.Button("Add Exit Transition"))
					{
						var conds = new AnimatorCondition[AnimatorConditionGenerators.Count];
						foreach (var generator in AnimatorConditionGenerators)
						{
							for (int i = 0; i < AnimatorConditionGenerators.Count; i++)
							{
								conds[i] = AnimatorConditionGenerators[i].Generate();
							}
							animatorStates.AddExitTransition(conds, HasExitTime, TransitionDuration);
						}
					}
				}

				for (int i = 0; i < animatorStates.Length; i++)
				{
					EditorGUILayout.ObjectField($"{i}", animatorStates[i], typeof(AnimatorState), false);
				}
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}

	public sealed class ConditionGeneratorMaker
	{
		public AnimatorConditionMode Mode { get; set; }
		public float Threshold { get; set; }
		public string Parameter { get; set; }
		public bool IsIncremental { get; set; }

		public ConditionGenerator Make()
		{
			return IsIncremental
				? new IncrementalThresholdConditionGenerator(Mode, Threshold, Parameter)
				: new ConditionGenerator(Mode, Threshold, Parameter);
		}
	}

	public class ConditionGenerator
	{
		public AnimatorConditionMode Mode { get; }
		public float Threshold { get; set; }
		public string Parameter { get; }

		public ConditionGenerator(AnimatorConditionMode mode, float threshold, string parameter)
		{
			Mode = mode;
			Threshold = threshold;
			Parameter = parameter;
		}

		public virtual AnimatorCondition Generate()
		{
			return new AnimatorCondition() { mode = Mode, threshold = Threshold, parameter = Parameter };
		}
	}

	public class IncrementalThresholdConditionGenerator : ConditionGenerator
	{
		public IncrementalThresholdConditionGenerator(AnimatorConditionMode mode, float threshold, string parameter) : base(mode, threshold, parameter) { }

		public override AnimatorCondition Generate()
		{
			var cond = new AnimatorCondition() { mode = Mode, threshold = Threshold, parameter = Parameter };
			var t = (int)(Threshold + 1.5f);
			Threshold = t;
			return cond;
		}
	}
}
