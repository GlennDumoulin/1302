using System;
using UnityEngine;
using UnityEngine.UI;

public class UICrossbowHelper : MonoBehaviour
{
    private Camera _mainCamera = null;
    [Header("Objects")]
    [SerializeField]
    private Transform _troopObject = null;
    [SerializeField]
    private Transform _pivotObject = null;
    [SerializeField]
    private UITroopHPManager _hpManager = null;
    [SerializeField]
    private TroopController _crossbowTroop = null;
    [SerializeField]
    private TroopModel _crossbowModel = null;

    [Header("UI Images")]
    [SerializeField]
    private Image _crossbowUI;
    [SerializeField]
    private Sprite _reloadingSprite;
    [SerializeField]
    private Sprite _shieldSprite;
    [SerializeField]
    private Sprite _brokenShieldSprite;

    void Awake()
    {
        _mainCamera = FindObjectOfType<Camera>();
        _crossbowUI.enabled = false;
    }

    private void OnEnable()
    {
        _crossbowTroop.OnUpdateCrossbowUI += UpdateUI;
        _hpManager.OnUpdateUIDirection += LookAtCamera;
    }

    private void OnDisable()
    {
        _crossbowTroop.OnUpdateCrossbowUI -= UpdateUI;
        _hpManager.OnUpdateUIDirection -= LookAtCamera;
    }

    private void UpdateUI(object sender, IntEventArgs e)
    {

        switch (e.Num)
        {
            case 0:
                _crossbowUI.enabled = true;
                LookAtCamera(this, EventArgs.Empty);
                _crossbowUI.sprite = _reloadingSprite;
                break;
            case 1:
                _crossbowUI.enabled = true;
                LookAtCamera(this, EventArgs.Empty);
                if (!_crossbowModel.ShieldBroken)
                    _crossbowUI.sprite = _shieldSprite;
                else
                    _crossbowUI.sprite = _brokenShieldSprite;
                break;
            case 2:
                _crossbowUI.enabled = false;
                break;
        }
    }

    private void LookAtCamera(object sender, EventArgs e)
    {
        if (_troopObject == null)
            return;

        _pivotObject.localRotation = Quaternion.Inverse(_troopObject.rotation);

        if (_mainCamera == null)
            return;

        transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward, _mainCamera.transform.rotation * Vector3.up);
    }


}
