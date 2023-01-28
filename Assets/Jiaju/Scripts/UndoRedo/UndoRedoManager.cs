using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble.Functions.Grab;

public static class UndoRedoManager
{

    public static Stack<ICommand> UndoStack = new Stack<ICommand>();
    public static Stack<ICommand> RedoStack = new Stack<ICommand>();

    public static FoamDataManager _data;

    public static void AddNewAction(ICommand newAction)
    {
        UndoStack.Push(newAction);
        RedoStack.Clear();
    }

    public static void UndoAction()
    {
        Debug.Log("Undo Button Presseed");
        if (UndoStack.Count > 0)
        {
            ICommand actionUndone = UndoStack.Pop();
            actionUndone.Undo();
            RedoStack.Push(actionUndone);
        }
    }

    public static void RedoAction()
    {
        Debug.Log("Redo Button Presseed");
        if (RedoStack.Count > 0)
        {
            ICommand actionRedone = RedoStack.Pop();
            actionRedone.Redo();
            UndoStack.Push(actionRedone);
        }
    }

    
}
