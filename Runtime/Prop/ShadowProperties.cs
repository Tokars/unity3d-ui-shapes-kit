using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class ShadowProperties
    {
        [SerializeField] public Color32 color = new(0, 0, 0, 120);
        [SerializeField] public Vector2 offset = Vector2.zero;
        [SerializeField, MinNum(0.0f)] public float size = 5.0f;
        [SerializeField, Range(0.0f, 1.0f)] public float softness = 0.5f;
    }
}
