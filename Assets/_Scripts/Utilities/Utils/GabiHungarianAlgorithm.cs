using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public static class GabiHungarianAlgorithm
    {
	    /*
	     private float[,] GetMultiCostMatrix(in FormationData formation)
        {
            float3[] destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination);
            float[,] costMatrix = new float[formation.NumUnitsAlive, formation.NumUnitsAlive];
            
            StringBuilder assigment = new StringBuilder();
            for (int y = 0; y < formation.NumUnitsAlive; y++)
            {
                assigment.Append($"Unit index = {y}: ");
                for (int x = 0; x < formation.NumUnitsAlive; x++)
                {
                    int index = GetIndex(x, y, formation.NumUnitsAlive);
                    float3 unitPosition = RegimentAttach.Units[y].transform.position;
                    float distancePoint = distance(unitPosition, destinations[x]) + distance(unitPosition, RegimentBlackboard.Destination);
                    costMatrix[y,x] = distancePoint;
                    assigment.Append($"{x} = {distancePoint} ");
                    //costMatrix[y,x] = lengthsq(unitPosition - destinations[x]);
                }
                assigment.Append("\r\n");
            }
            Debug.Log($"CostMatrix: \r\n{assigment}");
            return costMatrix;
        }
	     */
        public static int[] FindAssignments(float[,] _costMatrix)
		{
			int length = _costMatrix.GetLength(0);
			float[] array = new float[length];
			float[] array2 = new float[length];
			bool[] array3 = new bool[length];
			bool[] array4 = new bool[length];
			int[] array5 = new int[length];
			int[] array6 = new int[length];
			float[] array7 = new float[length];
			int[] array8 = new int[length];
			int[] array9 = new int[length];
			float maxValue = float.MaxValue;
			for (int i = 0; i < length; i++)
			{
				array5[i] = -1;
				array6[i] = -1;
			}
			if (length != _costMatrix.GetLength(1))
			{
				return null;
			}
			for (int j = 0; j < length; j++)
			{
				float num = _costMatrix[j, 0];
				for (int k = 0; k < length; k++)
				{
					if (_costMatrix[j, k] < num)
					{
						num = _costMatrix[j, k];
					}
					if (num == 0f)
					{
						break;
					}
				}
				array[j] = num;
			}
			for (int l = 0; l < length; l++)
			{
				float num2 = _costMatrix[0, l] - array[0];
				for (int m = 0; m < length; m++)
				{
					if (_costMatrix[m, l] - array[m] < num2)
					{
						num2 = _costMatrix[m, l] - array[m];
					}
					if (num2 == 0f)
					{
						break;
					}
				}
				array2[l] = num2;
			}
			float num3 = 0f;
			for (int n = 0; n < length; n++)
			{
				for (int num4 = 0; num4 < length; num4++)
				{
					if (_costMatrix[n, num4] == array[n] + array2[num4] && array6[num4] == -1)
					{
						array5[n] = num4;
						array6[num4] = n;
						num3 += 1f;
						break;
					}
				}
			}
			Queue<int> queue = new Queue<int>();
			while (num3 != (float)length)
			{
				queue.Clear();
				for (int num5 = 0; num5 < length; num5++)
				{
					array3[num5] = false;
					array4[num5] = false;
				}
				int num6 = 0;
				int num7 = 0;
				int num8;
				for (num8 = 0; num8 < length; num8++)
				{
					if (array5[num8] == -1)
					{
						queue.Enqueue(num8);
						num6 = num8;
						array9[num8] = -2;
						array3[num8] = true;
						break;
					}
				}
				for (int num9 = 0; num9 < length; num9++)
				{
					array7[num9] = _costMatrix[num6, num9] - array[num6] - array2[num9];
					array8[num9] = num6;
				}
				do
				{
					if (queue.Count != 0)
					{
						num8 = queue.Dequeue();
						float num10 = array[num8];
						for (num7 = 0; num7 < length; num7++)
						{
							if (_costMatrix[num8, num7] == num10 + array2[num7] && !array4[num7])
							{
								if (array6[num7] == -1)
								{
									break;
								}
								array4[num7] = true;
								queue.Enqueue(array6[num7]);
								array3[array6[num7]] = true;
								array9[array6[num7]] = num8;
								float num11 = array[array6[num7]];
								for (int num12 = 0; num12 < length; num12++)
								{
									if (_costMatrix[array6[num7], num12] - num11 - array2[num12] < array7[num12])
									{
										array7[num12] = _costMatrix[array6[num7], num12] - num11 - array2[num12];
										array8[num12] = array6[num7];
									}
								}
							}
						}
						if (num7 >= length)
						{
							continue;
						}
					}
					if (num7 < length)
					{
						break;
					}
					float num13 = maxValue;
					for (int num14 = 0; num14 < length; num14++)
					{
						if (!array4[num14] && num13 > array7[num14])
						{
							num13 = array7[num14];
						}
					}
					for (int num15 = 0; num15 < length; num15++)
					{
						if (array3[num15])
						{
							array[num15] += num13;
						}
						if (array4[num15])
						{
							array2[num15] -= num13;
						}
						else
						{
							array7[num15] -= num13;
						}
					}
					for (num7 = 0; num7 < length; num7++)
					{
						if (!array4[num7] && array7[num7] == 0f)
						{
							if (array6[num7] == -1)
							{
								num8 = array8[num7];
								break;
							}
							array4[num7] = true;
							if (!array3[array6[num7]])
							{
								queue.Enqueue(array6[num7]);
								array3[array6[num7]] = true;
								array9[array6[num7]] = array8[num7];
								float num16 = array[array6[num7]];
								for (int num17 = 0; num17 < length; num17++)
								{
									if (_costMatrix[array6[num7], num17] - num16 - array2[num17] < array7[num17])
									{
										array7[num17] = _costMatrix[array6[num7], num17] - num16 - array2[num17];
										array8[num17] = array6[num7];
									}
								}
							}
						}
					}
				}
				while (num7 >= length);
				num3 += 1f;
				int num18 = num8;
				int num19 = num7;
				while (num18 != -2)
				{
					int num20 = array5[num18];
					array6[num19] = num18;
					array5[num18] = num19;
					num18 = array9[num18];
					num19 = num20;
				}
			}
			return array5;
		}
        
    }
}
