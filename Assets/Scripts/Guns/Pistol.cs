﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Pistol : Gun
{
    // Start is called before the first frame update
    void Start()
    {
        var jsonString = File.ReadAllText("./Assets/Scripts/Weapons.json"); 
        var weaponList = JsonUtility.FromJson<Weapons>(jsonString);
        baseStats = weaponList.Pistol.stats;
        bulletDisplay = transform.GetChild(0).gameObject;
        reloadBar = transform.GetChild(1).gameObject;
        bulletObject = GameObject.Find("Bullet");
        ammoClone = GameObject.Find("Ammo");
        reloadTimer = -1;
        shooting = -1f;
        ammoQuantity = baseStats.ammoCapacity;
    }

}
