using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class SnappedPositionAndOrientationProperties
    {
        public enum OrientationTypes
        {
            Horizontal,
            Vertical
        }

        public enum PositionTypes
        {
            Center,
            Top,
            Bottom,
            Left,
            Right
        }

        [SerializeField] public OrientationTypes orientation = OrientationTypes.Horizontal;
        [SerializeField] public PositionTypes position = PositionTypes.Center;
    }
}
