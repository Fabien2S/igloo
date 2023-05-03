namespace Igloo.Common.Timings;

public interface ITickable
{
    void Tick(in TimeSpan deltaTime);
}