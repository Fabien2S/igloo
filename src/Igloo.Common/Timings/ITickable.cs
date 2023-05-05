namespace Igloo.Timings;

public interface ITickable
{
    /// <summary>
    ///     Executes a tick with a specified delta time.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last tick.</param>
    void Tick(in TimeSpan deltaTime);
}