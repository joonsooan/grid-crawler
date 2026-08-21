using System.Collections;

public interface ITurnActor
{
    IEnumerator ExecuteTurnCoroutine(float stepInterval);
}
