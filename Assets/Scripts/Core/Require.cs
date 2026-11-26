using System.Collections.Generic;
using UnityEngine;

namespace Mossbound
{
    public static class Require
    {
        public static bool Assigned(Object value, string field, Object owner)
        {
            if (value != null)
                return true;

            Fail(owner, $"'{field}' is not assigned.");
            return false;
        }

        public static bool NotEmpty<T>(IReadOnlyCollection<T> values, string field, Object owner)
        {
            if (values != null && values.Count > 0)
                return true;

            Fail(owner, $"'{field}' is empty.");
            return false;
        }

        public static bool InRange(int index, int count, string field, Object owner)
        {
            if (index >= 0 && index < count)
                return true;

            Fail(owner, $"'{field}' = {index} is out of range (0..{count - 1}).");
            return false;
        }

        private static void Fail(Object owner, string problem)
        {
            Debug.LogError($"{owner.GetType().Name} on '{owner.name}': {problem}", owner);
        }
    }
}
