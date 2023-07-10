using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public struct OrderPacketData
    {
        public readonly Vector3 PositionDestination;
        public readonly FormationData FormationDestination;

        public OrderPacketData(Vector3 position, FormationData formation)
        {
            PositionDestination = position;
            FormationDestination = formation;
        }
    }
}
