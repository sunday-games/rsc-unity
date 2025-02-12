using System;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    public static class MatrixUtils
    {
        public static T[,,] GetMatrixByRange<T>(this T[,,] matrix, int firstIndex, int range)
        {
            var depth = matrix.GetLength(1);
            var width = matrix.GetLength(2);

            var targetArray = new T[range, depth, width];

            var index = 0;
            var lastIndex = firstIndex + range;

            for (var i = firstIndex; i < lastIndex; i++)
            {
                for (var j = 0; j < depth; j++)
                {
                    for (var k = 0; k < width; k++)
                    {
                        targetArray[index, j, k] = matrix[i, j, k];
                    }
                }

                index++;
            }

            return targetArray;
        }

        public static T[,,] GetMatrixFromIndex<T>(this T[,,] matrix, int firstIndex)
        {
            var depth = matrix.GetLength(1);
            var width = matrix.GetLength(2);

            var height = matrix.GetLength(0);
            var targetArray = new T[height - firstIndex, depth, width];

            var index = 0;

            for (var i = firstIndex; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < depth; j++)
                {
                    for (var k = 0; k < width; k++)
                    {
                        targetArray[index, j, k] = matrix[i, j, k];
                    }
                }

                index++;
            }

            return targetArray;
        }
        public static T[,] Get2DMatrixByIndex<T>(this T[,,] matrix, int index)
        {
            var depth = matrix.GetLength(1);
            var width = matrix.GetLength(2);

            var targetArray = new T[depth, width];

            for (var i = 0; i < depth; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    targetArray[i, j] = matrix[index, i, j];
                }
            }

            return targetArray;
        }

        public static List<T> ToList<T>(this T[,,] matrix)
        {
            var targetElements = new List<T>();

            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    for (var k = 0; k < matrix.GetLength(2); k++)
                    {
                        var element = matrix[i, j, k];
                        if (element == null)
                        {
                            continue;
                        }

                        targetElements.Add(element);
                    }
                }
            }

            return targetElements;
        }
        
        public static List<T> ToList<T>(this T[,] matrix)
        {
            var targetElements = new List<T>();

            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    var element = matrix[i, j];
                    if (element == null)
                    {
                        continue;
                    }

                    targetElements.Add(element);
                }
            }

            return targetElements;
        }
        
        public static bool TryGetElementIndex<T>(this T[,] matrix, T target, out Vector2Int index)
        {
            index = Vector2Int.zero;
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == null)
                    {
                        continue;
                    }

                    if (!matrix[i, j].Equals(target))
                    {
                        continue;
                    }

                    index = new Vector2Int(i, j);
                    return true;
                }
            }

            return false;
        }
        
        public static bool TryGetElementIndex<T>(this T[,,] matrix, T target, out Vector3Int index)
        {
            index = Vector3Int.zero;
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    for (var k = 0; k < matrix.GetLength(2); k++)
                    {
                        if (matrix[i, j, k] == null)
                        {
                            continue;
                        }

                        if (!matrix[i, j, k].Equals(target))
                        {
                            continue;
                        }

                        index = new Vector3Int(i, j, k);
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static bool HasElement<T>(this T[,,] matrix, T target)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    for (var k = 0; k < matrix.GetLength(2); k++)
                    {
                        if (matrix[i, j, k] == null)
                        {
                            continue;
                        }

                        if (matrix[i, j, k].Equals(target))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool HasIndex<T>(this T[,,] matrix, Vector3Int index)
        {
            var height = matrix.GetLength(0);
            return 
                index.x >= 0 && index.x < height &&
                index.y >= 0 && index.y < matrix.GetLength(1) && 
                index.z >= 0 && index.z < matrix.GetLength(2);
        }
    }
}