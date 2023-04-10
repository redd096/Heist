using redd096.StateMachine.StateMachineRedd096;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPickDrop : ConditionTask
{
    enum ECheckPick { OnPick, OnDrop }

    [SerializeField] ECheckPick checkToDo = ECheckPick.OnPick;

    [Header("Necessary Components - default get in parent")]
    [SerializeField] DragComponent dragComponent = default;

    bool hasPickedSomething;

    private void OnEnable()
    {
        //set references
        if (dragComponent == null) dragComponent = GetStateMachineComponent<DragComponent>();

        if (dragComponent)
        {
            dragComponent.onPick += OnPick;
            dragComponent.onDrop += OnDrop;
        }
    }

    private void OnDisable()
    {
        if (dragComponent)
        {
            dragComponent.onPick -= OnPick;
            dragComponent.onDrop -= OnDrop;
        }
    }

    public override bool OnCheckTask()
    {
        return (checkToDo == ECheckPick.OnPick && hasPickedSomething) || (checkToDo == ECheckPick.OnDrop && hasPickedSomething == false);
    }

    void OnPick()
    {
        hasPickedSomething = true;
    }

    void OnDrop()
    {
        hasPickedSomething = false;
    }
}
