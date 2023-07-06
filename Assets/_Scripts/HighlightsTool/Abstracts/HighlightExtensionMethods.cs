using UnityEngine;

namespace KaizerWald
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
    }
}