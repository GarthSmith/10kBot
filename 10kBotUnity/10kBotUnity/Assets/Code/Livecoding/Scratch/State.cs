using UnityEngine;
using System.Collections;

/// <summary>
/// A current state of the livecoding xmpp authentication process.
/// </summary>
public abstract class State {
    public abstract void HandleReceivedData();
}
