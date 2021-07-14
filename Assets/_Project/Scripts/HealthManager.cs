using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public class HealthManager : MonoBehaviour
    {
        public delegate void EmptyAction();
        public delegate void HealthChangedAction(int oldValue, int currentValue);
        public event HealthChangedAction OnHealthSet;
        public event HealthChangedAction OnHealed;
        public event HealthChangedAction OnHurt;
        public event EmptyAction OnHealthDepleted;

        public int Health { get { return health; } }

        [SerializeField] protected int health;

        public virtual void SetHealth(int value)
        {
            int oldValue = health;
            health = value;
            OnHealthSet?.Invoke(oldValue, value);
            if(health <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }

        public virtual void Heal(int value)
        {
            health += value;
            OnHealed?.Invoke(health-value, health);
            if (health <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }

        public virtual void Hurt(int value)
        {
            health -= value;
            OnHurt?.Invoke(health+value, health);
            if (health <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }
    }
}