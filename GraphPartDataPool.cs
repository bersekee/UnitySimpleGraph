using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Pool;

public static class GraphNodeDataPool
{
    private static readonly ObjectPool<GraphPartData> Pool = new ObjectPool<GraphPartData>(
        createFunc: () => new GraphPartData(),
        actionOnGet: element => { },
        actionOnRelease: element => element.Dispose(),
        actionOnDestroy: element => { },
        collectionCheck: false,
        defaultCapacity: 16,
        maxSize: 1000
    );

    public static GraphPartData Get(BaseGraph graph, int partIndex, bool async)
    {
        GraphPartData data = Pool.Get();
        data.Init(graph, partIndex, async);
        return data;
    }

    public static void Release(GraphPartData data) => Pool.Release(data);
}
