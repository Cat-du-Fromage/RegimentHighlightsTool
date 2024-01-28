using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public static class HighlightExtensionMethods
    {
        public static Vector3[] GetHighlightsPositions(this HighlightRegister register, int regimentIndex)
        {
            Vector3[] positions = new Vector3[register.Records[regimentIndex].Length];
            
            for (int i = 0; i < register.Records[regimentIndex].Length; i++)
            {
                positions[i] = register.Records[regimentIndex][i].transform.position;
            }
            return positions;
        }

        public static Vector3 GetRegimentLeaderPosition(this HighlightRegister register, int regimentIndex, int width)
        {
            Vector3 firstUnitFirstRow = register.Records[regimentIndex][0].transform.position;
            Vector3 lastUnitFirstRow = register.Records[regimentIndex][width-1].transform.position;
            return (firstUnitFirstRow + lastUnitFirstRow) / 2f;
        }
    }
}