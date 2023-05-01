using UnityEditor;
using UnityEditor.Animations;

namespace KLabs.Utils.Editor
{
	public static class AnimatorTools
	{
		public static void SetAllAnimatorStateWriteDefaults(this AnimatorController self, bool value)
		{
			for (int i = 0; i < self.layers.Length; i++)
			{
				self.layers[i].stateMachine.SetAllAnimatorStateWriteDefaults(value);
			}
			AssetDatabase.SaveAssets();
		}

		public static void SetAllAnimatorStateWriteDefaults(this AnimatorStateMachine self, bool value)
		{
			for (int i = 0; i < self.states.Length; i++)
			{
				self.states[i].state.writeDefaultValues = value;
				EditorUtility.SetDirty(self.states[i].state);
			}
			for (int i = 0; i < self.stateMachines.Length; i++)
			{
				self.stateMachines[i].stateMachine.SetAllAnimatorStateWriteDefaults(value);
			}
		}
	}
}
