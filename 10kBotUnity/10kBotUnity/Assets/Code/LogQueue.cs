using UnityEngine;
using System.Collections.Generic;

public class LogQueue : MonoBehaviour {

    public static void Log(string l)
    {
        if (Instance.Logs == null)
            Instance.Logs = new Queue<QueuedLog>();
        Instance.Logs.Enqueue(new QueuedLog(Type.Log, l));
    }

    public static void Warn(string w)
    {
        if (Instance.Logs == null)
            Instance.Logs = new Queue<QueuedLog>();
        Instance.Logs.Enqueue(new QueuedLog(Type.Warn, w));
    }

    public static void Error(string e)
    {
        if (Instance.Logs == null)
            Instance.Logs = new Queue<QueuedLog>();
        Instance.Logs.Enqueue(new QueuedLog(Type.Error, e));
    }


    void Awake()
    {
        Logs = new Queue<QueuedLog>();
        Instance = this;
    }

	void Update()
    {
        while (Logs.Count > 0) {
            QueuedLog next = Logs.Dequeue();
            switch (next.Type)
            {
                case Type.Error:
                    Debug.LogError(next.Message);
                    break;
                case Type.Warn:
                    Debug.LogWarning(next.Message);
                    break;
                case Type.Log:
                    Debug.Log(next.Message);
                    break;
            }
        }
    }

    Queue<QueuedLog> Logs;

    public enum Type
    {
        Log, Warn, Error,
    }

    static LogQueue Instance
    {
        get
        {
            if (MInstance == null)
                MInstance = FindObjectOfType<LogQueue>();
            return MInstance;
        }
        set { MInstance = value; }
    }
    static LogQueue MInstance;
}

public struct QueuedLog
{
    public QueuedLog(LogQueue.Type type, string message)
    {
        Type = type;
        Message = message;
    }

    public LogQueue.Type Type;
    public string Message;
}