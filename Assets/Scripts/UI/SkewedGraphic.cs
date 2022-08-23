using UnityEngine;
using UnityEngine.UI;

namespace UI
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(SkewedGraphic))]
    public class SkewedImageInspector : Editor
    {
        private SkewedGraphic _skewedGraphic;
        private SerializedProperty _skewVector;

        protected void OnEnable()
        {
            _skewedGraphic = (SkewedGraphic)target;
            _skewVector = serializedObject.FindProperty("skewVector");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_skewVector);

            if (_skewVector.vector2Value != _skewedGraphic.skewVector)
            {
                Undo.RecordObject(_skewedGraphic, "Changed Skew");
                _skewedGraphic.skewVector = _skewVector.vector2Value;
                _skewedGraphic.CompoundRefresh();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    [ExecuteInEditMode]
    public class SkewedGraphic : GraphicComposite
    {
        [HideInInspector] public Vector2 skewVector;

        private Rect _rect = default;
#if UNITY_EDITOR

        protected override void OnEnable()
        {
            base.OnEnable();
            _rect = compoundGraphic.rectTransform.rect;
        }

        public override void CompoundRefresh()
        {
            compoundGraphic.Refresh();
        }
#endif
        public override void PopulateMesh(ref VertexHelper vh)
        {
            var height = _rect.height;
            var width = _rect.width;
            var xskew = height * Mathf.Tan(Mathf.Deg2Rad * skewVector.x);
            var yskew = width * Mathf.Tan(Mathf.Deg2Rad * skewVector.y);

            var y = _rect.yMin;
            var x = _rect.xMin;
            UIVertex v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                v.position += new Vector3(Mathf.Lerp(0, xskew, (v.position.y - y) / height),
                    Mathf.Lerp(0, yskew, (v.position.x - x) / width), 0);
                vh.SetUIVertex(v, i);
            }
        }
    }
}