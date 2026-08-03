using System;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BaseGame.Scripts.Manager
{
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private Camera mainCamera;
        [ShowInInspector] private Dictionary<Collider, IClickAble> dicObjClickAble = new();

        private IClickAble GetClickAble(Collider col)
        {
            if (dicObjClickAble.TryGetValue(col, out var clickAble))
            {
                return clickAble;
            }

            var e = RegisterClickAble(col);

            return e;
        }

        private IClickAble RegisterClickAble(Collider col)
        {
            var iClickAble = col.GetComponent<IClickAble>();
            return iClickAble ?? null;
        }

        private void Start()
        {
            this.UpdateAsObservable().Subscribe(OnUpdateForInput).AddTo(this);
        }

        private void OnUpdateForInput(Unit _)
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            var ray = mainCamera.ScreenPointToRay(pointer.position.value);
            if (!Physics.Raycast(ray, out var hit))
                return;

            GetClickAble(hit.collider)?.OnClick();
        }
    }
}
