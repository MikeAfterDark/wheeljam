using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent
{
    public abstract void Execute();
    public abstract void Undo();
}

public class MoveEvent : GameEvent
{
    private TileMover target;
    private Vector3 fromPosition;
    private Vector3 toPosition;

    public MoveEvent(TileMover target, Vector3 from, Vector3 to)
    {
        this.target = target;
        fromPosition = from;
        toPosition = to;
    }

    public override void Execute()
    {
        target.MoveTo(toPosition);
    }

    public override void Undo()
    {
        target.MoveTo(fromPosition);
    }
}

public class DieEvent : GameEvent
{
    private GameObject target;

    public DieEvent(GameObject target)
    {
        this.target = target;
    }

    public override void Execute()
    {
        target.SetActive(false);
    }

    public override void Undo()
    {
        target.SetActive(true);
    }
}

public class EventManager : MonoBehaviour
{
    private static Stack<GameEvent> UndoStack = new Stack<GameEvent>();
    private static Stack<GameEvent> RedoStack = new Stack<GameEvent>();

    public static void PerformEvent(GameEvent gameEvent)
    {
        gameEvent.Execute();
        UndoStack.Push(gameEvent);
        RedoStack.Clear(); // Clear redo stack when new action is performed
    }

    public static void UndoLastAction()
    {
        if (UndoStack.Count > 0)
        {
            var lastEvent = UndoStack.Pop();
            lastEvent.Undo();
            RedoStack.Push(lastEvent);
        }
    }

    public static void RedoLastAction()
    {
        if (RedoStack.Count > 0)
        {
            var lastEvent = RedoStack.Pop();
            lastEvent.Execute();
            UndoStack.Push(lastEvent);
        }
    }
}
