using System;
using RTS.Core;
using TMPro;
using UnityEngine;

namespace RTS.Control
{
    public class CursorLock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lockedText = default;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            if (_lockedText)
                _lockedText.text = string.Empty;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || GameData.Instance.Input.Provider.PanCamera)
                Cursor.lockState = CursorLockMode.None;
            else if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Confined;

            if(_lockedText)
                _lockedText.text = Cursor.lockState == CursorLockMode.Confined ? "Press <b><color=#ffa200> Escape </color></b> to Unlock Cursor" : string.Empty;
        }
    }
}