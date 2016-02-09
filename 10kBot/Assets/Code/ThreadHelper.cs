using UnityEngine;
using System;
using System.Collections.Generic;

public class ThreadHelper : MonoBehaviour {

    public static void RunOnUnityThread(Action thingToRun)
    {
        Instance.ThingsToRun.Enqueue(thingToRun);
    }

    void Awake()
    {
        Instance = this;
        ThingsToRun = new Queue<Action>();
    }

    void Update()
    {
        if (ThingsToRun == null) return;
        while (ThingsToRun.Count > 1)
        {
            Action toRun = ThingsToRun.Dequeue();
            if (toRun != null) toRun();
        }
    }

    Queue<Action> ThingsToRun;
    private static ThreadHelper Instance;
}
