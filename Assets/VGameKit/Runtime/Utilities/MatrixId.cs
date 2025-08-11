using System;
using UnityEngine;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Represents a unique identifier for a position in a matrix, defined by row and column indices.
    /// </summary>
    [Serializable]
    public class MatrixId
    {
        /// <summary>
        /// Gets the row index of the matrix position.
        /// </summary>
        [field: SerializeField] public int RowId { get; private set; }
    
        /// <summary>
        /// Gets the column index of the matrix position.
        /// </summary>
        [field: SerializeField] public int ColId { get; private set; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixId"/> class with the specified row and column indices.
        /// </summary>
        /// <param name="rowId">The row index.</param>
        /// <param name="colId">The column index.</param>
        public MatrixId(int rowId, int colId)
        {
            RowId = rowId;
            ColId = colId;
        }
    
        /// <summary>
        /// Adds two <see cref="MatrixId"/> instances by summing their row and column indices.
        /// </summary>
        /// <param name="matrixId1">The first matrix identifier.</param>
        /// <param name="matrixId2">The second matrix identifier.</param>
        /// <returns>A new <see cref="MatrixId"/> representing the sum.</returns>
        public static MatrixId operator +(MatrixId matrixId1, MatrixId matrixId2)
        {
            return new MatrixId(matrixId1.RowId + matrixId2.RowId
                , matrixId1.ColId + matrixId2.ColId);
        }
    
        /// <summary>
        /// Multiplies the row and column indices of a <see cref="MatrixId"/> by a scalar value.
        /// </summary>
        /// <param name="matrixId">The matrix identifier.</param>
        /// <param name="multiplier">The scalar multiplier.</param>
        /// <returns>A new <see cref="MatrixId"/> with multiplied indices.</returns>
        public static MatrixId operator *(MatrixId matrixId, int multiplier)
        {
            return new MatrixId(matrixId.RowId * multiplier, matrixId.ColId * multiplier);
        }
        
        /// <summary>
        /// Determines whether two <see cref="MatrixId"/> instances are equal by comparing their row and column indices.
        /// </summary>
        /// <param name="matrixId1">The first matrix identifier.</param>
        /// <param name="matrixId2">The second matrix identifier.</param>
        /// <returns><c>true</c> if both row and column indices are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(MatrixId matrixId1, MatrixId matrixId2)
        {
            // return (matrixId1 == matrixId2);
            return (matrixId1.RowId == matrixId2.RowId && matrixId1.ColId == matrixId2.ColId)
                ? true
                : false;
        }
        //
        /// <summary>
        /// Determines whether two <see cref="MatrixId"/> instances are not equal by comparing their row and column indices.
        /// </summary>
        /// <param name="matrixId1">The first matrix identifier.</param>
        /// <param name="matrixId2">The second matrix identifier.</param>
        /// <returns><c>true</c> if either the row or column indices are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(MatrixId matrixId1, MatrixId matrixId2)
        {
            return (matrixId1.RowId == matrixId2.RowId && matrixId1.ColId == matrixId2.ColId)
                ? true
                : false;
        }
    
        /// <summary>
        /// Returns a string that represents the current <see cref="MatrixId"/>.
        /// </summary>
        /// <returns>A string in the format [RowId,ColId].</returns>
        public override string ToString()
        {
            return $"[{RowId},{ColId}]";
        }
    }
}