using System;
using System.IO;

public class StartStreamState : State
{
    private StreamReader reader;
    private StreamWriter writer;

    public StartStreamState(StreamReader reader, StreamWriter writer)
    {
        this.reader = reader;
        this.writer = writer;
    }

    public override void HandleReceivedData()
    {
        throw new NotImplementedException();
    }
}
