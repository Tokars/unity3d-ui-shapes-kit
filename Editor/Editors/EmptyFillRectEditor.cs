using UnityEditor;
using UnityEditor.UI;
using EmptyFillRect = UIShapeKit.Shapes.EmptyFillRect;

namespace UIShapeKit.Editor.Editors
{
	[CustomEditor(typeof(EmptyFillRect))]
	[CanEditMultipleObjects]
	public class EmptyFillRectEditor : GraphicEditor
	{
		protected override void OnEnable()
		{
			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			RaycastControlsGUI();

			serializedObject.ApplyModifiedProperties();
		}
	}
}
