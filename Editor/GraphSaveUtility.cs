﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility 
{

    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();


    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        if (!Edges.Any()) return; //If there are no edges(no connections) return


        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (var i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.NodeLinks.Add(new NodeLinkData
            { 
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var dialogueNode in Nodes.Where(node=>!node.EntryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData 
            { 
                Guid = dialogueNode.GUID,
                DialogueText = dialogueNode.DialogueText,
                Position = dialogueNode.GetPosition().position

            });

            //Auto creates Resource folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();

        }
    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogueContainer>(fileName);
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exists!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();

    }

    private void ConnectNodes()
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var connections = _containerCache.NodeLinks.Where(
                x => x.BaseNodeGuid == Nodes[i].GUID).ToList();

            for (var j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(),
                    (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(_containerCache.DialogueNodeData.First(
                    x => x.Guid == targetNodeGuid).Position,
                    _targetGraphView.defaultNodeSize
                    ));
            }
        }
    }
    //Link the nodes after the load
    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.DialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName ));

        }
    }

    private void ClearGraph()
    {
        //set entry points guid back from the save file. Discar existing guid
        Nodes.Find(match: x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;

        foreach (var node in Nodes)
        {
            if (node.EntryPoint) continue;

            //Remove edges that connected to this node
            Edges.Where(x => x.input.node == node).ToList()
                .ForEach(Edge => _targetGraphView.RemoveElement(Edge));
            //Then remove the node
            _targetGraphView.RemoveElement(node);
        }



    }
}
