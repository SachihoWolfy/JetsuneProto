using UnityEngine;

public abstract class BonusBehavior : ScriptableObject, IBonusScript
{
    public abstract void AttachToBullet(Bullet bullet);
    public abstract void Trigger();
}

