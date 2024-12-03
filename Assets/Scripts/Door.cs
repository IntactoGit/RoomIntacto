using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpen;
    public Action OpenAndCloseDoor;
   
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        OpenAndCloseDoor += SwitchDoorState;
    }

    public void SwitchDoorState()
    {
        _isOpen = !_isOpen;
        _animator.SetBool("isOpen", _isOpen);
    }
}