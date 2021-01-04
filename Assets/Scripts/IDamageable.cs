﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable {

    void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDir);

    void TakeDamage (float damage);

}