using UIShapeKit.Shapes;
using UnityEditor;
using UnityEditor.UI;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(EmptyFillRect))]
    [CanEditMultipleObjects]
    public class EmptyFillRectEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RaycastControlsGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
