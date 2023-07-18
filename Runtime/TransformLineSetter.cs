using UI.Bezier;
using UIShapeKit.Shapes;

namespace UIShapeKit
{
    public class TransformLineSetter : BaseTransformPointSetter<CurvedLine>
    {
        public override void Draw()
        {
            TransformPoint[] points = new TransformPoint[trPoints.Length];
            for (int i = 0; i < trPoints.Length; i++)
            {
                points[i] = new TransformPoint(ToScreenPos(trPoints[i].Position), trPoints[i].populatePointCount);
            }
            
            line.ApplyPointsData(points);
        }

      
    }
}