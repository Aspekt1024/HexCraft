using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex
{
    public class PathIndicator : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private LineRenderer lineRenderer;
#pragma warning restore 649

        private void Awake()
        {
            Hide();
        }

        public void ShowPath(List<Vector3> path)
        {
            var points = new Vector3[path.Count];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = path[i];
                points[i].y = 0.2f;
            }

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;
        }

        public void Hide()
        {
            lineRenderer.enabled = false;
        }
    }
}