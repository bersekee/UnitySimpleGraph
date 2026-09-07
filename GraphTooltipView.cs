using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphTooltipView : VisualElement
{
    readonly Label _title;
    readonly Label _description;

    public GraphTooltipView()
    {
        style.position = Position.Absolute;
        style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        style.borderBottomWidth = 1;
        style.borderTopWidth = 1;
        style.borderLeftWidth = 1;
        style.borderRightWidth = 1;
        style.borderBottomColor = Color.black;
        style.borderTopColor = Color.black;
        style.borderLeftColor = Color.black;
        style.borderRightColor = Color.black;
        style.paddingLeft = 8;
        style.paddingRight = 8;
        style.paddingTop = 6;
        style.paddingBottom = 6;
        style.maxWidth = 300;

        pickingMode = PickingMode.Ignore;
        style.display = DisplayStyle.None;

        _title = new Label();
        _title.style.unityFontStyleAndWeight = FontStyle.Bold;
        _title.style.marginBottom = 4;
        _title.style.color = Color.white;

        _description = new Label();
        _description.style.whiteSpace = WhiteSpace.Normal;
        _description.style.color = new Color(.85f, .85f, .85f);

        Add(_title);
        Add(_description);
    }

    public void Show(string title, string description, Vector2 mousePosition)
    {
        _title.text = title;
        _description.text = description;

        style.left = mousePosition.x + 16;
        style.top = mousePosition.y + 16;
        style.display = DisplayStyle.Flex;

        BringToFront();
    }

    public void Move(Vector2 mousePosition)
    {
        style.left = mousePosition.x + 16;
        style.top = mousePosition.y + 16;
    }

    public void Hide()
    {
        style.display = DisplayStyle.None;
    }
}
