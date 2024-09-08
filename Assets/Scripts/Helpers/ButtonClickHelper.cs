using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickHelper : MonoBehaviour
{
    public void PlaySFX()
    {
        if (AudioManager.instance == null)
            return;

        AudioManager.instance.PlayButtonClick();
    }

    public void PlayForwardSFX()
    {
        if (AudioManager.instance == null)
            return;

        AudioManager.instance.PlayButtonForwardClick();
    }

    public void PlayBattleSFX()
    {
        if (AudioManager.instance == null)
            return;

        AudioManager.instance.PlayButtonBattle();
    }
}
