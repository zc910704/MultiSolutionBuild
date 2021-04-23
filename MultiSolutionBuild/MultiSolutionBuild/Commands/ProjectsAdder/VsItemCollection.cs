using System;
using System.Collections;
using System.Collections.Generic;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class VsItemCollection : IList<IVsSolutionItem>
    {
        private readonly VsDirectoryItem _Directory;
        private readonly List<IVsSolutionItem> _FileSystemItems = new List<IVsSolutionItem>();

        public VsItemCollection(VsDirectoryItem directory)
        {
            _Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public void Add(IVsSolutionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.Parent = _Directory;
            _FileSystemItems.Add(item);
        }

        public void Clear()
        {
            _FileSystemItems.Clear();
        }

        public bool Contains(IVsSolutionItem item)
        {
            return _FileSystemItems.Contains(item);
        }

        public void CopyTo(IVsSolutionItem[] array, int arrayIndex)
        {
            _FileSystemItems.CopyTo(array, arrayIndex);
        }

        public int Count => _FileSystemItems.Count;
        public bool IsReadOnly => false;

        public bool Remove(IVsSolutionItem item)
        {
            return _FileSystemItems.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _FileSystemItems.GetEnumerator();
        }

        public IEnumerator<IVsSolutionItem> GetEnumerator()
        {
            return _FileSystemItems.GetEnumerator();
        }

        public int IndexOf(IVsSolutionItem item)
        {
            return _FileSystemItems.IndexOf(item);
        }

        public void Insert(int index, IVsSolutionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.Parent = _Directory;
            _FileSystemItems.Insert(index, item);
        }

        public IVsSolutionItem this[int index]
        {
            get => _FileSystemItems[index];
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                value.Parent = _Directory;
                _FileSystemItems[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            _FileSystemItems.RemoveAt(index);
        }
    }
}
