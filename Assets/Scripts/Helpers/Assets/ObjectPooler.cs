using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Assets
{
    public class ObjectPooler<T> where T : Object
    {
        protected Stack<T> poolObjects = new Stack<T>();

        public virtual T GetObject()
        {
            return (poolObjects.Count == 0) ? null : poolObjects.Pop() as T;
        }

        public virtual void FreeObject(T obj)
        {
            poolObjects.Push(obj);
        }
    }
}