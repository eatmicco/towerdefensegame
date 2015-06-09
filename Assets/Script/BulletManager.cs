using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour {

    public MinionManager minionManager;
    public GameObject bulletPrefab;
    public float bulletSpeed;

    private List<GameObject> bullets_ = new List<GameObject>();

    public void Shoot(Vector3 towerPosition, Quaternion towerRotation, DamageType damageType,
        float damage)
    {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab);
        bullet.transform.position = towerPosition;
        bullet.transform.rotation = towerRotation;
        BulletProperties bulletProp = bullet.GetComponent<BulletProperties>();
        bulletProp.damageType = damageType;
        bulletProp.damage = damage;
        bullets_.Add(bullet);
    }

    private void UpdateBullets()
    {
        for (int i = 0; i < bullets_.Count; ++i)
        {
            Transform bulletTransform = bullets_[i].transform;
            bulletTransform.Translate(Vector3.up * (bulletSpeed * Time.deltaTime), Space.Self);

            //temporary check if bullet is out of screen
            if ((bulletTransform.position.x > 4f || bulletTransform.position.x < -4f) ||
                (bulletTransform.position.y > 5f || bulletTransform.position.y < -4f))
            {
                Destroy(bullets_[i]);
                bullets_.RemoveAt(i);
            }

            BulletProperties bulletProp = bullets_[i].GetComponent<BulletProperties>();

            if (minionManager.CheckMinionCollisionWithBullet(bulletTransform.position, bulletProp.damageType,
                bulletProp.damage))
            {
                Destroy(bullets_[i]);
                bullets_.RemoveAt(i);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameManager.instance.waveStarted)
        {
            return;
        }
        UpdateBullets();
	}
}
