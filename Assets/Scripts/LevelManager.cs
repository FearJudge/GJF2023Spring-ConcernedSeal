using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A class that handles loading of stages.
 * Was also intended to keep custom lists for a potential level editor implementation,
 * but ran out of time.
 */
public static class LevelManager
{
    const string LEVELPREFIX = "Stage";
    const string LEVELSEPERATOR = "_";
    const int LEVELNUMBEROFDIGITS = 2;
    public const string CUSTOMLEVELSCENE = "CustomStage";

    public static int tries = 0;
    public static bool isGoingThroughCustomData = false;
    public static int currentLevelNum = 0;
    public static List<string> customLevelDataList = new List<string>();
    public static float waterTemperature = 30f;

    public static string GetNextStandardLevelName()
    {
        currentLevelNum++;
        return string.Format("{0}{1}{2}", LEVELPREFIX, LEVELSEPERATOR, currentLevelNum.ToString(string.Format("D{0}", LEVELNUMBEROFDIGITS)));
    }

    // UNUSED
    public static string GetNextCustomLevelData()
    {
        currentLevelNum++;
        return customLevelDataList[currentLevelNum];
    }

    // UNUSED
    public static bool NextCustomLevelExists()
    {
        return (currentLevelNum + 1 <= customLevelDataList.Count);
    }

    public static void ResetAllLevelData()
    {
        currentLevelNum = 0;
        customLevelDataList.Clear();
    }
}
