using System;
using System.Collections.Generic;
using System.Linq;
namespace VLive.Runtime.Extensions
{
    public static class MatrixFunctions
    {
        public static double[,] MultiplyF(double[,] matrix1, double[,] matrix2)
        {
            var length1 = matrix1.GetLength(0);
            var length2 = matrix1.GetLength(1);
            var length3 = matrix2.GetLength(0);
            var length4 = matrix2.GetLength(1);
            if (length2 != length3)
                throw new InvalidOperationException(
                    "Product is undefined. n columns of first matrix must equal to n rows of second matrix");
            var numArray = new double[length1, length4];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length4; ++index2)
                {
                    for (var index3 = 0; index3 < length2; ++index3)
                        numArray[index1, index2] += matrix1[index1, index3] * matrix2[index3, index2];
                }
            }

            return numArray;
        }

        public static double[] MultiplyF(double[,] matrix1, double[] matrix2)
        {
            var length1 = matrix1.GetLength(0);
            var length2 = matrix1.GetLength(1);
            var length3 = matrix2.GetLength(0);
            if (length2 != length3)
                throw new InvalidOperationException(
                    "Product is undefined. n columns of first matrix must equal to n rows of second matrix");
            var numArray = new double[length1];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1] += matrix1[index1, index2] * matrix2[index2];
            }

            return numArray;
        }

        public static double[,] Multiply(this double[,] a, double[,] b)
        {
            var result = new double[a.GetLength(0), b.GetLength(1)];
            a.Multiply(b, result);
            return result;
        }

        public static double[][] Multiply(this double[][] a, double[][] b)
        {
            var length1 = a.Length;
            var length2 = b[0].Length;
            var result = new double[length1][];
            for (var index = 0; index < length1; ++index)
                result[index] = new double[length2];
            a.Multiply(b, result);
            return result;
        }

        public static float[][] Multiply(this float[][] a, float[][] b)
        {
            var length1 = a.Length;
            var length2 = b[0].Length;
            var result = new float[length1][];
            for (var index = 0; index < result.Length; ++index)
                result[index] = new float[length2];
            a.Multiply(b, result);
            return result;
        }

        public static double[][] Multiply(this float[][] a, double[][] b)
        {
            var length1 = a.Length;
            var length2 = b[0].Length;
            var result = new double[length1][];
            for (var index = 0; index < result.Length; ++index)
                result[index] = new double[length2];
            a.Multiply(b, result);
            return result;
        }

        public static float[,] Multiply(this float[,] a, float[,] b)
        {
            var result = new float[a.GetLength(0), b.GetLength(1)];
            a.Multiply(b, result);
            return result;
        }

        public static void Multiply(this double[,] a, double[,] b, double[,] result)
        {
            var length1 = a.GetLength(1);
            var length2 = result.GetLength(0);
            var length3 = result.GetLength(1);
            var numArray = new double[length1];
            for (var index1 = 0; index1 < length3; ++index1)
            {
                for (var index2 = 0; index2 < numArray.Length; ++index2)
                    numArray[index2] = b[index2, index1];
                for (var index3 = 0; index3 < length2; ++index3)
                {
                    var num = 0.0;
                    for (var index4 = 0; index4 < numArray.Length; ++index4)
                        num += a[index3, index4] * numArray[index4];
                    result[index3, index1] = num;
                }
            }
        }

        public static void Multiply(this double[][] a, double[][] b, double[][] result)
        {
            var length1 = a[0].Length;
            var length2 = a.Length;
            var length3 = b[0].Length;
            var numArray1 = new double[length1];
            for (var index1 = 0; index1 < length3; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray1[index2] = b[index2][index1];
                for (var index3 = 0; index3 < length2; ++index3)
                {
                    var numArray2 = a[index3];
                    var num = 0.0;
                    for (var index4 = 0; index4 < length1; ++index4)
                        num += numArray2[index4] * numArray1[index4];
                    result[index3][index1] = num;
                }
            }
        }

        public static void Multiply(this float[][] a, float[][] b, float[][] result)
        {
            var length1 = a[0].Length;
            var length2 = a.Length;
            var length3 = b[0].Length;
            var numArray1 = new float[length1];
            for (var index1 = 0; index1 < length3; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray1[index2] = b[index2][index1];
                for (var index3 = 0; index3 < length2; ++index3)
                {
                    var numArray2 = a[index3];
                    var num = 0.0f;
                    for (var index4 = 0; index4 < length1; ++index4)
                        num += numArray2[index4] * numArray1[index4];
                    result[index3][index1] = num;
                }
            }
        }

        public static void Multiply(this float[][] a, double[][] b, double[][] result)
        {
            var length1 = a[0].Length;
            var length2 = a.Length;
            var length3 = b[0].Length;
            var numArray1 = new double[length1];
            for (var index1 = 0; index1 < length3; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray1[index2] = b[index2][index1];
                for (var index3 = 0; index3 < length2; ++index3)
                {
                    var numArray2 = a[index3];
                    var num = 0.0;
                    for (var index4 = 0; index4 < length1; ++index4)
                        num += numArray2[index4] * numArray1[index4];
                    result[index3][index1] = num;
                }
            }
        }

        public static void Multiply(this float[,] a, float[,] b, float[,] result)
        {
            var length1 = a.GetLength(1);
            var length2 = a.GetLength(0);
            var length3 = b.GetLength(1);
            var numArray = new float[length1];
            for (var index1 = 0; index1 < length3; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray[index2] = b[index2, index1];
                for (var index3 = 0; index3 < length2; ++index3)
                {
                    var num = 0.0f;
                    for (var index4 = 0; index4 < length1; ++index4)
                        num += a[index3, index4] * numArray[index4];
                    result[index3, index1] = num;
                }
            }
        }

        public static double[] Multiply(this double[] rowVector, double[,] matrix)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var length3 = rowVector.Length;
            if (length1 != length3)
                throw new Exception("matrix, Matrix must have the same number of rows as the length of the vector.");
            var numArray = new double[length2];
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < rowVector.Length; ++index2)
                    numArray[index1] += rowVector[index2] * matrix[index2, index1];
            }

            return numArray;
        }

        public static float[] Multiply(this float[] rowVector, float[,] matrix)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var length3 = rowVector.Length;
            if (length1 != length3)
                throw new Exception("matrix Matrix must have the same number of rows as the length of the vector.");
            var numArray = new float[length2];
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < rowVector.Length; ++index2)
                    numArray[index1] += rowVector[index2] * matrix[index2, index1];
            }

            return numArray;
        }

        public static double[] Multiply(this double[,] matrix, double[] columnVector)
        {
            var length = matrix.GetLength(0);
            if (matrix.GetLength(1) != columnVector.Length)
                throw new Exception("columnVector Vector must have the same length as columns in the matrix.");
            var numArray = new double[length];
            for (var index1 = 0; index1 < length; ++index1)
            {
                for (var index2 = 0; index2 < columnVector.Length; ++index2)
                    numArray[index1] += matrix[index1, index2] * columnVector[index2];
            }

            return numArray;
        }

        public static double[] Multiply(this double[][] matrix, double[] columnVector)
        {
            var length = matrix.Length;
            if (matrix[0].Length != columnVector.Length)
                throw new Exception("columnVector Vector must have the same length as columns in the matrix.");
            var numArray = new double[length];
            for (var index1 = 0; index1 < length; ++index1)
            {
                for (var index2 = 0; index2 < columnVector.Length; ++index2)
                    numArray[index1] += matrix[index1][index2] * columnVector[index2];
            }

            return numArray;
        }

        public static float[] Multiply(this float[,] matrix, float[] columnVector)
        {
            var length = matrix.GetLength(0);
            if (matrix.GetLength(1) != columnVector.Length)
                throw new Exception("columnVector Vector must have the same length as columns in the matrix.");
            var numArray = new float[length];
            for (var index1 = 0; index1 < length; ++index1)
            {
                for (var index2 = 0; index2 < columnVector.Length; ++index2)
                    numArray[index1] += matrix[index1, index2] * columnVector[index2];
            }

            return numArray;
        }

        public static double[,] Multiply(this double[,] matrix, double x, bool inPlace = false)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var result = inPlace ? matrix : new double[length1, length2];
            matrix.Multiply(x, result);
            return result;
        }

        public static double[,] Multiply(this double[,] matrix, double x)
        {
            var result = new double[matrix.GetLength(0), matrix.GetLength(1)];
            matrix.Multiply(x, result);
            return result;
        }

        public static float[,] Multiply(this float[,] matrix, float x)
        {
            var result = new float[matrix.GetLength(0), matrix.GetLength(1)];
            matrix.Multiply(x, result);
            return result;
        }

        public static void Multiply(this double[,] matrix, double x, double[,] result)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    result[index2, index1] = matrix[index2, index1] * x;
            }
        }

        public static void Multiply(this float[,] matrix, float x, float[,] result)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    result[index2, index1] = matrix[index2, index1] * x;
            }
        }

        public static double[] Multiply(this double[] vector, double x)
        {
            var numArray = new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] * x;
            return numArray;
        }

        public static float[] Multiply(this float[] vector, float x)
        {
            var numArray = new float[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] * x;
            return numArray;
        }

        public static double[,] Multiply(this double x, double[,] matrix) => matrix.Multiply(x);

        public static float[,] Multiply(this float x, float[,] matrix) => matrix.Multiply(x);

        public static double[] Multiply(this double x, double[] vector) => vector.Multiply(x);

        public static float[] Multiply(this float x, float[] vector) => vector.Multiply(x);

        public static double[] Multiply(this int x, double[] vector) => vector.Multiply(x);

        public static float[] Multiply(this int x, float[] vector) => vector.Multiply(x);

        public static double[] Divide(this double x, double[] vector, bool inPlace = false)
        {
            var numArray = inPlace ? vector : new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = x / vector[index];
            return numArray;
        }

        public static double[] Divide(this int x, double[] vector, bool inPlace = false)
        {
            var numArray = inPlace ? vector : new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = x / vector[index];
            return numArray;
        }

        public static double[] Divide(this double[] vector, double x, bool inPlace = false)
        {
            var numArray = inPlace ? vector : new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] / x;
            return numArray;
        }

        public static double[] Divide(this int[] vector, double x)
        {
            var numArray = new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] / x;
            return numArray;
        }

        public static float[] Divide(this float[] vector, float x)
        {
            var numArray =
                vector != null ? new float[vector.Length] : throw new ArgumentNullException(nameof(vector));
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] / x;
            return numArray;
        }

        public static double[] Divide(this double x, double[] vector)
        {
            var numArray = new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = x / vector[index];
            return numArray;
        }

        public static double[,] Divide(this double[,] matrix, double x, bool inPlace = false)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = inPlace ? matrix : new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = matrix[index1, index2] / x;
            }

            return numArray;
        }

        public static float[,] Divide(this uint[,] matrix, float x)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = new float[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = matrix[index1, index2] / x;
            }

            return numArray;
        }

        public static double[,] Divide(this double x, double[,] matrix)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = x / matrix[index1, index2];
            }

            return numArray;
        }

        public static double[,] Divide(this int x, double[,] matrix)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = x / matrix[index1, index2];
            }

            return numArray;
        }

        public static double InnerProduct(this double[] a, double[] b)
        {
            var num = 0.0;
            if (a.Length != b.Length)
                throw new ArgumentException("Vector dimensions must match", nameof(b));
            for (var index = 0; index < a.Length; ++index)
                num += a[index] * b[index];
            return num;
        }

        public static float InnerProduct(this float[] a, float[] b)
        {
            var num = 0.0f;
            if (a.Length != b.Length)
                throw new ArgumentException("Vector dimensions must match", nameof(b));
            for (var index = 0; index < a.Length; ++index)
                num += a[index] * b[index];
            return num;
        }

        public static double[,] OuterProduct(this double[] a, double[] b)
        {
            var numArray = new double[a.Length, b.Length];
            for (var index1 = 0; index1 < a.Length; ++index1)
            {
                for (var index2 = 0; index2 < b.Length; ++index2)
                    numArray[index1, index2] += a[index1] * b[index2];
            }

            return numArray;
        }

        public static double[] VectorProduct(double[] a, double[] b) => new[]
        {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
        };

        public static float[] VectorProduct(float[] a, float[] b) => new[]
        {
            (float)(a[1] * (double)b[2] - a[2] * (double)b[1]),
            (float)(a[2] * (double)b[0] - a[0] * (double)b[2]),
            (float)(a[0] * (double)b[1] - a[1] * (double)b[0])
        };

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(
            this IEnumerable<IEnumerable<T>> sequences)
        {
            var seed = (IEnumerable<IEnumerable<T>>)new[]
            {
                Enumerable.Empty<T>()
            };
            return sequences.Aggregate(seed,
                (accumulator, sequence) => accumulator.SelectMany(
                    accumulatorSequence => sequence,
                    (accumulatorSequence, item) =>
                        accumulatorSequence.Concat(new[]
                        {
                            item
                        })));
        }

        public static T[][] CartesianProduct<T>(params T[][] sequences)
        {
            var objs = sequences.CartesianProduct();
            var objArrayList = new List<T[]>();
            foreach (var source in objs)
                objArrayList.Add(source.ToArray());
            return objArrayList.ToArray();
        }

        public static T[][] CartesianProduct<T>(this T[] sequence1, T[] sequence2) => CartesianProduct(
            new T[][]
            {
                sequence1,
                sequence2
            });

        public static double[] KroneckerProduct(this double[] a, double[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            var numArray = new double[a.Length * b.Length];
            var num = 0;
            for (var index1 = 0; index1 < a.Length; ++index1)
            {
                for (var index2 = 0; index2 < b.Length; ++index2)
                    numArray[num++] = a[index1] * b[index2];
            }

            return numArray;
        }

        public static double[,] Add(this double[,] matrix, double x)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = matrix[index1, index2] + x;
            }

            return numArray;
        }

        public static double[,] Add(this double x, double[,] matrix) => Add(matrix, x);

        public static double[,] Add(this double[,] a, double[,] b)
        {
            var length1 = a.GetLength(0) == b.GetLength(0) && a.GetLength(1) == b.GetLength(1)
                ? a.GetLength(0)
                : throw new ArgumentException("Matrix dimensions must match", nameof(b));
            var length2 = a.GetLength(1);
            var length3 = a.Length;
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray[index1, index2] = a[index1, index2] + b[index1, index2];
            }

            return numArray;
        }

        public static double[][] Add(this double[][] a, double[][] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Matrix dimensions must match", nameof(b));
            var numArray = new double[a.Length][];
            for (var index1 = 0; index1 < a.Length; ++index1)
            {
                numArray[index1] = new double[a[index1].Length];
                for (var index2 = 0; index2 < a[index1].Length; ++index2)
                    numArray[index1][index2] = a[index1][index2] + b[index1][index2];
            }

            return numArray;
        }

        public static double[][] Add(this double[][] a, double x)
        {
            var numArray = new double[a.Length][];
            for (var index1 = 0; index1 < a.Length; ++index1)
            {
                numArray[index1] = new double[a[index1].Length];
                for (var index2 = 0; index2 < a[index1].Length; ++index2)
                    numArray[index1][index2] = a[index1][index2] + x;
            }

            return numArray;
        }

        public static double[,] Add(this double[,] matrix, double[] vector, int dimension)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            if (dimension == 1)
            {
                if (length1 != vector.Length)
                    throw new Exception("vector Length of vector should equal the number of rows in matrix.");
                for (var index1 = 0; index1 < length2; ++index1)
                {
                    for (var index2 = 0; index2 < length1; ++index2)
                        numArray[index2, index1] = matrix[index2, index1] + vector[index2];
                }
            }
            else
            {
                if (length2 != vector.Length)
                    throw new Exception("vector Length of vector should equal the number of columns in matrix.");
                for (var index3 = 0; index3 < length1; ++index3)
                {
                    for (var index4 = 0; index4 < length2; ++index4)
                        numArray[index3, index4] = matrix[index3, index4] + vector[index4];
                }
            }

            return numArray;
        }

        public static double[,] AddToDiagonal(this double[,] matrix, double scalar, bool inPlace = false)
        {
            var num = Math.Min(matrix.GetLength(0), matrix.GetLength(1));
            var diagonal = inPlace ? matrix : (double[,])matrix.Clone();
            for (var index = 0; index < num; ++index)
                diagonal[index, index] = matrix[index, index] + scalar;
            return diagonal;
        }

        public static double[,] SubtractFromDiagonal(this double[,] matrix, double scalar, bool inPlace = false) =>
            matrix.AddToDiagonal(-scalar, inPlace);

        public static double[,] Subtract(this double[,] a, double[] b, int dimension = 0)
        {
            var length1 = a.GetLength(0);
            var length2 = a.GetLength(1);
            var numArray = new double[length1, length2];
            if (dimension == 1)
            {
                if (length1 != b.Length)
                    throw new ArgumentException("Length of B should equal the number of rows in A", nameof(b));
                for (var index1 = 0; index1 < length2; ++index1)
                {
                    for (var index2 = 0; index2 < length1; ++index2)
                        numArray[index2, index1] = a[index2, index1] - b[index2];
                }
            }
            else
            {
                if (length2 != b.Length)
                    throw new ArgumentException("Length of B should equal the number of cols in A", nameof(b));
                for (var index3 = 0; index3 < length1; ++index3)
                {
                    for (var index4 = 0; index4 < length2; ++index4)
                        numArray[index3, index4] = a[index3, index4] - b[index4];
                }
            }

            return numArray;
        }

        public static double[][] Subtract(this double[][] a, double[] b, int dimension = 0)
        {
            var length1 = a.Length;
            var length2 = a[0].Length;
            var numArray = new double[length1][];
            for (var index = 0; index < numArray.Length; ++index)
                numArray[index] = new double[length2];
            if (dimension == 1)
            {
                if (length1 != b.Length)
                    throw new ArgumentException("Length of B should equal the number of rows in A", nameof(b));
                for (var index1 = 0; index1 < length2; ++index1)
                {
                    for (var index2 = 0; index2 < length1; ++index2)
                        numArray[index2][index1] = a[index2][index1] - b[index2];
                }
            }
            else
            {
                if (length2 != b.Length)
                    throw new ArgumentException("Length of B should equal the number of cols in A", nameof(b));
                for (var index3 = 0; index3 < length1; ++index3)
                {
                    for (var index4 = 0; index4 < length2; ++index4)
                        numArray[index3][index4] = a[index3][index4] - b[index4];
                }
            }

            return numArray;
        }

        public static double[] Add(this double[] a, double[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            if (a.Length != b.Length)
                throw new ArgumentException("Vector lengths must match", nameof(b));
            var numArray = new double[a.Length];
            for (var index = 0; index < a.Length; ++index)
                numArray[index] = a[index] + b[index];
            return numArray;
        }

        public static double[] Add(this double[] a, double b)
        {
            var numArray = a != null ? new double[a.Length] : throw new ArgumentNullException(nameof(a));
            for (var index = 0; index < a.Length; ++index)
                numArray[index] = a[index] + b;
            return numArray;
        }

        public static double[,] Subtract(this double[,] a, double[,] b, bool inPlace = false)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            var length1 = a.GetLength(0) == b.GetLength(0) && a.GetLength(1) == b.GetLength(1)
                ? a.GetLength(0)
                : throw new ArgumentException("Matrix dimensions must match", nameof(b));
            var length2 = b.GetLength(1);
            var length3 = a.Length;
            var numArray = inPlace ? a : new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = a[index1, index2] - b[index1, index2];
            }

            return numArray;
        }

        public static double[][] Subtract(this double[][] a, double[][] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Matrix dimensions must match", nameof(b));
            var numArray = new double[a.Length][];
            for (var index1 = 0; index1 < a.Length; ++index1)
            {
                numArray[index1] = new double[a[index1].Length];
                for (var index2 = 0; index2 < a[index1].Length; ++index2)
                    numArray[index1][index2] = a[index1][index2] - b[index1][index2];
            }

            return numArray;
        }

        public static double[,] Subtract(this double[,] matrix, double x)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = matrix[index1, index2] - x;
            }

            return numArray;
        }

        public static double[,] Subtract(this double x, double[,] matrix)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = x - matrix[index1, index2];
            }

            return numArray;
        }

        public static double[,] Subtract(this int x, double[,] matrix)
        {
            var length1 = matrix != null ? matrix.GetLength(0) : throw new ArgumentNullException(nameof(matrix));
            var length2 = matrix.GetLength(1);
            var numArray = new double[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = x - matrix[index1, index2];
            }

            return numArray;
        }

        public static double[] Subtract(this double[] a, double[] b, bool inPlace = false)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vector length must match", nameof(b));
            var numArray = inPlace ? a : new double[a.Length];
            for (var index = 0; index < a.Length; ++index)
                numArray[index] = a[index] - b[index];
            return numArray;
        }

        public static int[] Subtract(this int[] a, int[] b, bool inPlace = false)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vector length must match", nameof(b));
            var numArray = inPlace ? a : new int[a.Length];
            for (var index = 0; index < a.Length; ++index)
                numArray[index] = a[index] - b[index];
            return numArray;
        }

        public static double[] Subtract(this double[] vector, double x, bool inPlace = false)
        {
            var numArray = inPlace ? vector : new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] - x;
            return numArray;
        }

        public static int[] Subtract(this int[] vector, int x, bool inPlace = false)
        {
            var numArray = inPlace ? vector : new int[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] - x;
            return numArray;
        }

        public static double[] Subtract(this double x, double[] vector)
        {
            var numArray = new double[vector.Length];
            for (var index = 0; index < vector.Length; ++index)
                numArray[index] = vector[index] - x;
            return numArray;
        }

        public static T[] Diagonal<T>(this T[,] matrix)
        {
            var objArray = matrix != null
                ? new T[matrix.GetLength(0)]
                : throw new ArgumentNullException(nameof(matrix));
            for (var index = 0; index < objArray.Length; ++index)
                objArray[index] = matrix[index, index];
            return objArray;
        }

        public static T[,] Diagonal<T>(int size, T value)
        {
            var objArray = new T[size, size];
            for (var index = 0; index < size; ++index)
                objArray[index, index] = value;
            return objArray;
        }

        public static T[,] Transpose<T>(this T[,] matrix) => matrix.Transpose(false);

        public static T[,] Transpose<T>(this T[,] matrix, bool inPlace)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            if (inPlace)
            {
                if (length1 != length2)
                    throw new ArgumentException("Only square matrices can be transposed in place.", nameof(matrix));
                for (var index1 = 0; index1 < length1; ++index1)
                {
                    for (var index2 = index1; index2 < length2; ++index2)
                    {
                        var obj = matrix[index2, index1];
                        matrix[index2, index1] = matrix[index1, index2];
                        matrix[index1, index2] = obj;
                    }
                }

                return matrix;
            }

            var objArray = new T[length2, length1];
            for (var index3 = 0; index3 < length1; ++index3)
            {
                for (var index4 = 0; index4 < length2; ++index4)
                    objArray[index4, index3] = matrix[index3, index4];
            }

            return objArray;
        }

        public static double SquareMahalanobis(this double[] x, double[] y, double[,] precision)
        {
            var numArray = new double[x.Length];
            for (var index = 0; index < x.Length; ++index)
                numArray[index] = x[index] - y[index];
            return numArray.InnerProduct(precision.Multiply(numArray));
        }

        public static double Mahalanobis(this double[] x, double[] y, double[,] precision) =>
            Math.Sqrt(x.SquareMahalanobis(y, precision));

        public static double Manhattan(this double[] x, double[] y)
        {
            var num = 0.0;
            for (var index = 0; index < x.Length; ++index)
                num += Math.Abs(x[index] - y[index]);
            return num;
        }

        public static double[,] Inverse(this double[,] matrix) => matrix.Inverse(false);

        public static double[,] Inverse(this double[,] matrix, bool inPlace)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            if (length1 != length2)
                throw new ArgumentException("Matrix must be square", nameof(matrix));
            if (length1 == 3)
            {
                var num1 = matrix[0, 0];
                var num2 = matrix[0, 1];
                var num3 = matrix[0, 2];
                var num4 = matrix[1, 0];
                var num5 = matrix[1, 1];
                var num6 = matrix[1, 2];
                var num7 = matrix[2, 0];
                var num8 = matrix[2, 1];
                var num9 = matrix[2, 2];
                var num10 = num1 * (num5 * num9 - num6 * num8) - num2 * (num4 * num9 - num6 * num7) +
                            num3 * (num4 * num8 - num5 * num7);
                if (num10 == 0.0)
                    throw new Exception();
                var num11 = 1.0 / num10;
                var numArray = inPlace ? matrix : new double[3, 3];
                numArray[0, 0] = num11 * (num5 * num9 - num6 * num8);
                numArray[0, 1] = num11 * (num3 * num8 - num2 * num9);
                numArray[0, 2] = num11 * (num2 * num6 - num3 * num5);
                numArray[1, 0] = num11 * (num6 * num7 - num4 * num9);
                numArray[1, 1] = num11 * (num1 * num9 - num3 * num7);
                numArray[1, 2] = num11 * (num3 * num4 - num1 * num6);
                numArray[2, 0] = num11 * (num4 * num8 - num5 * num7);
                numArray[2, 1] = num11 * (num2 * num7 - num1 * num8);
                numArray[2, 2] = num11 * (num1 * num5 - num2 * num4);
                return numArray;
            }

            if (length1 != 2)
                throw new ArgumentException("Matrix not Support size", nameof(matrix));
            var num12 = matrix[0, 0];
            var num13 = matrix[0, 1];
            var num14 = matrix[1, 0];
            var num15 = matrix[1, 1];
            var num16 = num12 * num15 - num13 * num14;
            if (num16 == 0.0)
                throw new Exception();
            var num17 = 1.0 / num16;
            var numArray1 = inPlace ? matrix : new double[2, 2];
            numArray1[0, 0] = num17 * num15;
            numArray1[0, 1] = -num17 * num13;
            numArray1[1, 0] = -num17 * num14;
            numArray1[1, 1] = num17 * num12;
            return numArray1;
        }

        public static double[,] Identity(int size)
        {
            var numArray = new double[size, size];
            for (var index = 0; index < size; ++index)
                numArray[index, index] = 1.0;
            return numArray;
        }
    }

    public static class MatrixExtensions
    {
        public static int[] GetSize<T>(this T[,] matrix) => new int[]
        {
            matrix.GetLength(0),
            matrix.GetLength(1)
        };

        public static int ColumnCount<T>(this T[,] matrix) => matrix.GetLength(1);

        public static int RowCount<T>(this T[,] matrix) => matrix.GetLength(0);

        public static bool IsEmpty<T>(this T[,] matrix) => matrix.RowCount() == 0 || matrix.ColumnCount() == 0;

        public static T[,] DefaultIfEmpty<T>(this T[,] matrix, T[,] defaultValue) =>
            matrix.IsEmpty() ? defaultValue : matrix;

        public static T[,] DefaultIfEmpty<T>(this T[,] matrix) => matrix.DefaultIfEmpty(new T[0, 0]);

        public static IEnumerable<T> GetAt<T>(this T[,] matrix, int[][] indices)
        {
            var numArray = indices;
            foreach (var numArray1 in numArray)
            {
                yield return matrix[numArray1[0], numArray1[1]];
            }

            numArray = null;
        }

        public static void SetAt<T>(this T[,] matrix, int[][] indices, Func<T, T> setter)
        {
            foreach (var index1 in indices)
            {
                var index2 = index1[0];
                var index3 = index1[1];
                matrix[index2, index3] = setter(matrix[index2, index3]);
            }
        }

        public static bool IsMultipliableBy(this double[,] leftMatrix, double[,] rightMatrix) =>
            leftMatrix.ColumnCount() == rightMatrix.RowCount();

        public static double[] Mean(this double[,] matrix)
        {
            var numArray = new double[matrix.GetLength(1)];
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var length3 = (double)matrix.GetLength(0);
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = 0; index2 < length1; ++index2)
                    numArray[index1] += matrix[index2, index1];
                numArray[index1] /= length3;
            }

            return numArray;
        }

        public static double[,] Covariance(this double[,] matrix, double[] means)
        {
            var length1 = matrix.GetLength(0);
            var length2 = matrix.GetLength(1);
            var num1 = (double)(length1 - 1);
            var numArray = new double[length2, length2];
            for (var index1 = 0; index1 < length2; ++index1)
            {
                for (var index2 = index1; index2 < length2; ++index2)
                {
                    var num2 = 0.0;
                    for (var index3 = 0; index3 < length1; ++index3)
                        num2 += (matrix[index3, index2] - means[index2]) * (matrix[index3, index1] - means[index1]);
                    var num3 = num2 / num1;
                    numArray[index1, index2] = num3;
                    numArray[index2, index1] = num3;
                }
            }

            return numArray;
        }

        public static double[,] Covariance(this double[,] matrix)
        {
            var means = matrix.Mean();
            return matrix.Covariance(means);
        }

        public static T[,] ToMatrix<T>(this IEnumerable<T[]> matrixRows)
        {
            var enumerable = matrixRows.ToList();
            var length1 = enumerable.First().Length;
            var length2 = enumerable.Count();
            var matrix = new T[length2, length1];
            for (var index1 = 0; index1 < length2; ++index1)
            {
                var objArray = enumerable[index1];
                for (var index2 = 0; index2 < length1; ++index2)
                    matrix[index1, index2] = objArray[index2];
            }

            return matrix;
        }

        public static T[,] ToDiagonalMatrix<T>(this T[] vector)
        {
            var length = vector.Length;
            var diagonalMatrix = new T[length, length];
            for (var index = 0; index < length; ++index)
                diagonalMatrix[index, index] = vector[index];
            return diagonalMatrix;
        }

        public static double[,] Multiply(this double[] columnVector, double[] rowVector)
        {
            if (columnVector.Length != rowVector.Length)
                throw new ArgumentException("Vector lengths must match!");
            var length = columnVector.Length;
            var numArray = new double[length, length];
            for (var index1 = 0; index1 < length; ++index1)
            {
                for (var index2 = 0; index2 < length; ++index2)
                    numArray[index1, index2] = columnVector[index1] * rowVector[index2];
            }

            return numArray;
        }

        public static float[,] Divide(this float[,] matrix, float value, bool inPlace = false)
        {
            var length1 = matrix.RowCount();
            var length2 = matrix.ColumnCount();
            var numArray = inPlace ? matrix : new float[length1, length2];
            for (var index1 = 0; index1 < length1; ++index1)
            {
                for (var index2 = 0; index2 < length2; ++index2)
                    numArray[index1, index2] = matrix[index1, index2] / value;
            }

            return numArray;
        }

        public static IEnumerable<T> GetColumn<T>(
            this IEnumerable<IList<T>> jaggedMatrix,
            int columnIndex)
        {
            return jaggedMatrix.Select(objList => objList[columnIndex]);
        }

        public static T[][] ToJaggedMatrix<T>(this IList<T> vector)
        {
            var jaggedMatrix = new T[vector.Count][];
            for (var index = 0; index < vector.Count; ++index)
                jaggedMatrix[index] = new[] { vector[index] };
            return jaggedMatrix;
        }
    }
}
