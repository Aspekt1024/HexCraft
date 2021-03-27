using System;
using System.Collections;
using UnityEngine;

namespace Aspekt.Hex.Util
{
    public static class CoroutineUtil
    {
        private static Coroutine ActionAfterDelay<T>(MonoBehaviour owner, Action<T> action, T param, float delay)
        {
            var context = owner.StartCoroutine(ActionAfterDelayRoutine(action, param, delay));
            return context;
        }
        
        private static IEnumerator ActionAfterDelayRoutine<T>(Action<T> action, T param, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke(param);
        }
    }
}