using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class InteractableController : MonoBehaviour
{
    private readonly Vector3
        _startPosition = new(Screen.width / 2, Screen.height / 2, 0); // Определяем центр камеры игрока

    [SerializeField] private Transform _handPosition; // Позиция руки для подобранного предмета
    [SerializeField] private float _interactDistance; // Максимальная дистанция взаимодействия
    [SerializeField] private float _throwForce = 10f; // Сила броска предмета
    private GameObject _heldObject; // Текущий объект в руке
    [SerializeField] private Camera _playerCamera; // Ссылка на камеру игрока
    private InteractableItem _interactableItem;
    private void Update()
    {
        HighlightObject();
        // Проверяем, нажал ли игрок кнопку взаимодействия
        if (Input.GetKeyUp(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    private void LateUpdate()
    {
        // Проверяем, нажал ли игрок ЛКМ для броска
        if (Input.GetMouseButtonDown(0))
        {
            ThrowHeldObject();
        }
    }

    private void HandleInteraction()
    {
        // Если игрок уже держит объект, и он смотрит на новый объект
        Ray ray = Camera.main.ScreenPointToRay(_startPosition);
        Physics.Raycast(ray, out RaycastHit hit, _interactDistance);
        if (hit.collider != null && hit.collider.CompareTag("InteractableObject"))
        {
            GameObject targetObject = hit.collider.gameObject;
            // Если уже есть предмет в руке, выбрасываем его
            if (_heldObject != null)
            {
                DropHeldObject();
            }

            // Подбираем новый предмет
            PickUpObject(targetObject);
        }
        
        if (hit.collider == null || !hit.collider.gameObject.CompareTag("Door")) return;
        if (!Input.GetKeyUp(KeyCode.E)) return;
        var door = hit.collider.gameObject.GetComponent<Door>();
        door.OpenAndCloseDoor?.Invoke();
    }

    private void PickUpObject(GameObject targetObject)
    {
        _heldObject = targetObject;

        // Отключаем физику, чтобы предмет вел себя как часть игрока
        Rigidbody objectRigidbody = _heldObject.GetComponent<Rigidbody>();
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // Перемещаем предмет в позицию руки
        _heldObject.transform.SetParent(_handPosition);
        _heldObject.transform.localPosition = Vector3.zero;
        _heldObject.transform.localRotation = Quaternion.identity;
    }

    private void DropHeldObject()
    {
        // Убираем привязку предмета к руке
        _heldObject.transform.SetParent(null);

        // Включаем физику для предмета
        Rigidbody objectRigidbody = _heldObject.GetComponent<Rigidbody>();
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
        }

        _heldObject = null;
    }

    private void ThrowHeldObject()
    {
        if (_heldObject != null)
        {
            // Убираем привязку предмета к руке
            _heldObject.transform.SetParent(null);

            // Включаем физику и применяем силу броска
            Rigidbody objectRigidbody = _heldObject.GetComponent<Rigidbody>();
            if (objectRigidbody != null)
            {
                objectRigidbody.isKinematic = false;
                objectRigidbody.AddForce(_playerCamera.transform.forward * _throwForce, ForceMode.Impulse);
            }

            _heldObject = null;
        }
    }

    private void HighlightObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(_startPosition);
        Physics.Raycast(ray, out RaycastHit hit, _interactDistance);
        // Подсвечиваем объект, когда смотрим на него
        if (hit.collider != null && hit.collider.gameObject.CompareTag("InteractableObject"))
        {
            _interactableItem = hit.collider.GetComponent<InteractableItem>();
            _interactableItem.SetFocus();
        }
        else  // Если мы не смотрим на объект, отключаем подсветку по контуру
        {
            _interactableItem?.RemoveFocus();
        }
    }
}