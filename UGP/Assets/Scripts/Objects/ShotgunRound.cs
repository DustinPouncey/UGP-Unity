﻿using UnityEngine;

namespace UGP
{
    [CreateAssetMenu(fileName = "ShotgunRound", menuName = "AmmoType/Shotgun", order = 1)]
    public class ShotgunRound : Ammo, IShootable
    {
    }
}