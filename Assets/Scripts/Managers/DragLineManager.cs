using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragLineManager : MonoBehaviour
{
    private Vector3 _startPosition;
    private Vector3 _middlePosition;
    private Transform _endPosition;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private float _vertexCount = 12;
    [SerializeField]
    private float _middleYPosMax = 50;
    [SerializeField]
    private float _middleYPosMin = 10;

    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer.enabled = false;
    }

    public void SetStartAndEndPos(Vector3 startPos, Transform endPos)
    {
        _startPosition = startPos;
        _endPosition = endPos;

        if (_startPosition != null && _endPosition != null)
        {
            CalculateMiddlePosition();
            _lineRenderer.enabled = true;
        }
    }

    private void CalculateMiddlePosition()
    {
        if (_startPosition == _endPosition.position)
            return;

        _middlePosition = new Vector3((_startPosition.x + _endPosition.position.x) / 2, 
            _middleYPosMax - (((Vector3.Distance(_startPosition, _endPosition.position) / 43)) * (_middleYPosMax - _middleYPosMin)),
            (_startPosition.z + _endPosition.position.z) / 2);

        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += (1 / _vertexCount))
        {
            var tangent1 = Vector3.Lerp(_startPosition, _middlePosition, ratio);
            var tangent2 = Vector3.Lerp(_middlePosition, _endPosition.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        _lineRenderer.positionCount = pointList.Count;
        _lineRenderer.SetPositions(pointList.ToArray());

    }

    public void EnableLineRenderer()
    {
        _lineRenderer.enabled = true;   
    }

    public void DisableLineRenderer()
    {
        _lineRenderer.enabled = false;
    }


}
