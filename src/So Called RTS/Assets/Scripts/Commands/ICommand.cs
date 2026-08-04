using Scripts.Units;
using UnityEngine;

namespace Scripts.Commands
{
    public interface ICommand
    {
        bool CanHandle(AbstractCommandable commandable, RaycastHit hit);
        void Handle(AbstractCommandable commandable, RaycastHit hit);
    }
}
