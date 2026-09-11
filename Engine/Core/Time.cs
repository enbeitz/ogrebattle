// Engine/Core/Time.cs
namespace OgreBattle.Engine.Core
{
    public class Time
    {
        public float CurrentTime { get; private set; }

        public void Tick(float delta)
        {
            CurrentTime += delta;
        }

        public void Reset()
        {
            CurrentTime = 0f;
        }
    }
}
