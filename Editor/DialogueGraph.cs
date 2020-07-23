using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Narrative";

    //makes Graph apper as a window in unity
    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        var blackBoard = new Blackboard(_graphView);
        blackBoard.Add(new BlackboardSection { title = "Exposed Properties" });
        blackBoard.addItemRequested = _blackBoard => { _graphView.AddPropertyToBlackBoard(new ExposedProperty()); };
        blackBoard.editTextRequested = (blackBoard1, element, newValue) =>
        {
            //Dialogue graph view Blackboard text property then cast
            var oldPropertyName = ((BlackboardField)element).text;
            if(_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exits, Please chose another one!", "OK");
                return;
            }

            var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.ExposedProperties[propertyIndex].PropertyName = newValue;
            ((BlackboardField)element).text = newValue;

        };

        blackBoard.SetPosition(new Rect(10,30,200,300));
        _graphView.Add(blackBoard);
        _graphView.Blackboard = blackBoard;
    }

    private void GenerateMinimap()
    {
        var miniMap = new MiniMap
        {
            anchored = true   
        };
        //This will give 10 px offset from left side
        var cords = _graphView.contentContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        _graphView.Add(miniMap);

    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    //editor window
    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();
        //Show the file name on toolbar
        var filenameTextField = new TextField("File Name: ");
        filenameTextField.SetValueWithoutNotify(_fileName);
        filenameTextField.MarkDirtyRepaint();
        filenameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(filenameTextField);
       

        //button
        toolbar.Add(new Button (()=>RequestDataOperation(true)) { text = "Save Data"});
       toolbar.Add(new Button (()=>RequestDataOperation(false)) { text = "Load Data"});



        //var nodeCreateButton = new Button(() =>
        //{
        //    _graphView.CreateNode("Dialogue Node");
        //});
        //nodeCreateButton.text = "Create Node";

        //toolbar.Add(nodeCreateButton);
        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name", "OK");


        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph(_fileName);
        }
    }
}
