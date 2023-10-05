using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class OutlineShapeProperties : ShapeProperties
    {
        [SerializeField] public bool drawFill = true;
        [SerializeField] public bool drawFillShadow = true;
        [SerializeField] public bool drawOutline = false;
        [SerializeField] public Color32 outlineColor = Color.white;
        [SerializeField] public bool drawOutlineShadow = false;
    }
}
