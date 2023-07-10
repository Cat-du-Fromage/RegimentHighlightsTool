﻿using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Vector2;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public static class FormationExtension
    {
        public static Vector2 GetUnitRelativePositionToRegiment(this in FormationData formation, int unitIndex, Vector3 regimentPosition)
        {
            Vector2 dstUnitToUnit = formation.DistanceUnitToUnit;

            int y = unitIndex / formation.Width;
            int x = unitIndex - y * formation.Width;
            int widthRow = y == formation.Depth - 1 ? formation.NumUnitsLastLine : formation.Width;
            
            Vector2 regimentBackDirection = -(float2)formation.Direction2DForward;
            Vector2 yBaseOffset = regimentBackDirection * dstUnitToUnit.y;
            Vector2 yOffset = yBaseOffset + y * dstUnitToUnit.y * regimentBackDirection;
            
            //Attention! si Width Pair: 
            int midWidth = widthRow / 2;
            bool pair = (widthRow & 1) == 0;

            Vector2 xLeftDirection = -Perpendicular(regimentBackDirection);
            Vector2 xBaseOffset = (pair ? dstUnitToUnit.x * 0.5f : 0) * xLeftDirection;
            xBaseOffset += (pair ? midWidth - 1 : midWidth) * dstUnitToUnit.x * xLeftDirection; //space MidRow -> first Unit Left
            Vector2 xOffset = xBaseOffset + x * dstUnitToUnit.x * Perpendicular(regimentBackDirection);
            return new Vector2(regimentPosition.x, regimentPosition.z) + yOffset + xOffset;
        }
        
        public static float2 GetUnitRelativePositionToRegiment(this in FormationData formation, int unitIndex, float2 regimentPosition)
        {
            float2 dstUnitToUnit = formation.DistanceUnitToUnit;

            int y = unitIndex / formation.Width;
            int x = unitIndex - y * formation.Width;
            int widthRow = select(formation.Width, formation.NumUnitsLastLine, y == formation.Depth - 1);
            //YOffset
            //On prend simplement la direction "back" du régiment à laquelle on ajoute l'espace Y d'une unité
            float2 regimentBackDirection = -(float2)formation.Direction2DForward;
            float2 yBaseOffset = regimentBackDirection * dstUnitToUnit.y;
            float2 yOffset = yBaseOffset + y * dstUnitToUnit.y * regimentBackDirection;
            
            //XOffset
            int midWidth = widthRow / 2;
            bool pair = (widthRow & 1) == 0;
            //on cherche a atteindre l'unité (0,Y) de la ligne => il nous faut la direction gauche
            float2 xLeftDirection = regimentBackDirection.CrossClockWise();
            //avec la direction on saute le nombre nécessaire d'espace, Attention si PAIR! premier saut/2!!!
            float2 xBaseOffset = select(0, dstUnitToUnit.x * 0.5f, pair) * xLeftDirection;
            //restant des sauts(moitié de rangé car on commence au centre) Attention si PAIR! réduire de 1!
            xBaseOffset += select(midWidth, midWidth-1, pair) * dstUnitToUnit.x * xLeftDirection;
            //Arrivé à unité (0,Y): direction inverse * la coord X de l'unité cherchée
            float2 xOffset = xBaseOffset + x * dstUnitToUnit.x * regimentBackDirection.CrossCounterClockWise();
            
            return regimentPosition + yOffset + xOffset;
        }
        
        public static float2 GetUnitRelativePositionToRegiment(this in FormationData formation, int unitIndex, float3 regimentPosition)
        {
            return formation.GetUnitRelativePositionToRegiment(unitIndex, regimentPosition.xz);
            /*
            float2 dstUnitToUnit = formation.DistanceUnitToUnit;

            int y = unitIndex / formation.Width;
            int x = unitIndex - y * formation.Width;
            int widthRow = select(formation.Width, formation.NumUnitsLastLine, y == formation.Depth - 1);
            //YOffset
            //On prend simplement la direction "back" du régiment à laquelle on ajoute l'espace Y d'une unité
            float2 regimentBackDirection = -(float2)formation.Direction2DForward;
            float2 yBaseOffset = regimentBackDirection * dstUnitToUnit.y;
            float2 yOffset = yBaseOffset + y * dstUnitToUnit.y * regimentBackDirection;
            
            //XOffset
            int midWidth = widthRow / 2;
            bool pair = (widthRow & 1) == 0;
            //on cherche a atteindre l'unité (0,Y) de la ligne => il nous faut la direction gauche
            float2 xLeftDirection = regimentBackDirection.Cross(true);
            //avec la direction on saute le nombre nécessaire d'espace, Attention si PAIR! premier saut/2!!!
            float2 xBaseOffset = select(0, dstUnitToUnit.x * 0.5f, pair) * xLeftDirection;
            //restant des sauts(moitié de rangé car on commence au centre) Attention si PAIR! réduire de 1!
            xBaseOffset += select(midWidth, midWidth-1, pair) * dstUnitToUnit.x * xLeftDirection;
            //Arrivé à unité (0,Y): direction inverse * la coord X de l'unité cherchée
            float2 xOffset = xBaseOffset + x * dstUnitToUnit.x * regimentBackDirection.Cross(false);
            
            return regimentPosition.xz + yOffset + xOffset;
            */
        }

        public static NativeArray<float2> GetUnitsPositionRelativeToRegiment(this FormationData formation, float2 regimentPosition)
        {
            NativeArray<float2> positions = new(formation.NumUnitsAlive, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < formation.NumUnitsAlive; i++)
            {
                positions[i] = formation.GetUnitRelativePositionToRegiment(i, regimentPosition);
            }
            return positions;
        }
        
        public static float2[] GetUnitsPositionRelativeToRegiment(this FormationData formation, float3 regimentPosition)
        {
            return formation.GetUnitsPositionRelativeToRegiment(regimentPosition.xz).ToArray();
            /*
            float2[] positions = new float2[formation.NumUnitsAlive];
            for (int i = 0; i < formation.NumUnitsAlive; i++)
                positions[i] = formation.GetUnitRelativePositionToRegiment(i, regimentPosition);
            return positions;
            */
        }
        
        public static Vector2[] GetUnitsPositionRelativeToRegiment(this FormationData formation, Vector3 regimentPosition)
        {
            float3 position = (float3)regimentPosition;
            return formation.GetUnitsPositionRelativeToRegiment(position.xz).Reinterpret<Vector2>().ToArray();
            /*
            Vector2[] positions = new Vector2[formation.NumUnitsAlive];
            for (int i = 0; i < formation.NumUnitsAlive; i++)
            {
                positions[i] = formation.GetUnitRelativePositionToRegiment(i, regimentPosition);
            }
            return positions;
            */
        }
    }
}