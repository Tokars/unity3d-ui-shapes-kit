using UnityEditor;
using UnityEngine;

namespace UIShapeKit.Editor.CustomDrawers
{
    public class PointListDrawer
    {
        private static Vector3 s_worldPosition;
        private static Vector3 s_uiNormal = Vector3.forward;
        private static Vector3 s_tmpUiPos = Vector3.zero;
        private static Vector3 s_tmpUiPos2 = Vector3.zero;

        private static Vector3 s_draggedPosition = Vector3.zero;
        private static Vector2 s_offset = Vector3.zero;

        public static bool Draw(
            ref Vector2[] positions,
            RectTransform rectTransform,
            bool isClosed,
            int minPoints
        )
        {
            bool needsUpdate = false;

            bool runDelete = Event.current.modifiers == EventModifiers.Control;
            bool axisSnapping = Event.current.modifiers == EventModifiers.Shift;

            if (runDelete)
            {
                needsUpdate |= DrawRemovePointPosition(ref positions, rectTransform, minPoints);
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    needsUpdate |= DrawUpdatePointPosition(ref positions[i], rectTransform, axisSnapping);
                }

                needsUpdate |= DrawInbetweenButtons(ref positions, rectTransform, isClosed);
            }

            return needsUpdate;
        }

        private static bool DrawUpdatePointPosition(
            ref Vector2 position,
            RectTransform rectTransform,
            bool axisSnapping
        )
        {
            s_worldPosition = rectTransform.TransformPoint(position);

            s_draggedPosition = rectTransform.InverseTransformPoint(
                Handles.FreeMoveHandle(
                    s_worldPosition,
                    HandleUtility.GetHandleSize(s_worldPosition) * 0.1f,
                    Vector3.zero,
                    DrawPointHandle
                )
            );

            s_offset.x = s_draggedPosition.x - position.x;
            s_offset.y = s_draggedPosition.y - position.y;

            /// TODO snapping

            position.x += s_offset.x;
            position.y += s_offset.y;

            return s_offset.x != 0.0f || s_offset.y != 0.0f;
        }

        private static bool DrawRemovePointPosition(
            ref Vector2[] positions,
            RectTransform rectTransform,
            int minPoints
        )
        {
            bool removedPoint = false;

            for (int i = 0; i < positions.Length; i++)
            {
                s_worldPosition = rectTransform.TransformPoint(positions[i]);

                float handleSize = HandleUtility.GetHandleSize(s_worldPosition) * 0.1f;

                if (
                    Handles.Button(s_worldPosition, Quaternion.identity, handleSize, handleSize,
                        DrawRemovePointHandle) &&
                    positions.Length > minPoints
                )
                {
                    // shift other points
                    for (int j = i; j < positions.Length - 1; j++)
                    {
                        positions[j] = positions[j + 1];
                    }

                    System.Array.Resize(ref positions, positions.Length - 1);

                    removedPoint = true;
                }
            }

            return removedPoint;
        }

        private static bool DrawInbetweenButtons(
            ref Vector2[] positions,
            RectTransform rectTransform,
            bool isClosed
        )
        {
            bool addedPoint = false;

            Handles.color = Color.red;

            float handleSize;

            for (int i = positions.Length - 2; i >= 0; i--)
            {
                s_worldPosition.x = (positions[i].x + positions[i + 1].x) * 0.5f;
                s_worldPosition.y = (positions[i].y + positions[i + 1].y) * 0.5f;
                s_worldPosition.z = 0.0f;

                s_worldPosition = rectTransform.TransformPoint(s_worldPosition);

                handleSize = HandleUtility.GetHandleSize(s_worldPosition) * 0.08f;

                if (
                    Handles.Button(s_worldPosition, Quaternion.identity, handleSize, handleSize, DrawAddPointHandle)
                )
                {
                    System.Array.Resize(ref positions, positions.Length + 1);

                    // shift other points
                    for (int j = positions.Length - 1; j > i; j--)
                    {
                        positions[j] = positions[j - 1];
                    }

                    positions[i + 1] = rectTransform.InverseTransformPoint(s_worldPosition);

                    addedPoint = true;
                }
            }

            if (isClosed)
            {
                s_worldPosition.x = (positions[0].x + positions[^1].x) * 0.5f;
                s_worldPosition.y = (positions[0].y + positions[^1].y) * 0.5f;
                s_worldPosition.z = 0.0f;

                s_worldPosition = rectTransform.TransformPoint(s_worldPosition);

                handleSize = HandleUtility.GetHandleSize(s_worldPosition) * 0.08f;

                if (
                    Handles.Button(s_worldPosition, Quaternion.identity, handleSize, handleSize, DrawAddPointHandle)
                )
                {
                    System.Array.Resize(ref positions, positions.Length + 1);

                    positions[^1] = rectTransform.InverseTransformPoint(s_worldPosition);

                    // slightly offset positionif there is a closed loop and the new point is right between the two other points
                    if (isClosed && positions.Length == 3)
                    {
                        positions[^1].y += 0.1f;
                    }

                    addedPoint = true;
                }
            }

            return addedPoint;
        }

        private static void DrawPointHandle(int controlId, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            Handles.color = Color.black;

            Handles.DrawSolidDisc(position, s_uiNormal, size * 1.4f);

            Handles.color = Color.white;
            Handles.DrawSolidDisc(position, s_uiNormal, size);
            Handles.CircleHandleCap(controlId, position, rotation, size, eventType);

            Handles.color = Color.black;
            Handles.DrawSolidDisc(position, s_uiNormal, size * 0.8f);
        }

        private static void DrawRemovePointHandle(int controlId, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            Handles.color = Color.black;

            Handles.DrawSolidDisc(position, s_uiNormal, size * 1.4f);
            Handles.CircleHandleCap(controlId, position, rotation, size * 1.4f, eventType);

            Handles.color = Color.red;
            Handles.DrawSolidDisc(position, s_uiNormal, size);

            Handles.color = Color.black;
            Handles.DrawSolidDisc(position, s_uiNormal, size * 0.8f);
        }

        private static void DrawAddPointHandle(int controlId, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            Handles.color = Color.black;
            Handles.CircleHandleCap(controlId, position, rotation, size, eventType);
            Handles.DrawSolidDisc(position, s_uiNormal, size);

            Handles.color = Color.white;
            Handles.DrawSolidDisc(position, s_uiNormal, size * 0.2f);
        }

        static void DrawPlus(Vector3 position, float size)
        {
            s_tmpUiPos.x = position.x - size * 0.5f;
            s_tmpUiPos.y = position.y;
            s_tmpUiPos.z = position.z;

            s_tmpUiPos2.x = position.x + size * 0.5f;
            s_tmpUiPos2.y = position.y;
            s_tmpUiPos2.z = position.z;

            Handles.DrawLine(s_tmpUiPos, s_tmpUiPos2);

            s_tmpUiPos.x = position.x;
            s_tmpUiPos.y = position.y - size * 0.5f;

            s_tmpUiPos2.x = position.x;
            s_tmpUiPos2.y = position.y + size * 0.5f;

            Handles.DrawLine(s_tmpUiPos, s_tmpUiPos2);
        }
    }
}
