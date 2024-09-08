using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraRectHelper : MonoBehaviour
{
    private Camera _mainCamera = null;
    
    //Reference Camera Positions Positions
    private Vector3 _firstReferencePos = new Vector3(0f, 34f, -21f); // Camera position for 25:10 aspect ratio
    private Vector3 _secondReferencePos = new Vector3(0, 33f, -21.5f); //Camera position for 16:9 aspect ratio
    private Vector3 _thirdReferencePos = new Vector3(0f, 35f, -23f); //Camera position for 5:3 aspect ratio
    private Vector3 _fourthReferencePos = new Vector3(0f, 43f, -25f); // Camera position for 4:3 aspect ratio

    //Reference Camera FOVs
    private float _firstReferenceFOV = 40f; // FOV for 25:10 aspect ratio
    private float _secondReferenceFOV = 43f; // FOV for 16:9 aspect ratio
    private float _thirdReferenceFOV = 44f; // FOV for 5:3 aspect ratio
    private float _fourthReferenceFOV = 45f; // FOV for 4:3 aspect ratio

    //Reference Aspect Ratios
    private float _firstReferenceAspect = 25f / 10f; // Main Aspect Ratio: 25:10
    private float _secondReferenceAspect = 16f / 9f; // Next Aspect Ratio: 16:10
    private float _thirdReferenceAspect = 5f / 3f; //Next Aspect Ratio: 5:3
    private float _fourthReferenceAspect = 4f / 3f; // Next Aspect Ratio: 4:3

    private void Awake()
    {
        _mainCamera = Camera.main;
        AdjustCameraParameters();
    }

    private void AdjustCameraParameters()
    {
        float currentAspect = (float) Screen.width / Screen.height;

        if (currentAspect >= _secondReferenceAspect)
        {
            //Interpolate camera position and FOV based on the 25:10 and 16:9 reference pos and FOV
            float t = Mathf.InverseLerp(_firstReferenceAspect, _secondReferenceAspect, currentAspect);
            Vector3 newPosition = Vector3.Lerp(_firstReferencePos, _secondReferencePos, t);
            float newFOV = Mathf.Lerp(_firstReferenceFOV, _secondReferenceFOV, t);
            _mainCamera.transform.position = newPosition;
            _mainCamera.fieldOfView = newFOV; 
        } else if (currentAspect >= _thirdReferenceAspect)
        {
            //Interpolate camera position and FOV based on the 16:9 and 5:3 reference pos and FOV
            float t = Mathf.InverseLerp(_secondReferenceAspect, _thirdReferenceAspect, currentAspect);
            Vector3 newPosition = Vector3.Lerp(_secondReferencePos, _thirdReferencePos, t);
            float newFOV = Mathf.Lerp(_secondReferenceFOV, _thirdReferenceFOV, t);
            _mainCamera.transform.position = newPosition;
            _mainCamera.fieldOfView = newFOV;

        } else if (currentAspect >= _fourthReferenceAspect)
        {
            //Interpolate camera position and FOV based on the 5:3 and 4:3 reference pos and FOV
            float t = Mathf.InverseLerp(_thirdReferenceAspect, _fourthReferenceAspect, currentAspect);
            Vector3 newPosition = Vector3.Lerp(_thirdReferencePos, _fourthReferencePos, t);
            float newFOV = Mathf.Lerp(_thirdReferenceFOV, _fourthReferenceFOV, t);
            _mainCamera.transform.position = newPosition;
            _mainCamera.fieldOfView = newFOV;
        } else
        {
            //Use the camera position and FOV for 4:3 aspect ratio if it is lower than 4:3
            _mainCamera.transform.position = _fourthReferencePos;
            _mainCamera.fieldOfView = _fourthReferenceFOV;
        }
    }

    public Vector3 GetChangeInCameraPosition()
    {

        //If the Aspect Ratio is 4:3 or less, then add a displacement to the Deck
        if (((float) Screen.width /  Screen.height) <= _fourthReferenceAspect)
            return _firstReferencePos - _mainCamera.transform.position - new Vector3(0f, -5f, 0);

        return _firstReferencePos - _mainCamera.transform.position;
    }
} 
