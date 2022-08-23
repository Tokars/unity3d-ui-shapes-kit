using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(CompoundGraphic))]
    public class E_GraphicComposite : GraphicEditor
    {
        private CompoundGraphic _composite;

        private SerializedProperty composites;
        private SerializedProperty shapePropertiesProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _composite = (CompoundGraphic)target;

            composites = serializedObject.FindProperty("composites");
            
        }
        protected override void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
            
            serializedObject.Update();

            EditorGUILayout.PropertyField(composites, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    public class CompoundGraphic : MaskableGraphic
    {
        [SerializeField] private GraphicComposite[] composites = null;

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            Refresh();
        }
#endif

        public void Refresh()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            base.OnPopulateMesh(vh);

            for (int i = 0; i < composites.Length; i++)
                composites[i].PopulateMesh(ref vh);
        }
    }
}