using System;
using System.Collections;

public interface ITurnActor
{
    IEnumerator ExecuteTurnCoroutine(float stepInterval, Action<int, int> onMovesRemainingChanged);
}
