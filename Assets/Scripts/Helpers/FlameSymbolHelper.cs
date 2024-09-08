using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FlameSymbolHelper : MonoBehaviour
{
    private Vector3 _endPosition;
    private UICampfireCounter _uiCampfire = null;

    private void Awake()
    {
        _uiCampfire = FindObjectOfType<UICampfireCounter>();

    }

    private void Start()
    {
        RectTransform campfireImageRect = _uiCampfire.Position;

        //Convert the position of the UI element to a point in the Game World
        RectTransformUtility.ScreenPointToWorldPointInRectangle(campfireImageRect, campfireImageRect.position, Camera.main, out _endPosition);

        //Since we don't want the Flame Particle to go underneath the map, set the Y position higher
        _endPosition = new Vector3(_endPosition.x, transform.position.y, _endPosition.z);

        MoveToUI();
    }

    private void MoveToUI()
    {
        LeanTween.moveLocal(this.gameObject, _endPosition, 0.6f).setDelay(0.75f).setOnComplete(DestroySymbol).setEase(LeanTweenType.easeInQuart);
    }

    private void DestroySymbol()
    {
        _uiCampfire.EnlargeCampfire();
        Destroy(this.gameObject);
    }
}
