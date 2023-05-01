using UnityEditor;
using UnityEditor.Animations;

namespace KLabs.Utils.Editor
{
	public static class AnimatorContextMenu
	{
		private const string DISABLE_WRITE_DEFAULTS = "Assets/KLabs/AnimatorUtils/States Disable WriteDefaults";
		private const string ENABLE_WRITE_DEFAULTS = "Assets/KLabs/AnimatorUtils/States Enable WriteDefaults";

		[MenuItem(DISABLE_WRITE_DEFAULTS)]
		private static void DisableWriteDefaults()
		{
			if (Selection.objects.Length == 1 && Selection.activeObject is AnimatorController ctrl)
			{
				ctrl.SetAllAnimatorStateWriteDefaults(false);
			}
		}
		[MenuItem(DISABLE_WRITE_DEFAULTS, true)]
		private static bool ValidateDisableWriteDefaults()
		{
			return Selection.objects.Length == 1 && Selection.activeObject is AnimatorController;
		}

		[MenuItem(ENABLE_WRITE_DEFAULTS)]
		private static void EnableWriteDefaults()
		{
			if (Selection.objects.Length == 1 && Selection.activeObject is AnimatorController ctrl)
			{
				ctrl.SetAllAnimatorStateWriteDefaults(true);
			}
		}
		[MenuItem(ENABLE_WRITE_DEFAULTS, true)]
		private static bool ValidateEnableWriteDefaults()
		{
			return Selection.objects.Length == 1 && Selection.activeObject is AnimatorController;
		}
	}
}
