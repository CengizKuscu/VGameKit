using Unity.Mathematics;

namespace VGameKit.Runtime.Utilities.Camera
{
    /// <summary>
    /// Provides utilities for calculating orthographic camera layouts,
    /// including aspect ratio, orthographic size, grid rows and columns,
    /// padding, origin, and unit size. Supports initialization with
    /// different parameters for flexible camera setup.
    /// </summary>
    public class OrthographicLayout
    {
        /// <summary>
        /// The default orthographic size.
        /// </summary>
        public const float DEFAULT_ORTHOGRAPHIC_SIZE = 5f;
        
        /// <summary>
        /// The aspect ratio of the camera view.
        /// </summary>
        public static float Aspect { get; private set; }
    
        /// <summary>
        /// The calculated orthographic size.
        /// </summary>
        public static float OrthographicSize { get; private set; } = DEFAULT_ORTHOGRAPHIC_SIZE;
    
        /// <summary>
        /// The number of rows in the layout grid.
        /// </summary>
        public static int Row { get; private set; }
    
        /// <summary>
        /// The number of columns in the layout grid.
        /// </summary>
        public static int Column { get; private set; }
    
        /// <summary>
        /// The padding applied to the layout.
        /// </summary>
        public static Padding Padding { get; private set; }
    
        /// <summary>
        /// The origin point of the layout.
        /// </summary>
        public static float2 Origin { get; private set; }
        
        /// <summary>
        /// The size of each unit in the layout grid.
        /// </summary>
        public static float2 UnitSize { get; private set; }
        
        /// <summary>
        /// Initializes the layout with the specified row count, aspect ratio, padding, and unit size.
        /// Calculates the number of columns and orthographic size automatically.
        /// </summary>
        /// <param name="row">Number of rows in the layout.</param>
        /// <param name="aspect">Aspect ratio of the camera view.</param>
        /// <param name="padding">Padding to apply to the layout.</param>
        /// <param name="unitSize">Size of each unit in the layout grid.</param>
        public static void Initialize(int row, float aspect, Padding padding, float2 unitSize)
        {
            Aspect = aspect;
            Padding = padding;
            Row = row;
            UnitSize = unitSize;
    
            Column = (int)math.floor(Aspect * ((Row * UnitSize.y) + Padding.Top + Padding.Bottom));
            
            //Column = Mathf.FloorToInt(Aspect * ((Row * UnitSize.y) + Padding.Top + Padding.Bottom));
    
            OrthographicSize = ((Column * UnitSize.x) + (Padding.Left + Padding.Right)) / (Aspect * 2f);
    
            float factor = (OrthographicSize / DEFAULT_ORTHOGRAPHIC_SIZE);
    
            if (OrthographicSize * 2f - ((Padding.Top + Padding.Bottom) * factor) < (Row * UnitSize.y))
                OrthographicSize = ((DEFAULT_ORTHOGRAPHIC_SIZE * ((Row*UnitSize.y) / 2f)) / ((DEFAULT_ORTHOGRAPHIC_SIZE * 2f - (Padding.Top + Padding.Bottom)) / 2f));
    
            float additionalColumn = OrthographicSize * Aspect * 2f;
            additionalColumn -= ((Column*UnitSize.x) + (Padding.Left + Padding.Right));
            additionalColumn = (int)math.floor(additionalColumn);
            //additionalColumn = Mathf.FloorToInt(additionalColumn);
            Column += (int)additionalColumn;
    
            Origin = GetOrigin();
    
        }
    
        /// <summary>
        /// Initializes the layout with the specified row and column counts, aspect ratio, padding, and unit size.
        /// Calculates the orthographic size automatically.
        /// </summary>
        /// <param name="row">Number of rows in the layout.</param>
        /// <param name="column">Number of columns in the layout.</param>
        /// <param name="aspect">Aspect ratio of the camera view.</param>
        /// <param name="padding">Padding to apply to the layout.</param>
        /// <param name="unitSize">Size of each unit in the layout grid.</param>
        public static void Initialize(int row, int column, float aspect, Padding padding, float2 unitSize)
        {
            Aspect = aspect;
            Padding = padding;
            Row = row;
            Column = column;
            UnitSize = unitSize;
    
            OrthographicSize = ((Column * UnitSize.x) + (Padding.Left + Padding.Right)) / (Aspect * 2f);
            float factor = (OrthographicSize / DEFAULT_ORTHOGRAPHIC_SIZE);
    
            if (OrthographicSize * 2f - ((Padding.Top + Padding.Bottom) * factor) < (Row* UnitSize.y))
                OrthographicSize = ((DEFAULT_ORTHOGRAPHIC_SIZE * ((Row * UnitSize.y) / 2f)) / ((DEFAULT_ORTHOGRAPHIC_SIZE * 2f - (Padding.Top + Padding.Bottom)) / 2f));
    
            Origin = GetOrigin();
    
        }
    
        /// <summary>
        /// Calculates the origin point of the layout based on the current orthographic size and padding.
        /// </summary>
        /// <returns>The calculated origin as a float2.</returns>
        private static float2 GetOrigin()
        {
    
            float factor = (OrthographicSize / DEFAULT_ORTHOGRAPHIC_SIZE);
            float left = Padding.Left;
            float top = Padding.Top * factor;
            float bottom = Padding.Bottom * factor;
            float right = Padding.Right;
    
            return new float2((left - right) / 2f, (bottom - top) / 2);
        }
    }
}