#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class BaseGraphView : GraphView
{
    private BaseGraph _graph;
    private GraphWindow _parentWindow;
    protected GraphNodeSearchWindow _search;
    private GraphBlackboardView _blackboardView;
    private GraphTooltipView _tooltipView;
    private GridBackground _grid;
    private Dictionary<int, BaseNodeView> _nodeViewMap = new();

    public BaseGraphView(GraphWindow parentWindow)
    {
        _parentWindow = parentWindow;

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/UI/NodeGraph.uss"
        );
        styleSheets.Add(styleSheet);

        style.flexGrow = 1;

        this.StretchToParentSize();

        _grid = new GridBackground();
        _grid.StretchToParentSize();
        _grid.MarkDirtyRepaint();
        //Do not move this
        Insert(0, _grid);

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        _blackboardView = new GraphBlackboardView();
        Add(_blackboardView);

        _tooltipView = new GraphTooltipView();
        Add(_tooltipView);

        _search = ScriptableObject.CreateInstance<GraphNodeSearchWindow>();
        _search.Init(this);

        nodeCreationRequest = ctx =>
        {
            SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), _search);
        };

        graphViewChanged = OnGraphViewChanged;

        NodeExecutionEvents.OnNodeStarted += OnNodeStarted;
        NodeExecutionEvents.OnNodeFinished += OnNodeFinished;
    }

    private void OnNodeStarted(int id)
    {
        BaseNodeView nodeView = _nodeViewMap[id];
        nodeView.style.borderLeftWidth = new StyleFloat(4.0f);
        nodeView.style.borderRightWidth = new StyleFloat(4.0f);
        nodeView.style.borderBottomWidth = new StyleFloat(4.0f);
        nodeView.style.borderTopWidth = new StyleFloat(4.0f);
        nodeView.style.borderLeftColor = new StyleColor(Color.yellow);
        nodeView.style.borderRightColor = new StyleColor(Color.yellow);
        nodeView.style.borderBottomColor = new StyleColor(Color.yellow);
        nodeView.style.borderTopColor = new StyleColor(Color.yellow);
    }

    public void Save()
    {
        EditorUtility.SetDirty(_graph);
        AssetDatabase.SaveAssetIfDirty(_graph);
    }

    private void OnNodeFinished(int id)
    {
        BaseNodeView nodeView = _nodeViewMap[id];
        nodeView.style.borderLeftWidth = new StyleFloat(0.0f);
        nodeView.style.borderRightWidth = new StyleFloat(0.0f);
        nodeView.style.borderBottomWidth = new StyleFloat(0.0f);
        nodeView.style.borderTopWidth = new StyleFloat(0.0f);
    }

    public Vector2 GetLocalMousePosition(Vector2 screenMousePosition)
    {
        var windowRoot = _parentWindow.rootVisualElement;
        var windowMousePosition = windowRoot.ChangeCoordinatesTo(
            windowRoot.parent,
            screenMousePosition - _parentWindow.position.position
        );
        var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);

        return graphMousePosition;
    }

    public void Populate(BaseGraph graph)
    {
        _graph = graph;

        if (_graph == null)
            return;

        graphElements.ForEach(RemoveElement);
        _nodeViewMap.Clear();

        bool hasEntryPoint = false;

        // Create nodes
        foreach (BaseNode node in _graph.Nodes)
        {
            if (node is EntryNode)
                hasEntryPoint = true;

            BaseNodeView view = CreateNodeView(node);
            _nodeViewMap[node.UniqueId] = view;
            AddElement(view);
        }

        // Create entry point
        if (!hasEntryPoint)
        {
            BaseNodeView entryView = CreateNodeFromType(_graph.GetDefaultEntryNode(), Vector2.zero);
            _nodeViewMap[entryView.Data.UniqueId] = entryView;
        }

        // Link nodes
        foreach (BaseNode fromNode in _graph.Nodes)
        {
            foreach (NodeLink link in fromNode.OuputLinks)
            {
                BaseNode toNode = _graph.IdToNodeMap[link.ToNodeId];
                BaseNodeView fromView = _nodeViewMap[fromNode.UniqueId];
                BaseNodeView toView = _nodeViewMap[toNode.UniqueId];

                INodePort fromPort = fromNode.GetPort(link.FromPortId);
                INodePort toPort = toNode.GetPort(link.ToPortId);
                if (fromPort == null)
                {
                    Debug.LogWarning(
                        "Invalid port detected '" + link.FromPortId + "' : '" + fromNode + "'"
                    );
                    continue;
                }

                if (toPort == null)
                {
                    Debug.LogWarning(
                        "Invalid port detected '" + link.ToPortId + "' : '" + toNode + "'"
                    );
                    continue;
                }

                Port outputPort = fromView.GetPort(fromPort.Name, Direction.Output);
                Port inputPort = toView.GetPort(toPort.Name, Direction.Input);

                Edge edge = outputPort.ConnectTo<Edge>(inputPort);

                // Add custom  class
                if (toPort is NodeFlowPort)
                {
                    edge.AddToClassList("flow-edge");
                }

                AddElement(edge);
            }
        }
    }

    public BaseNodeView CreateNodeFromType(Type type, Vector2 position)
    {
        BaseNode baseNode = Activator.CreateInstance(type) as BaseNode;
        baseNode.NType.Type = type;
        baseNode.Position = new Rect(position, new Vector2(100, 100));

        BaseNodeView view = CreateNodeView(baseNode);

        _nodeViewMap.Add(baseNode.UniqueId, view);

        _graph.AddNode(baseNode);

        AddElement(view);

        return view;
    }

    private BaseNodeView CreateNodeView(BaseNode node)
    {
        return new BaseNodeView(node);
    }

    public override List<Port> GetCompatiblePorts(Port startPortView, NodeAdapter adapter)
    {
        List<Port> ports = new List<Port>();

        foreach (Node nodeView in nodes)
        {
            foreach (Port endPortView in nodeView.Query<Port>().ToList())
            {
                // prevent same node
                if (startPortView.node == endPortView.node)
                    continue;

                // prevent same direction
                if (startPortView.direction == endPortView.direction)
                    continue;

                // HACK TO IMPROVE
                BaseNode startNode = null;
                BaseNode endNode = null;
                foreach (KeyValuePair<int, BaseNodeView> item in _nodeViewMap)
                {
                    if (item.Value == nodeView)
                    {
                        endNode = _graph.FindNode(item.Key);
                    }
                    if (item.Value == startPortView.node)
                    {
                        startNode = _graph.FindNode(item.Key);
                    }
                }

                INodePort startPort = startNode.GetPortFromName(startPortView.portName);
                INodePort endPort = endNode.GetPortFromName(endPortView.portName);
                if (
                    startPort.DataType != null
                    && endPort.DataType != null
                    && endPort.DataType.IsAssignableFrom(startPort.DataType)
                )
                {
                    ports.Add(endPortView);
                }
                if (startPort.DataType == null && endPort.DataType == null)
                {
                    ports.Add(endPortView);
                }
            }
        }

        return ports;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate)
            {
                BaseNodeView inputNode = edge.input.node as BaseNodeView;
                BaseNodeView outputNode = edge.output.node as BaseNodeView;

                NodeLink link = new();
                link.FromNodeId = outputNode.Data.UniqueId;
                link.FromPortId = outputNode.Data.GetPortFromName(edge.output.portName).Id;
                link.ToNodeId = inputNode.Data.UniqueId;
                link.ToPortId = inputNode.Data.GetPortFromName(edge.input.portName).Id;

                inputNode.Data.InputLinks.Add(link);
                outputNode.Data.OuputLinks.Add(link);

                // Add custom  class
                if (outputNode.Data.GetPortFromName(edge.output.portName) is NodeFlowPort)
                {
                    edge.AddToClassList("flow-edge");
                }
            }
        }

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    BaseNodeView outputNode = edge.output.node as BaseNodeView;
                    BaseNodeView inputNode = edge.input.node as BaseNodeView;

                    inputNode.Data.InputLinks.RemoveAll(l =>
                        l.FromNodeId == outputNode.Data.UniqueId
                        && l.FromPortId == outputNode.Data.GetPortFromName(edge.output.portName).Id
                        && l.ToNodeId == inputNode.Data.UniqueId
                        && l.ToPortId == inputNode.Data.GetPortFromName(edge.input.portName).Id
                    );

                    outputNode.Data.OuputLinks.RemoveAll(l =>
                        l.FromNodeId == outputNode.Data.UniqueId
                        && l.FromPortId == outputNode.Data.GetPortFromName(edge.output.portName).Id
                        && l.ToNodeId == inputNode.Data.UniqueId
                        && l.ToPortId == inputNode.Data.GetPortFromName(edge.input.portName).Id
                    );
                }
                else if (element is BaseNodeView view)
                {
                    _graph.Nodes.Remove(view.Data);
                }
            }
        }

        return change;
    }

    public BaseGraph Graph => _graph;
    public GraphTooltipView TooltipView => _tooltipView;
}
#endif
