#if UNITY_EDITOR
using System;
using UnityEditor;

public struct ArraySerializedProperty {
    private SerializedObject _serializedObject;
    private string _name;

    public void SetLength(int size) {
        if (size == Length()) return;
        GetArraySizeProperty().intValue = size;
    }

    public int Length() {
        return GetArraySizeProperty().intValue;
    }

    public void SetEnumIntValue(int i, int enumValueIndex) {
        GetArrayElementProperty(i).enumValueIndex = enumValueIndex;
    }

    public void UpdateEnumArray<T>(T[] values) where T : Enum {
        if (Length() != values.Length) {
            SetLength(values.Length);
        }

        for (var i = 0; i < values.Length; i++) {
            var value = values[i];
            var enumValueIndex = (int)(object)value;
            SetEnumIntValue(i, enumValueIndex);
        }
    }

    private SerializedProperty GetArraySizeProperty() {
        return _serializedObject.FindProperty($"{_name}.Array.size");
    }

    private SerializedProperty GetArrayElementProperty(int i) {
        return _serializedObject.FindProperty($"{_name}.Array.data[{i}]");
    }

    public static ArraySerializedProperty Create(SerializedObject serializedObject, string name) {
        return new ArraySerializedProperty {
            _name = name,
            _serializedObject = serializedObject
        };
    }
}

#endif