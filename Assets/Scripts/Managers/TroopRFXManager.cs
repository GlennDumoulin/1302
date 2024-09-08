using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopRFXManager : MonoBehaviour
{
    [Header("Slash RFX")]
    [SerializeField]
    private GameObject _slashParent = null;
    [SerializeField]
    private ParticleSystem _slashParentRFX = null;


    public void PlaySlashRFX()
    {
        _slashParent.SetActive(true);

        _slashParentRFX.Play(true);

    }

}
