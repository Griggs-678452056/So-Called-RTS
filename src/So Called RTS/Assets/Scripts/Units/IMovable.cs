using UnityEngine;

namespace Scripts.Units
{
    public interface IMovable
    {
        void MoveTo(Vector3 position);
        void Stop();
    }
}