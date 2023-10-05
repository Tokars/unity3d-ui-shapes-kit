using UnityEngine;

namespace UIShapeKit.ShapeUtils.PointsGenerators
{
	public class PointsGenerator
	{
		public static void SetPoints(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			switch (data.generator) {
				case PointsList.PointListGeneratorData.Generators.Custom:
					break;

				case PointsList.PointListGeneratorData.Generators.Rect:
					SetPointsRect(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.Round:
					SetPointsRound(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.RadialGraph:
					SetPointsRadialGraph(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.LineGraph:
					SetPointsLineGraph(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.AngleLine:
					SetPointsAngleLine(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.Star:
					SetPointsStar(ref positions, data);
					break;

				case PointsList.PointListGeneratorData.Generators.Gear:
					SetPointsGear(ref positions, data);
					break;
			}
		}

		public static void SetPointsRect(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			if (
				positions == null ||
				positions.Length != 4
			) {
				positions = new Vector2[4];
			}

			float halfWidth = data.width * 0.5f;
			float halfHeight = data.height * 0.5f;

			int offset = data.intStartOffset % 4;

			offset = 4 + offset;
			offset %= 4;

			for (int i = 0; i < 4; i++)
			{
				int index = i + offset;
				index %= 4;

				switch (index)
				{
					case 0:
						positions[i].x = data.center.x - halfWidth;
						positions[i].y = data.center.y + halfHeight;
						break;
					case 1:
						positions[i].x = data.center.x + halfWidth;
						positions[i].y = data.center.y + halfHeight;
						break;
					case 2:
						positions[i].x = data.center.x + halfWidth;
						positions[i].y = data.center.y - halfHeight;
						break;
					case 3:
						positions[i].x = data.center.x - halfWidth;
						positions[i].y = data.center.y - halfHeight;
						break;
				}
			}
		}

		public static void SetPointsRound(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			float absLength = Mathf.Abs(data.length);
			
			int numFullSteps = Mathf.CeilToInt(data.resolution * absLength);
			float partStepAmount = 1.0f + ((data.resolution * absLength) - (float)numFullSteps);

			bool addPartialStep = partStepAmount >= 0.0001f;

			int resolution = numFullSteps;

			if (addPartialStep) {
				resolution++;
			}

			if (data.centerPoint)
			{
				resolution++;
			}

			if (
				positions == null ||
				positions.Length != resolution
			) {
				positions = new Vector2[resolution];
			}

			if (data.centerPoint)
			{
				positions[resolution-1].x = data.center.x;
				positions[resolution-1].y = data.center.y;
			}

			float halfWidth = Mathf.Max(0.001f, data.width * 0.5f);
			float halfHeight = Mathf.Max(0.001f, data.height * 0.5f);

			float angle = data.floatStartOffset * GeoUtils.TwoPI;
			float angleIncrement = (GeoUtils.TwoPI) / (float)(data.resolution);

			if (data.skipLastPosition)
			{
				angleIncrement = (GeoUtils.TwoPI) / ((float)data.resolution + 1);
			}

			angleIncrement *= Mathf.Sign(data.direction);

			float relCompletion;

			for (int i = 0; i < numFullSteps; i++)
			{
				relCompletion = (float)i / (float)resolution;

				positions[i].x = data.center.x + Mathf.Sin(angle) * (halfWidth + (halfWidth * data.endRadius * relCompletion));
				positions[i].y = data.center.y + Mathf.Cos(angle) * (halfHeight + (halfHeight * data.endRadius * relCompletion));

				angle += angleIncrement;
			}

			// add last point
			if (addPartialStep)
			{
				relCompletion = ((float)numFullSteps + partStepAmount) / (float)resolution;
//				angle -= angleIncrement * (1.0f - partStepAmount);
				positions[numFullSteps].x = data.center.x + Mathf.Sin(angle) * (halfWidth + (halfWidth * data.endRadius * relCompletion));
				positions[numFullSteps].y = data.center.y + Mathf.Cos(angle) * (halfHeight + (halfHeight * data.endRadius * relCompletion));

				int prevStep = Mathf.Max(numFullSteps-1, 0);

				// lerp back to partial position
				positions[numFullSteps].x = Mathf.LerpUnclamped(positions[prevStep].x, positions[numFullSteps].x, partStepAmount);
				positions[numFullSteps].y = Mathf.LerpUnclamped(positions[prevStep].y, positions[numFullSteps].y, partStepAmount);
			}
		}

		public static void SetPointsRadialGraph(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			int resolution = data.floatValues.Length;

			if (data.floatValues.Length < 3)
				return;

			if (
				positions == null ||
				positions.Length != resolution
			) {
				positions = new Vector2[resolution];
			}

			float angle = data.floatStartOffset * GeoUtils.TwoPI;
			float angleIncrement = GeoUtils.TwoPI / (float)(resolution);

			for (int i = 0; i < resolution; i++)
			{
				float value = Mathf.InverseLerp(
					data.minFloatValue,
					data.maxFloatValue,
					data.floatValues[i]
				);

				value *= data.radius;

				positions[i].x = data.center.x + Mathf.Sin(angle) * value;
				positions[i].y = data.center.y + Mathf.Cos(angle) * value;

				angle += angleIncrement;
			}
		}

		public static void SetPointsLineGraph(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			int resolution = data.floatValues.Length;

			if (data.floatValues.Length < 2)
				return;

			if (data.centerPoint)
				resolution += 2;

			if (
				positions == null ||
				positions.Length != resolution
			) {
				positions = new Vector2[resolution];
			}

			float xPos = data.center.x + data.width * -0.5f;

			float xStep = data.width / (float)(data.floatValues.Length - 1.0f);

			for (int i = 0; i < data.floatValues.Length; i++)
			{
				float value = Mathf.InverseLerp(
					data.minFloatValue,
					data.maxFloatValue,
					data.floatValues[i]
				);

				value -= 0.5f;

				value *= data.height;

				positions[i].x = xPos;
				positions[i].y = data.center.y + value;

				xPos += xStep;
			}

			if (data.centerPoint)
			{
				positions[data.floatValues.Length].x = data.center.x + data.width * 0.5f;
				positions[data.floatValues.Length].y = data.center.y - data.height * 0.5f;

				positions[data.floatValues.Length + 1].x = data.center.x + data.width * -0.5f;
				positions[data.floatValues.Length + 1].y = positions[data.floatValues.Length].y;
			}
		}

		public static void SetPointsAngleLine(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			if (
				positions == null ||
				positions.Length != 2
			) {
				positions = new Vector2[2];
			}

			float xDir = Mathf.Sin(data.angle * GeoUtils.TwoPI);
			float yDir = Mathf.Cos(data.angle * GeoUtils.TwoPI);

			float startOffset = data.length * data.floatStartOffset;

			positions[0].x = data.center.x + xDir * startOffset;
			positions[0].y = data.center.y + yDir * startOffset;

			positions[1].x = data.center.x + xDir * (data.length + startOffset);
			positions[1].y = data.center.y + yDir * (data.length + startOffset);
		}

		public static void SetPointsStar(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			int resolution = data.resolution * 2;

			if (
				positions == null ||
				positions.Length != resolution
			) {
				positions = new Vector2[resolution];
			}

			float angle = data.floatStartOffset * GeoUtils.TwoPI;
			float angleIncrement = (GeoUtils.TwoPI * data.length) / (float)resolution;

			float outerRadiusX = data.width;
			float outerRadiusY = data.height;

			float innerRadiusX = data.endRadius * outerRadiusX;
			float innerRadiusY = data.endRadius * outerRadiusX;

			for (int i = 0; i < resolution; i+= 2)
			{
				// add outer point
				positions[i].x = data.center.x + Mathf.Sin(angle) * outerRadiusX;
				positions[i].y = data.center.y + Mathf.Cos(angle) * outerRadiusY;

				angle += angleIncrement;

				// add inner point
				positions[i+1].x = data.center.x + Mathf.Sin(angle) * innerRadiusX;
				positions[i+1].y = data.center.y + Mathf.Cos(angle) * innerRadiusY;

				angle += angleIncrement;
			}
		}

		public static void SetPointsGear(
			ref Vector2[] positions,
			PointsList.PointListGeneratorData data
		) {
			int resolution = data.resolution * 4;

			if (
				positions == null ||
				positions.Length != resolution
			) {
				positions = new Vector2[resolution];
			}

			float angle = data.floatStartOffset * GeoUtils.TwoPI;
			float angleIncrement = GeoUtils.TwoPI / (float)data.resolution;

			float outerRadiusX = data.width;
			float outerRadiusY = data.height;

			float innerRadiusX = data.endRadius * outerRadiusX;
			float innerRadiusY = data.endRadius * outerRadiusY;

			float bottomAngleOffset = angleIncrement * 0.49f * data.innerScaler;
			float topAngleOffset = angleIncrement * 0.49f * data.outerScaler;

			int index;

			for (int i = 0; i < data.resolution; i++)
			{
				index = i * 4;

				// add first inner point
				positions[index].x = data.center.x + Mathf.Sin(angle - bottomAngleOffset) * innerRadiusX;
				positions[index].y = data.center.y + Mathf.Cos(angle - bottomAngleOffset) * innerRadiusY;

				// add first outer point
				positions[index + 1].x = data.center.x + Mathf.Sin(angle - topAngleOffset) * outerRadiusX;
				positions[index + 1].y = data.center.y + Mathf.Cos(angle - topAngleOffset) * outerRadiusY;

				// add secont outer point
				positions[index + 2].x = data.center.x + Mathf.Sin(angle + topAngleOffset) * outerRadiusX;
				positions[index + 2].y = data.center.y + Mathf.Cos(angle + topAngleOffset) * outerRadiusY;

				// add second inner point
				positions[index + 3].x = data.center.x + Mathf.Sin(angle + bottomAngleOffset) * innerRadiusX;
				positions[index + 3].y = data.center.y + Mathf.Cos(angle + bottomAngleOffset) * innerRadiusY;

				angle += angleIncrement;
			}
		}
	}
}
