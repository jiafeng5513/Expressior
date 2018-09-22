using System;
using System.Collections.Generic;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    public interface ICustomNodeManager
    {
        IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(string path, bool isPackageMember = false);
        Guid GuidFromPath(string path);
        bool TryGetFunctionWorkspace(Guid id, out ICustomNodeWorkspaceModel ws);
    }
}
