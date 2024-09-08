using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool _hitSomething = false;

    private const int _crossbowSpeed = 25;
    private const float _archerSpeed = 3f;
    private const float _arrowSmoothness = 10.0f; //The smoothness of the arrow (lerp) for archers

    private TroopModel _attacker = null;
    private TroopModel _target = null;

    //The point that controls the curvature
    private GameObject _controlPoint = null;

    //How far along the curve
    private float _t = 0.0f;

    public void StartStraightArrow(Vector3 targetLoc, TroopModel attacker, TroopModel target)
    {
        //Set attacker and target
        _attacker = attacker;
        _target = target;

        //Start moving the arrow
        StartCoroutine(MoveArrowStraight(targetLoc));
    }

    private IEnumerator MoveArrowStraight(Vector3 targetLoc)
    {
        //Direction vector to target
        Vector3 toTarget = targetLoc - transform.position;
        toTarget.Normalize();

        //Make sure the arrow doesn't fall
        toTarget.y = 0;

        while(!_hitSomething)
        {
            //Update transform
            transform.position += toTarget * _crossbowSpeed * Time.deltaTime;

            yield return null;
        }
    }

    public void StartCurveArrow(Vector3 targetLoc, TroopModel attacker, TroopModel target)
    {
        //Set attacker and target
        _attacker = attacker;
        _target = target;

        //Middle point
        Vector3 middlePos = new Vector3((attacker.transform.position.x + target.transform.position.x) / 2f, 0, (attacker.transform.position.z + target.transform.position.z) / 2f);

        //The height of the control point
        const int heightControlPoint = 6;

        //Set the controlPoint
        _controlPoint = new GameObject();
        _controlPoint.transform.position = new Vector3(middlePos.x, heightControlPoint, middlePos.z);

        //Start moving the arrow
        StartCoroutine(MoveArrowAlongCurve(targetLoc));
    }

    private IEnumerator MoveArrowAlongCurve(Vector3 targetLoc)
    {
        while(_t <= 1f)
        {
            //Arrow speed
            float arrowSpeed = Mathf.Lerp(_archerSpeed, 1f, _t);

            //Update the position along curve
            _t += Time.deltaTime * arrowSpeed;

            //The 3 points that generates the bezier
            Vector3 p0 = transform.position;
            Vector3 p1 = _controlPoint.transform.position;
            Vector3 p2 = targetLoc;

            //Calculate it's current location on the curve
            Vector3 positionOnCurve = Mathf.Pow(1 - _t, 2) * p0 + 2 * (1 - _t) * _t * p1 + Mathf.Pow(_t, 2) * p2;

            //Update it's location
            transform.position = positionOnCurve;

            //Lerp
            transform.position = Vector3.Lerp(transform.position, positionOnCurve, Time.deltaTime * _arrowSmoothness);

            const int minAngle = 45;
            const int maxAngle = 135;

            //Rotation based on how far along the curve it is
            float angle = Remap(_t, 0, 1, minAngle, maxAngle);

            //Lerp this angle based on _t
            angle = Mathf.Lerp(minAngle, maxAngle, _t);

            //Rotate it
            transform.localRotation = Quaternion.Euler(angle, 0, 0);

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var parent = gameObject.transform.parent.gameObject;

        //Check if we didn't hit ourself
        if (collision.gameObject != parent)
        {
            //Hit something
            _hitSomething = true;

            //Rotate back to default rotation
            parent.GetComponent<TroopView>().RotateBack();

            //Check if we hit a troop
            var troop = collision.gameObject.GetComponent<TroopModel>();
            if (troop)
            {
                //Deal the damage
                _attacker.GetComponent<TroopController>().DealDamage(_attacker, _target);

                //Destroy the arrow
                Destroy(gameObject);
            }
        }
    }

    private static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        // Clamp the value to the source range
        value = Mathf.Clamp(value, fromMin, fromMax);

        // Calculate the normalized position of the value in the source range
        float normalizedValue = (value - fromMin) / (fromMax - fromMin);

        // Remap the normalized value to the target range
        return Mathf.Lerp(toMin, toMax, normalizedValue);
    }
}
