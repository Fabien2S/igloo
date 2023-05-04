namespace Igloo.Timings;

public interface ITickable
{
    void Tick(in TimeSpan deltaTime);
}