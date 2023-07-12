using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public class PlacementRegimentHint : MonoBehaviour
    {
        private const float RADIUS = 0.5f;
        
        //ATTENTION: METTRE UN LAYER A VOTRE TERRAIN!!!
        [SerializeField] private LayerMask TerrainLayer;
        [SerializeField] private float Distance = 1.2f;
        [SerializeField] private int NumberUnit = 10;
        
        private Vector3 StartMouse, EndMouse = Vector3.zero;
        private Vector3 DirectionStartToEnd = Vector3.zero;
        
        [SerializeField] private bool startDebug = true;
        private void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (GetIntersectionPointOnTerrain(out RaycastHit hit))
                {
                    StartMouse = hit.point;
                }
            }
            
            if (Mouse.current.rightButton.isPressed)
            {
                if (GetIntersectionPointOnTerrain(out RaycastHit hit))
                {
                    EndMouse = hit.point;
                    DirectionStartToEnd = (EndMouse - StartMouse).normalized; //Attention a bien normaliser !
                }
            }
        }

        //"Dont repeat yourself" qu'on nous disait...
        private bool GetIntersectionPointOnTerrain(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, TerrainLayer);
        }

        //Exemple pour comment faire un bouton rapidement en jeu
        //Ici Permet de desactiver le debugger visuel
        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 30), "Start Debug"))
            {
                startDebug = !startDebug;
            }
        }

        private void OnDrawGizmos()
        {
            if (!startDebug) return;
            DrawMouseStartPoint();
            DrawMouseEndPoint();

            Vector3[] unitsPositions = GetUnitsPositions();
            DrawRegiment(unitsPositions);
        }

        //Exemple d'utilisation des positions calculées En dessinant une sphère à chaque itération
        private void DrawRegiment(Vector3[] unitsPositions)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < unitsPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(unitsPositions[i], RADIUS);
            }
        }

        private Vector3[] GetUnitsPositions()
        {
            Vector3[] positions = new Vector3[NumberUnit];
            for (int unitIndex = 0; unitIndex < NumberUnit; unitIndex++)
            {
                // Start: point de départ
                // DirectionStartToEnd * Distance: Direction(normalizé * Distance voulue)
                // unitIndex: multiplicateur à (DirectionStartToEnd * Distance)
                positions[unitIndex] = StartMouse + (DirectionStartToEnd * Distance) * unitIndex;
            }
            return positions;
        }

        private void DrawMouseStartPoint()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(StartMouse, RADIUS);
        }
        
        private void DrawMouseEndPoint()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(EndMouse, RADIUS);
        }
    }
}
