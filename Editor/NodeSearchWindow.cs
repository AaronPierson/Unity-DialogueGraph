using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    //Get obj from other classes
    private EditorWindow _window;
    private DialogueGraphView _graphView;
    private Texture2D _indeantationIcon;
    public void Init(EditorWindow window, DialogueGraphView graphView) {
        _graphView = graphView;
        _window = window;
        //Indentation hack for search window as a transparent icon
        _indeantationIcon = new Texture2D(1, 1);
        _indeantationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0)); //Dont forget to set the alpha to 0 as well
        _indeantationIcon.Apply();
    }

    //data provider
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        //setting up the search window dialog view
        var tree = new List<SearchTreeEntry>
        {
            //Groups
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            //Add node visual
            new SearchTreeEntry(new GUIContent("Dialogue Node", _indeantationIcon))
            { 
                userData = new DialogueNode(), level = 2
            }
        };

        return tree;
    }

    
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        //Get mouse position
        var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);
        //Get mouse position in editor window
        var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

        switch (SearchTreeEntry.userData)
        {
            //dio selected
            case DialogueNode dialogueNode:
                _graphView.CreateNode("Dialogue Node", localMousePosition);
                return true;
            default:
                return false;
        }
    }
}
