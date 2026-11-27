using System;
using UnityEngine;

namespace Mossbound.Player
{
    public class PlayerGold : MonoBehaviour
    {
        public static int TotalGold { get; private set; }
        public static event Action<int> GoldChanged;

        public static void AddGold(int amount)
        {
            if (amount <= 0) return;

            TotalGold += amount;
            GoldChanged?.Invoke(TotalGold);
            Debug.Log($"Total Gold: {TotalGold}");
        }

        public static void ResetGold()
        {
            TotalGold = 0;
            GoldChanged?.Invoke(TotalGold);
        }
    }
}