using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HexCamera : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float scrollSpeed = 3f;
        [SerializeField] private float scrollFactor = 0.1f;

#pragma warning disable 649
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera uiCamera;
#pragma warning restore 649

        public Camera MainCamera => mainCamera;
        public Camera UICamera => uiCamera;
        
        private GameManager game;

        private float currentZoom = 0.6f;
        private float defaultYPos;
        private float defaultRotation;

        private bool isBoundsSet = false;
        private Vector3 gridBounds;
        private Transform camTf;
        
        private void Awake()
        {
            game = FindObjectOfType<GameManager>();
            currentZoom = 1f;
            
            camTf = transform;
            defaultYPos = camTf.position.y;
            defaultRotation = camTf.eulerAngles.x;
        }

        private void Update()
        {
            if (!game.IsRunning()) return;
            ActionInput();
            UpdateCameraZoom();
        }

        private void LateUpdate()
        {
            HandleBounds();
        }

        public void ScrollTo(HexCoordinates coords)
        {
            var pos = HexCoordinates.ToPosition(coords);
            pos.y = transform.position.y;
            pos.z -= 5f;
            // TODO lerp
            camTf.position = pos;
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

            var camRot = camTf.eulerAngles;
            var targetRot = currentZoom < 0.2f
                ? Mathf.Lerp(defaultRotation - 40f, defaultRotation, currentZoom / 0.2f)
                : defaultRotation;
            
            camRot.x = Mathf.Lerp(camRot.x, targetRot, Time.deltaTime * scrollSpeed);
            transform.eulerAngles = camRot;
        }

        private void HandleBounds()
        {
            if (!isBoundsSet)
            {
                var grid = FindObjectOfType<HexGrid>();
                if (grid == null) return;
                
                gridBounds = grid.GetBoardLimitsInWorldUnits();
                isBoundsSet = true;
            }

            const float xAllowance = -1f;
            const float zAllowanceTop = -8f;
            const float zAllowanceBottom = 5f;

            var pos = camTf.position;
            pos.x = Mathf.Clamp(pos.x, -gridBounds.x - xAllowance, gridBounds.x + xAllowance);
            pos.z = Mathf.Clamp(pos.z, -gridBounds.z - zAllowanceBottom, gridBounds.z + zAllowanceTop);
            camTf.position = pos;
        }
    }
}