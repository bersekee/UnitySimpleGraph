#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class GraphCompiler
{
    private BaseGraph _graph;

    public GraphCompiler(BaseGraph graph)
    {
        _graph = graph;
    }

    public void Compile()
    {
        InitGraph();

        List<GraphPart> graphParts = new();

        List<NodeLink> entryLinks = GetEntryLinks();
        for (int i = 0; i < entryLinks.Count; i++)
        {
            NodeLink entryLink = entryLinks[i];

            BaseNode toNode = _graph.IdToNodeMap[entryLink.ToNodeId];

            GraphPart part = graphParts.Find(x => toNode.UniqueId == x.EntryNode.UniqueId);

            if (part == null)
            {
                List<NodeLink> nextEntryLinks;
                part = CreatePart(entryLink, out nextEntryLinks);

                graphParts.Add(part);
                entryLinks.AddRange(nextEntryLinks);
            }

            if (entryLink.FromNodeId != 0)
            {
                BaseNode fromNode = _graph.IdToNodeMap[entryLink.FromNodeId];
                if (
                    fromNode != null
                    && !Array.Exists(
                        fromNode.PartLinks,
                        x => x.Port == entryLink.FromPortId && x.Part == part
                    )
                )
                {
                    fromNode.PushBackPartLink(new GraphPartLink(entryLink.FromPortId, part));
                }
            }
        }

        _graph.GraphParts.AddRange(graphParts);

        ComputeRuntimeIds();

        ComputeDataLinks();

        ComputeGraphEntries();

        ComputeEventEntries();
    }

    private void InitGraph()
    {
        _graph.GraphParts.Clear();
        _graph.EntryParts.Clear();

        foreach (BaseNode node in _graph.Nodes)
        {
            node.ResetPartLinks();

            if (node is EventNode eventEntry)
            {
                eventEntry.ClearParts();
            }
        }
    }

    private void ComputeGraphEntries()
    {
        foreach (GraphPart part in _graph.GraphParts)
        {
            BaseNode node = part.EntryNode;
            if (node is EntryNode entryNode)
            {
                if (entryNode.ShouldExecuteOnRun)
                {
                    _graph.EntryParts.Add(part);
                }
            }
        }
    }

    private void ComputeEventEntries()
    {
        foreach (GraphPart part in _graph.GraphParts)
        {
            BaseNode node = part.EntryNode;
            if (node is EventNode eventEntry)
            {
                eventEntry.AddPart(part);
            }
        }
    }

    private List<NodeLink> GetEntryLinks()
    {
        List<NodeLink> entryLinks = new();
        foreach (BaseNode node in _graph.Nodes)
        {
            if (node is EntryNode)
            {
                NodeLink entryLInk = new();
                entryLInk.ToPortId = 0;
                entryLInk.ToNodeId = node.UniqueId;
                entryLInk.FromNodeId = 0;
                entryLInk.FromPortId = 0;

                entryLinks.Add(entryLInk);
            }
        }
        return entryLinks;
    }

    private void ComputeDataLinks()
    {
        foreach (BaseNode node in _graph.Nodes)
        {
            node.ResetDataLinks();

            foreach (NodeLink link in node.InputLinks)
            {
                INodePort port = node.GetPort(link.ToPortId);
                if (port.DataType != null)
                {
                    GraphNodeDataLink nodeLink = new GraphNodeDataLink();
                    nodeLink.FromPortId = link.FromPortId;
                    nodeLink.FromNode = _graph.IdToNodeMap[link.FromNodeId];

                    node.DataLinks[link.ToPortId] = nodeLink;
                }
            }
        }
    }

    private void ComputeRuntimeIds()
    {
        int currentId = 0;
        foreach (BaseNode node in _graph.Nodes)
        {
            node.RuntimeId = currentId++;
        }
    }

    private GraphPart CreatePart(NodeLink entryLink, out List<NodeLink> nextLinks)
    {
        HashSet<BaseNode> subGraphNodes = new();

        BaseNode entryNode = _graph.IdToNodeMap[entryLink.ToNodeId];

        // Step 1: Forward traversal (execution)
        nextLinks = new();
        TraverseForward(entryNode, subGraphNodes, nextLinks);

        // No longer needed
        // Step 2: Backward traversal (dependencies)
        //Queue<BaseNode> toProcess = new Queue<BaseNode>(subGraphNodes);
        //while (toProcess.Count > 0)
        //{
        //    BaseNode node = toProcess.Dequeue();

        //    foreach (NodeLink inputLink in node.InputLinks)
        //    {
        //        BaseNode input = _graph.IdToNodeMap[inputLink.FromNodeId];
        //        if (input.Id != entryNode.Id)
        //        {
        //            if (subGraphNodes.Add(input))
        //            {
        //                toProcess.Enqueue(input);
        //            }
        //        }
        //    }
        //}

        GraphPart subGraph = new();

        subGraph.Nodes.AddRange(subGraphNodes);

        return subGraph;
    }

    private void TraverseForward(
        BaseNode node,
        HashSet<BaseNode> localVisited,
        List<NodeLink> nextLinks
    )
    {
        if (node == null)
            return;

        if (!localVisited.Add(node))
            return;

        List<NodeLink> flows = node.GetNextFlowLinks();
        if (flows.Count > 1 || node.ShouldBreakIntoParts())
        {
            nextLinks.AddRange(flows);
            return;
        }

        foreach (NodeLink outputLink in flows)
        {
            TraverseForward(_graph.IdToNodeMap[outputLink.ToNodeId], localVisited, nextLinks);
        }
    }

    //// Khan algo
    //private void ComputeExecutionOrder(
    //    GraphPart subGraph
    //)
    //{
    //    List<BaseNode> nodeCopy = new(subGraph.Nodes);

    //    int n = nodeCopy.Count;
    //    Dictionary<int, int> indegree = new();
    //    Queue<int> queue = new();

    //    // Compute indegrees
    //    for (int i = 0; i < n; i++)
    //    {
    //        indegree.TryAdd(nodeCopy[i].Id, 0);
    //        foreach (NodeLink link in nodeCopy[i].OuputLinks)
    //        {
    //            if (nodeCopy.Exists(n => n == _graph.IdToNodeMap[link.ToNodeId]))
    //            {
    //                indegree.TryAdd(link.ToNodeId, 0);
    //                indegree[link.ToNodeId]++;
    //            }
    //        }
    //    }

    //    // Add all nodes with indegree 0
    //    // into the queue
    //    foreach (var degree in indegree)
    //    {
    //        if (degree.Value == 0)
    //            queue.Enqueue(degree.Key);
    //    }

    //    subGraph.Nodes.Clear();

    //    // Kahn’s Algorithm (BFS)
    //    while (queue.Count > 0)
    //    {
    //        int top = queue.Dequeue();
    //        subGraph.Nodes.Add(_graph.IdToNodeMap[top]);
    //        foreach (NodeLink link in _graph.IdToNodeMap[top].OuputLinks)
    //        {
    //            if (nodeCopy.Exists(n => n == _graph.IdToNodeMap[link.ToNodeId]))
    //            {
    //                indegree[link.ToNodeId]--;
    //                if (indegree[link.ToNodeId] == 0)
    //                    queue.Enqueue(link.ToNodeId);
    //            }
    //        }
    //    }
    //}
}
#endif
