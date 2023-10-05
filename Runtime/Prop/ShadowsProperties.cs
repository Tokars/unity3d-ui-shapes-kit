using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class ShadowsProperties
    {
        [SerializeField] public bool showShape = true;
        [SerializeField] public bool showShadows = true;

        [SerializeField, Range(-1.0f, 1.0f)] public float angle = 0.0f;
        [SerializeField, MinNum(0.0f)] public float distance = 0.0f;
        [SerializeField] public ShadowProperties[] shadows;

        [HideInInspector] public Vector2 offset = Vector2.zero;

        public bool ShadowsEnabled => showShadows && shadows != null && shadows.Length > 0;

        public void UpdateAdjusted()
        {
            offset.x = Mathf.Sin(angle * Mathf.PI - Mathf.PI) * distance;
            offset.y = Mathf.Cos(angle * Mathf.PI - Mathf.PI) * distance;
        }

        public Vector2 GetCenterOffset(Vector2 center, int index)
        {
            center.x += offset.x + shadows[index].offset.x;
            center.y += offset.y + shadows[index].offset.y;

            return center;
        }
    }
}
