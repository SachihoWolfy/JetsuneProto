public interface IBonusScript
{
    void AttachToBullet(Bullet bullet); // Called when the script is added to a bullet.
    void Trigger(); // Called to activate the behavior on the bullet.
}
