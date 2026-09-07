# NodeGraph

<img width="1700" height="916" alt="Capture" src="https://github.com/user-attachments/assets/c5e0f984-6641-48db-9636-dc243ee3a7be" />

A Unity-based node graph framework for creating, editing, compiling, and
executing graph-driven logic.

The repository contains the core graph runtime, editor UI, node/port
system, blackboard variables, graph compilation, graph instances, and
JSON import/export utilities. A `StandardGraph` implementation is
included as a reference implementation with common nodes and events.

## Requirements

This repository is intended to be used inside a Unity project.

The source uses Unity APIs and editor-only APIs, including:

-   UnityEngine
-   UnityEditor
-   Unity UI Toolkit / Graph Toolkit APIs
-   Unity Visual Scripting APIs  (Graph View)
