using Scripts.Units;
using System.Collections;
using UnityEngine;

namespace Scripts.Commands
{
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        public abstract bool CanHandle(AbstractCommandable commandable, RaycastHit hit);
        public abstract void Handle(AbstractCommandable commandable, RaycastHit hit);
    }
}