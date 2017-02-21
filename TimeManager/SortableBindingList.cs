using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace TimeManager
{
    /// <summary>Support a databound datagrid view column.
    ///     Taken from http://stackoverflow.com/questions/1699642/how-to-sort-databound-datagridview-column
    /// </summary>
    /// <typeparam name="T">The column type</typeparam>
    public class SortableBindingList<T> : BindingList<T>
    {
        #region constructors

        /// <summary>Constructor - Default</summary>
        public SortableBindingList() { }
        /// <summary>Constructor - List initializer</summary>
        /// <param name="list">List of items to initialize with</param>
        public SortableBindingList(IList<T> list)
        {
            foreach (object o in list)
            {
                this.Add((T)o);
            }
        }

        #endregion

        #region Searching

        /// <summary>indicate if this bindingList supports searching</summary>
        protected override bool SupportsSearchingCore { get { return true; } }
        /// <summary>Find core for the Binding List</summary>
        /// <param name="prop">The propery type to search</param>
        /// <param name="key">The value of the property to find</param>
        /// <returns></returns>
        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            PropertyInfo propInfo = typeof(T).GetProperty(prop.Name);
            T item;

            if (key != null)
            {
                for (int i = 0; i < Count; ++i)
                {
                    item = (T)Items[i];
                    if (propInfo.GetValue(item, null).Equals(key))
                        return i;
                }
            }
            return -1;
        }
        /// <summary>Find a property in the Binding list</summary>
        /// <param name="property">The name of the property</param>
        /// <param name="key">the value of the property</param>
        /// <returns></returns>
        public int Find(string property, object key)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            PropertyDescriptor prop = properties.Find(property, true);

            //return -1 if the property was not found
            if (prop == null)
                return -1;
            //return the find status of the core
            return FindCore(prop, key);
        }

        #endregion

        #region Sorting

        //private ArrayList sortedList;
        private ArrayList unsortedItems;

        //has the list been sorted
        private bool isSortedValue;
        //Holds the list sort direction
        ListSortDirection sortDirectionValue;
        //Holds the sort property value
        PropertyDescriptor sortPropertyValue;

        /// <summary>The sort property</summary>
        protected override PropertyDescriptor SortPropertyCore { get { return sortPropertyValue; } }
        /// <summary>The sort direction</summary>
        protected override ListSortDirection SortDirectionCore { get { return sortDirectionValue; } }

        /// <summary>indicate if this bindinglist supports sorting</summary>
        protected override bool SupportsSortingCore { get { return true; } }

        /// <summary>indicate if this binding list has been sorted</summary>
        protected override bool IsSortedCore { get { return isSortedValue; } }

        /// <summary>Sort based on the property and direction</summary>
        /// <param name="prop">The property to sort by</param>
        /// <param name="direction">The direction of the sort</param>
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            //sortedList = new ArrayList();

            Type interfaceType = prop.PropertyType.GetInterface("IComparable");

            if (interfaceType == null && prop.PropertyType.IsValueType)
            {
                Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

                if (underlyingType != null)
                {
                    interfaceType = underlyingType.GetInterface("IComparable");
                }
            }

            if (interfaceType != null)
            {
                sortPropertyValue = prop;
                sortDirectionValue = direction;

                IEnumerable<T> query = base.Items;
                if (direction == ListSortDirection.Ascending)
                {
                    query = query.OrderBy(i => prop.GetValue(i));
                }
                else
                {
                    query = query.OrderByDescending(i => prop.GetValue(i));
                }
                int newIndex = 0;
                foreach (object item in query)
                {
                    this.Items[newIndex] = (T)item;
                    newIndex++;
                }
                isSortedValue = true;
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

            }
            else
            {
                throw new NotSupportedException("Cannot sort by " + prop.Name +
                    ". This" + prop.PropertyType.ToString() +
                    " does not implement IComparable");
            }
        }
        /// <summary>Remove the sorting on the list</summary>
        protected override void RemoveSortCore()
        {
            int position;
            object temp;

            if (unsortedItems != null)
            {
                for (int i = 0; i < unsortedItems.Count; )
                {
                    position = this.Find("LastName",
                        unsortedItems[i].GetType().
                        GetProperty("LastName").GetValue(unsortedItems[i], null));
                    if (position > 0 && position != i)
                    {
                        temp = this[i];
                        this[i] = this[position];
                        this[position] = (T)temp;
                        i++;
                    }
                    else if (position == i)
                        i++;
                    else
                        unsortedItems.RemoveAt(i);
                }
                isSortedValue = false;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }
        /// <summary>Remove the sorting on the List</summary>
        public void RemoveSort() { RemoveSortCore(); }

        #endregion
    }
}
