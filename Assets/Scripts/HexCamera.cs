using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HexCamera : MonoBehaviour
    {
        // #pragma warning disable 649
        // [SerializeField] private Camera mainCam;
        // #pragma warning restore 649
        
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float scrollSpeed = 3f;
        [SerializeField] private float scrollFactor = 0.1f;

        public Camera Camera { get; private set; }
        
        private GameManager game;

        private float currentZoom = 0.8f;
        private float defaultYPos;
        private float defaultRotation;
        
        private void Awake()
        {
            Camera = GetComponent<Camera>();
            game = FindObjectOfType<GameManager>();
            currentZoom = 1f;
            defaultYPos = transform.position.y;
            defaultRotation = transform.eulerAngles.x;
        }

        private void Update()
        {
            if (!game.IsRunning()) return;
            ActionInput();
            UpdateCameraZoom();
        }

        private void ActionInput()
        {
            var horizontal = Input.GetAxis(Inputs.HorizontalAxis);
            var vertical = Input.GetAxis(Inputs.VerticalAxis);

            if (Mathf.Abs(horizontal) > 0.1f)
            {
                transform.position += Vector3.right * (moveSpeed * horizontal * Time.deltaTime);
            }

            if (Math.Abs(vertical) > 0.1f)
            {
                transform.position += Vector3.forward * (moveSpeed * vertical * Time.deltaTime);
            }

            var scrollDelta = Input.mouseScrollDelta;
            currentZoom = Mathf.Clamp01(currentZoom - scrollDelta.y * scrollFactor);
        }

        private void UpdateCameraZoom()
        {
            const float minYPos = 3f;
            var maxYPos = defaultYPos * 2f;
            var targetYPos = Mathf.Lerp(minYPos, maxYPos, currentZoom);
            
            var camPos = transform.position;
            camPos.y = Mathf.Lerp(camPos.y, targetYPos, Time.deltaTime * scrollSpeed);
            transform.position = camPos;

            var camRot = transform.eulerAngles;
            var targetRot = currentZoom < 0.2f
                ? Mathf.Lerp(defaultRotation - 40f, defaultRotation, currentZoom / 0.2f)
                : defaultRotation;
            
            camRot.x = Mathf.Lerp(camRot.x, targetRot, Time.deltaTime * scrollSpeed);
            transform.eulerAngles = camRot;
        }
    }
}