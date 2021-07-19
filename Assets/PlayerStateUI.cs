using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : SingletonMonoBehavior<PlayerStateUI>
{
    public Image hpImage;
    internal void UpdateHp(float hp, float maxHp)
    {
        hpImage.fillAmount = hp / maxHp;
    }
}
