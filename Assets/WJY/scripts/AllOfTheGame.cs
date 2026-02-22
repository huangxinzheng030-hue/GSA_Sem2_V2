using System.Collections.Generic;
using UnityEngine;

public class AllOfTheGame : MonoBehaviour
{
    private static HashSet<string> unlocked = new HashSet<string>();

    public static void Unlock(string id)
    {
        unlocked.Add(id);
    }

    public static bool IsUnlocked(string id)
    {
        return unlocked.Contains(id);
    }
}
