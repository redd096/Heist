//this is like a List, with sort up and sort down functions when add or remove item
//this is used to optimize PathFinding calculations
//https://www.youtube.com/watch?v=3Dw5d7PlcTM&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=4

using System;

namespace redd096.PathFinding.AStar3D
{
    public class Heap<T> where T : IHeapItem3D<T>
    {
        T[] items;
        int currentItemCount;
        public int Count => currentItemCount;

        /// <summary>
        /// Constructor, set array size
        /// </summary>
        /// <param name="maxHeapSize"></param>
        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        #region public API

        /// <summary>
        /// Add in the array
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            //add in the array
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;

            //sort up, and upgrade current item count
            SortUp(item);
            currentItemCount++;
        }

        /// <summary>
        /// Remove first item in the array
        /// </summary>
        /// <returns></returns>
        public T RemoveFirst()
        {
            //remove first item in the array
            T firstItem = items[0];
            currentItemCount--;

            //set last item as first
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;

            //sort down, and return removed item
            SortDown(items[0]);
            return firstItem;
        }

        /// <summary>
        /// Sort Up item
        /// </summary>
        /// <param name="item"></param>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        /// <summary>
        /// Check if array contains this item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            //if the item with same heap index is in the array, check is equals
            return Equals(items[item.HeapIndex], item);
        }

        #endregion

        #region private API

        /// <summary>
        /// Swap items going down (childs) from this one
        /// </summary>
        /// <param name="item"></param>
        void SortDown(T item)
        {
            int childIndexLeft;
            int childIndexRight;
            int swapIndex;

            while (true)
            {
                //left one, right two
                childIndexLeft = item.HeapIndex * 2 + 1;
                childIndexRight = item.HeapIndex * 2 + 2;

                //if child left didn't reach Count, try to swap it
                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    //if child right is lower then left, swap right instead
                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    //if compare to swap is lower then 0, swap it
                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Swap items going up (parents) from this one
        /// </summary>
        /// <param name="item"></param>
        void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                //if compare to parent is greater then 0, swap it
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                //if swapped, set new parent
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        /// <summary>
        /// Swap items in the array
        /// </summary>
        /// <param name="itemA"></param>
        /// <param name="itemB"></param>
        void Swap(T itemA, T itemB)
        {
            //in the array, swap items
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            //swap heap index too
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }

        #endregion
    }

    public interface IHeapItem3D<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}