﻿using UnityEngine;

namespace RTS.Controls
{
    public class SelectedObject
    {
        private MonoBehaviour _monoBehaviour;
        
        private IMoveable _moveable;
        private IDamageable _damageable;
        private IAttackable _attackable;
        private ISelectable _selectable;

        private bool _isInit;
        public bool IsInit => _isInit;

        public void InitObject(MonoBehaviour monoBehaviourObj)
        {
            UninitObject();
            
            _monoBehaviour = monoBehaviourObj;
            _selectable = _monoBehaviour.GetComponent<ISelectable>();

            if (_selectable == null || !_selectable.CanSelect())
            {
                UninitObject();
                return;
            }

            _damageable = _monoBehaviour.GetComponent<IDamageable>();
            _moveable = _monoBehaviour.GetComponent<IMoveable>();
            _attackable = _monoBehaviour.GetComponent<IAttackable>();

            if (_damageable != null)
            {
                // If enemy, don't select.
                if (!_damageable.IsFriend)
                {
                    UninitObject();
                    return;
                }
            }

            _selectable.Select();
            
            _isInit = true;
        }

        private void UninitObject()
        {
            if (_isInit)
                _selectable.Unselect();
            
            _monoBehaviour = null;
            _damageable = null;
            _moveable = null;
            _attackable = null;
            _isInit = false;
        }

        public bool SameObject(MonoBehaviour monoBehaviourObj)
        {
            return _monoBehaviour == monoBehaviourObj;
        }

        public void TryMoveToPos(Vector3 position)
        {
            if (!_isInit) return;

            _moveable?.MoveToPositon(position);
        }

        public bool TryInteractWithObject(MonoBehaviour monoBehaviourObj)
        {
            if (!_isInit) return false;
            
            var damageable = monoBehaviourObj.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (!damageable.IsFriend && damageable.CanBeDamaged())
                {
                    _attackable.AttackTarget(monoBehaviourObj);
                    return true;
                }
            }

            return false;
        }
    }
}