#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.GraphToolkit.Editor;
using Unity.VisualScripting;
using UnityEditor.Categorization;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseNodeView : UnityEditor.Experimental.GraphView.Node
{
    private BaseNode _data;

    public BaseNodeView(BaseNode data)
    {
        _data = data;

        SetPosition(_data.Position);

        string name = Regex.Replace(data.GetType().Name, "(?<!^)([A-Z])", " $1");
        GraphNodeAttribute attribute = data.GetType().GetCustomAttribute<GraphNodeAttribute>();
        if (attribute != null)
        {
            name = attribute.Name;
        }

        title = "<b>" + name + "</b><br>" + data.UniqueId;

        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        RegisterCallback<MouseMoveEvent>(OnMouseMove);

        DrawAllFields();

        titleContainer.style.backgroundColor = _data.Color;

        CreatePorts();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        var newPos = GetPosition();

        if (_data.Position != newPos)
        {
            _data.Position = newPos;
        }
    }

    public Port GetPort(string portName, Direction direction)
    {
        VisualElement container = direction == Direction.Input ? inputContainer : outputContainer;

        return container.Query<Port>().ToList().FirstOrDefault(p => p.portName == portName);
    }

    private void DrawAllFields()
    {
        Type type = _data.GetType();
        if (type == null)
            return;

        VisualElement container = new VisualElement();

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            HeaderAttribute headerAttr = field.GetCustomAttribute<HeaderAttribute>();
            if (headerAttr != null)
            {
                AddVisualHeader(headerAttr.header, container);
            }

            container.Add(
                CreateField(
                    field.Name,
                    field.FieldType,
                    field.GetValue(_data),
                    value => field.SetValue(_data, value)
                )
            );
        }

        extensionContainer.Add(container);
        RefreshExpandedState();
    }

    private VisualElement CreateField(string label, Type type, object value, Action<object> setter)
    {
        if (type == typeof(int))
        {
            IntegerField field = new(label) { value = (int)value };
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type == typeof(float))
        {
            FloatField field = new(label) { value = (float)value };
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type == typeof(string))
        {
            TextField field = new(label) { value = (string)value };
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type == typeof(bool))
        {
            Toggle field = new(label) { value = (bool)value };
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type.IsEnum)
        {
            EnumField field = new(label, (Enum)value);
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type == typeof(Color))
        {
            ColorField field = new(label) { value = (Color)value };
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            ObjectField field = new(label)
            {
                objectType = type,
                allowSceneObjects = false,
                value = (UnityEngine.Object)value,
            };

            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        if (type.IsArray)
        {
            return CreateArrayField(label, (Array)value, type, setter);
        }

        return new Label($"Unsupported type: {type.Name}");
    }

    private VisualElement CreateArrayField(
        string label,
        Array array,
        Type arrayType,
        Action<object> setter
    )
    {
        Type elementType = arrayType.GetElementType();

        Foldout foldout = new() { text = $"{label} ({array.Length})" };

        VisualElement container = new();

        void Refresh()
        {
            container.Clear();

            for (int i = 0; i < array.Length; i++)
            {
                int index = i;

                VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };

                VisualElement field = CreateField(
                    $"Element {index}",
                    elementType,
                    array.GetValue(index),
                    value =>
                    {
                        array.SetValue(value, index);
                        setter(array);
                    }
                );

                Button removeButton = new(() =>
                {
                    Array newArray = Array.CreateInstance(elementType, array.Length - 1);

                    int newIndex = 0;

                    for (int oldIndex = 0; oldIndex < array.Length; oldIndex++)
                    {
                        if (oldIndex == index)
                            continue;

                        newArray.SetValue(array.GetValue(oldIndex), newIndex++);
                    }

                    array = newArray;
                    setter(array);
                    Refresh();
                })
                {
                    text = "-",
                };

                row.Add(field);
                row.Add(removeButton);

                container.Add(row);
            }
        }

        Button addButton = new(() =>
        {
            Array newArray = Array.CreateInstance(elementType, array.Length + 1);

            Array.Copy(array, newArray, array.Length);

            object defaultValue = elementType.IsValueType
                ? Activator.CreateInstance(elementType)
                : null;

            newArray.SetValue(defaultValue, newArray.Length - 1);

            array = newArray;
            setter(array);
            Refresh();
        })
        {
            text = "+",
        };

        Refresh();

        foldout.Add(container);
        foldout.Add(addButton);

        return foldout;
    }

    private void AddVisualHeader(string title, VisualElement container)
    {
        Label headerLabel = new Label(title);
        headerLabel.style.marginTop = 8;
        headerLabel.style.marginBottom = 4;
        headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

        VisualElement underline = new VisualElement();
        underline.style.height = 1;
        underline.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
        underline.style.marginBottom = 6;

        container.Add(headerLabel);
        container.Add(underline);
    }

    protected void CreatePorts()
    {
        foreach (INodePort port in _data.Ports)
        {
            if (port == null)
                continue;

            Orientation orientation = Orientation.Horizontal;
            Direction direction =
                port.Direction == PortDirection.Input ? Direction.Input : Direction.Output;
            Port.Capacity capacity =
                port.Capacity == PortCapacity.Multi ? Port.Capacity.Multi : Port.Capacity.Single;

            Port newPortView = InstantiatePort(orientation, direction, capacity, typeof(object));

            newPortView.portName = port.Name;
            newPortView.portColor = port.Color;

            if (port.Direction == PortDirection.Input)
            {
                inputContainer.Add(newPortView);
            }
            else
            {
                outputContainer.Add(newPortView);
            }
        }
        RefreshPorts();
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        BaseGraphView graphView = this.GetFirstAncestorOfType<BaseGraphView>();

        GraphNodeAttribute attribute = _data.GetType().GetCustomAttribute<GraphNodeAttribute>();

        if (attribute != null)
        {
            graphView.TooltipView.Show(attribute.Name, attribute.Description, evt.mousePosition);
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        var graphView = this.GetFirstAncestorOfType<BaseGraphView>();

        graphView.TooltipView.Move(evt.mousePosition);
    }

    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        var graphView = this.GetFirstAncestorOfType<BaseGraphView>();

        graphView.TooltipView.Hide();
    }

    public BaseNode Data => _data;
}
#endif
