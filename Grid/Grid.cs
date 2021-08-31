using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seoner
{

    [Serializable]
    public class Array2D<T>
    {
        public int x, y;

        /// <summary>2D array stored in 1D array.</summary>
        public T[] SingleArray;

        public T this[int x, int y]
        {
            get => SingleArray[y * this.x + x];
            set => SingleArray[y * this.x + x] = value;
        }

        public Array2D(int x, int y)
        {
            this.x = x;
            this.y = y;
            SingleArray = new T[x * y];
        }

        /// <summary>Gets the total number of elements in X dimension (1st dimension). </summary>
        public int Get_X_Length => x;

        /// <summary>Gets the total number of elements in Y dimension. (2nd dimension).</summary>
        public int Get_Y_Length => y;

        /// <summary>Gets the total number of elements all dimensions.</summary>
        public int Length => SingleArray.Length;

    }

    [System.Serializable]
    public class Grid<TGridObject>
    {
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        private Array2D<TGridObject> gridArray;
        //private TGridObject[,] gridArray;


        public int GetWidth()
        {
            return gridArray.Get_X_Length;
        }
        public int GetHeight()
        {
            return gridArray.Get_Y_Length;
        }
        public float GetCellSize()
        {
            return cellSize;
        }
        //public int GetWidth()
        //{
        //    return width;
        //}
        //public int GetHeight()
        //{
        //    return height;
        //}
        //public float GetCellsize()
        //{
        //    return cellSize;
        //}
        public Vector3 GetOriginPosition()
        {
            return originPosition;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>,int,int,TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            //gridArray = new TGridObject[width, height];
            gridArray = new Array2D<TGridObject>(width, height);

            for (int x = 0; x < gridArray.Get_X_Length; x++)
            {
                for (int y = 0; y < gridArray.Get_Y_Length; y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            //for (int x = 0; x < gridArray.GetLength(0); x++)
            //{
            //    for (int y = 0; y < gridArray.GetLength(1); y++)
            //    {
            //        gridArray[x, y] = createGridObject(this, x, y);
            //    }
            //}

            bool showDebug = true;

            if (showDebug)
            {
                for (int x = 0; x < gridArray.Get_X_Length; x++)
                {
                    for (int y = 0; y < gridArray.Get_Y_Length; y++)
                    {
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.red, 100f);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.red, 100f);
                    }
                    Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.red, 100f);
                    Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.red, 100f);
                }
            }
            //if(showDebug)
            //{
            //    for (int x = 0; x < gridArray.GetLength(0); x++)
            //    {
            //        for (int y = 0; y < gridArray.GetLength(1); y++)
            //        {
            //            Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.red, 100f);
            //            Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.red, 100f);
            //        }
            //        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.red, 100f);
            //        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.red, 100f);
            //    }
            //}
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }
        public Vector3 GetCellCenterWorldPosition(int x, int y)
        {
            return new Vector3(x, y) + new Vector3(cellSize, cellSize) * 0.5f + originPosition;
        }

        private void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }

        public void SetGridObject(int x,int y, TGridObject value)
        {
            if(x>= 0 && y>= 0 && x < width && y < height) {
                gridArray[x, y] = value;
                if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
            }
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            SetGridObject(x, y, value);
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetGridObject(x, y);
        }
    }
}

