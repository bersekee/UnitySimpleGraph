#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

public class GraphBlackboardView : VisualElement
{
    private readonly ScrollView _scrollView;

    private GraphBlackboardData _data;

    public event Action BlackboardChanged;

    public GraphBlackboardView()
    {
        style.width = 250;
        style.minWidth = 200;

        style.alignSelf = Align.FlexEnd;
        style.flexGrow = 0;

        style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

        style.borderLeftWidth = 1;
        style.borderLeftColor = new Color(0.25f, 0.25f, 0.25f);

        CreateHeader();

        _scrollView = new ScrollView();

        _scrollView.style.flexGrow = 1;

        Add(_scrollView);
    }

    private void CreateHeader()
    {
        VisualElement header = new VisualElement();

        header.style.height = 32;

        header.style.flexDirection = FlexDirection.Row;

        header.style.alignItems = Align.Center;

        header.style.paddingLeft = 8;
        header.style.paddingRight = 5;

        header.style.borderBottomWidth = 1;

        header.style.borderBottomColor = new Color(0.25f, 0.25f, 0.25f);

        Label title = new Label("Blackboard");

        title.style.flexGrow = 1;

        title.style.color = Color.white;

        title.style.unityFontStyleAndWeight = FontStyle.Bold;

        header.Add(title);

        Button addButton = new Button();

        addButton.text = "+";

        addButton.style.width = 24;
        addButton.style.height = 22;

        addButton.clicked += () =>
        {
            ShowAddMenu();
        };

        header.Add(addButton);

        Add(header);
    }

    public void Populate(GraphBlackboardData data)
    {
        _data = data;

        _scrollView.Clear();

        if (_data == null)
            return;

        foreach (GraphBlackboardVariableData variable in _data.Variables)
        {
            AddVariableElement(variable);
        }
    }

    private void AddVariableElement(GraphBlackboardVariableData variable)
    {
        VisualElement container = new VisualElement();

        container.style.paddingLeft = 8;
        container.style.paddingRight = 6;
        container.style.paddingTop = 5;
        container.style.paddingBottom = 6;

        container.style.borderBottomWidth = 1;

        container.style.borderBottomColor = new Color(0.23f, 0.23f, 0.23f);

        GraphBlackboardKey key = variable.Key;

        CreateVariableHeader(container, variable, key);

        CreateTypeLabel(container, variable);

        _scrollView.Add(container);
    }

    private void CreateVariableHeader(
        VisualElement container,
        GraphBlackboardVariableData variable,
        GraphBlackboardKey key
    )
    {
        VisualElement header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.alignItems = Align.Center;

        TextField nameField = new TextField();
        nameField.value = variable.Name;
        nameField.style.flexGrow = 1;
        nameField.style.marginRight = 4;
        nameField.style.maxWidth = 60;
        nameField.RegisterValueChangedCallback(evt =>
        {
            string newName = evt.newValue.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                nameField.SetValueWithoutNotify(variable.Name);

                return;
            }

            if (_data.ContainsVariable(newName) && newName != variable.Name)
            {
                nameField.SetValueWithoutNotify(variable.Name);

                return;
            }

            if (newName == variable.Name)
                return;

            _data.Rename(key, newName);

            BlackboardChanged?.Invoke();
        });
        header.Add(nameField);


        GraphBlackboardTypeRegistry.Entry? entry = GraphBlackboardTypeRegistry.Find(variable.TypeId);
        Type type = entry?.Type;
        if (type == typeof(int))
        {
            IntegerField integerField = new();
            integerField.value = variable.Value == string.Empty ? 0 : int.Parse(variable.Value);
            integerField.RegisterValueChangedCallback(evt => variable.Value = integerField.value.ToString());
            integerField.style.flexGrow = 1;
            integerField.style.marginRight = 4;
            header.Add(integerField);
        }
        else if(type == typeof(float))
        {
            FloatField floatField = new();
            floatField.value = variable.Value == string.Empty ? 0.0f : float.Parse(variable.Value);
            floatField.RegisterValueChangedCallback(evt => variable.Value = floatField.value.ToString());
            floatField.style.flexGrow = 1;
            floatField.style.marginRight = 4;
            header.Add(floatField);
        }
        else if(type == typeof(string))
        {
            TextField textField = new();
            textField.value = variable.Value;
            textField.RegisterValueChangedCallback(evt => variable.Value = textField.value.ToString());
            textField.style.flexGrow = 1;
            textField.style.marginRight = 4;
            header.Add(textField);
        }
        else if(type == typeof(bool))
        {
            Toggle booleanField = new();
            booleanField.value = variable.Value == string.Empty ? false : bool.Parse(variable.Value);
            booleanField.RegisterValueChangedCallback(evt => variable.Value = booleanField.value.ToString());
            booleanField.style.flexGrow = 1;
            booleanField.style.marginRight = 4;
            header.Add(booleanField);
        }
        else if (type == typeof(Color))
        {
            //UnityEditor.UIElements.ColorField colorField = new();
            //colorField.value = Color. variable.Value;
            //colorField.RegisterValueChangedCallback(evt => variable.Value = colorField.value.ToString());
            //colorField.style.flexGrow = 1;
            //colorField.style.marginRight = 4;
            //header.Add(colorField);
        }
        else
        {
            Debug.LogError("GraphBlackboardView: Invalid type.");
        }

        Button removeButton = new Button();
        removeButton.text = "×";
        removeButton.style.width = 20;
        removeButton.style.height = 20;
        removeButton.clicked += () =>
        {
            RemoveVariable(key);
        };
        header.Add(removeButton);

        container.Add(header);
    }

    private void CreateTypeLabel(VisualElement container, GraphBlackboardVariableData variable)
    {
        GraphBlackboardTypeRegistry.Entry? entry = GraphBlackboardTypeRegistry.Find(
            variable.TypeId
        );

        string typeName = entry.HasValue ? entry.Value.Name : variable.TypeId;

        Label typeLabel = new Label(typeName);

        typeLabel.style.fontSize = 10;

        typeLabel.style.color = new Color(0.55f, 0.55f, 0.55f);

        container.Add(typeLabel);
    }

    private void ShowAddMenu()
    {
        if (_data == null)
            return;

        GenericMenu menu = new GenericMenu();

        foreach (GraphBlackboardTypeRegistry.Entry typeEntry in GraphBlackboardTypeRegistry.Entries)
        {
            menu.AddItem(
                new GUIContent(typeEntry.Name),
                false,
                () =>
                {
                    AddVariable(typeEntry);
                }
            );
        }

        if (GraphBlackboardTypeRegistry.Entries.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No variable types registered"));
        }

        menu.ShowAsContext();
    }

    private void AddVariable(GraphBlackboardTypeRegistry.Entry typeEntry)
    {
        string name = GetUniqueVariableName(typeEntry.Name);

        _data.AddVariable(name, typeEntry.Id);

        Populate(_data);

        BlackboardChanged?.Invoke();
    }

    private void RemoveVariable(GraphBlackboardKey key)
    {
        if (_data == null)
            return;

        if (!_data.Remove(key))
            return;

        Populate(_data);

        BlackboardChanged?.Invoke();
    }

    private string GetUniqueVariableName(string baseName)
    {
        string name = baseName;

        int index = 1;

        while (_data.ContainsVariable(name))
        {
            name = $"{baseName}_{index}";

            index++;
        }

        return name;
    }
}

#endif
