#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.MaterialProperty;

public class GraphNodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private BaseGraphView _view;

    public void Init(BaseGraphView view)
    {
        _view = view;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<string> categories = _view.Graph.GetCategories();
        categories.Sort(
            (a, b) =>
            {
                return string.Compare(a, b, StringComparison.Ordinal);
            }
        );

        List<Type> validTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass
                && !t.IsAbstract
                && t.IsSubclassOf(typeof(BaseNode))
                && categories.Contains(GetCategoryName(t))
            )
            .ToList();

        // Create Tree
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
        };

        foreach (string category in categories)
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent(category), 1));

            List<Type> typesInCategory = new();
            foreach (Type type in validTypes)
            {
                string typeCategory = GetCategoryName(type);
                if (typeCategory == category)
                {
                    typesInCategory.Add(type);
                }
            }
            typesInCategory.Sort(
                (a, b) =>
                {
                    return string.Compare(
                        GetDisplayName(a),
                        GetDisplayName(b),
                        StringComparison.Ordinal
                    );
                }
            );

            foreach (Type typeInCategory in typesInCategory)
            {
                tree.Add(
                    new SearchTreeEntry(new GUIContent(GetDisplayName(typeInCategory)))
                    {
                        level = 2,
                        userData = typeInCategory,
                    }
                );
            }
        }

        return tree;
    }

    private static string GetCategoryName(Type type)
    {
        GraphNodeAttribute attribute = type.GetCustomAttribute<GraphNodeAttribute>();
        if (attribute != null)
        {
            return attribute.Category;
        }

        return "Uncategorized";
    }

    private static string GetDisplayName(Type type)
    {
        GraphNodeAttribute attribute = type.GetCustomAttribute<GraphNodeAttribute>();
        if (attribute != null)
        {
            return attribute.Name;
        }

        return Regex.Replace(type.Name, "([A-Z])", " $1").Trim();
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (entry.userData is not Type type)
            return false;

        Vector2 graphPos = _view.GetLocalMousePosition(context.screenMousePosition);

        _view.CreateNodeFromType(type, graphPos);
        return true;
    }
}
#endif
