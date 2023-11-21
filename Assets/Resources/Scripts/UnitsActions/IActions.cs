using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActions
{
    void ReceiveDamage(int damage);

    void ReceiveHeal(int heal);
}
