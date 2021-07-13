using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ITakeHit
{
    void OnTakeHit(float damage);
}
interface IReceiveMeleeAttackInfo
{
    void OnTriggerEnterFromChildCollider(Collider other);
}
public class ChildCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponentInParent<IReceiveMeleeAttackInfo>().OnTriggerEnterFromChildCollider(other);
    }
}
