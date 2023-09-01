using UnityEngine;
using System;
using System.Collections.Generic;

public class ThreadHelper : MonoBehaviour {

    public static void RunOnUnityThread(Action thingToRun)
    {
        Instance.ThingsToRun.Enqueue(thingToRun);
    }

    private void Awake()
    {
        Instance = this;
        ThingsToRun = new Queue<Action>();
    }

    private void Update()
    {
        if (ThingsToRun == null) return;
        while (ThingsToRun.Count > 1)
        {
            var toRun = ThingsToRun.Dequeue();
            if (toRun != null) toRun();
        }
    }

    private Queue<Action> ThingsToRun;
    private static ThreadHelper Instance;
}
