using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridView : MonoBehaviour
{
    [SerializeField] private GameObject _hexagon = null;
    [SerializeField] private GameObject _hexagonParent = null;

    private int _rows = 9;
    public int Rows
    {
        get { return _rows; }
    }

    private int _cols = 11;
    public int Cols
    {
        get { return _cols; }
    }

    private void Awake()
    {
        CreateGrid();
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void CreateGrid()
    {
        //Middle row to top row
        for (int row = 0; row < (_rows / 2) + 1; ++row)
        {
            //Even rows
            if (row % 2 == 0)
            {
                //Middle to right
                for (int col = 0; col < (_cols / 2) + 1; ++col)
                {
                    int doubleX = 2 * col; //Double coordinates

                    //Create the hexagon
                    CreateHexagon(new Vector2(doubleX, row));
                }

                //Left from middle to the left
                for (int col = -1; col > -(_cols / 2) - 1; --col)
                {
                    int doubleX = 2 * col;

                    //Create the hexagon
                    CreateHexagon(new Vector2(doubleX, row));
                }
            }

            //Odd rows
            else
            {
                //Middle to right
                for (int col = 0; col < _cols / 2; ++col)
                {
                    int doubleX = col * 2;
                    ++doubleX;

                    //Create the hexagon
                    CreateHexagon(new Vector2(doubleX, row));
                }

                //Left from middle to the left
                {
                    for (int col = 0; col > -(_cols / 2); --col)
                    {
                        int doubleX = col * 2;
                        --doubleX;

                        //Create the hexagon
                        CreateHexagon(new Vector2(doubleX, row));
                    }
                }
            }

        }

        //Under middle row to bot row
        for (int row = -1; row > -((_rows / 2) + 1); --row)
        {
            //Even rows
            if (row % 2 == 0)
            {
                //Middle to right
                for (int col = 0; col < (_cols / 2) + 1; ++col)
                {
                    int doubleX = 2 * col; //Double coordinates

                    //Create the hexagon
                    CreateHexagon(new Vector2(doubleX, row));
                }

                //Left from middle to the left
                {
                    for (int col = -1; col > -(_cols / 2) - 1; --col)
                    {
                        int doubleX = 2 * col;

                        //Create the hexagon
                        CreateHexagon(new Vector2(doubleX, row));

                    }
                }
            }

            //Odd rows
            else
            {
                //Middle to right
                for (int col = 0; col < _cols / 2; ++col)
                {
                    int doubleX = col * 2;
                    ++doubleX;

                    //Create the hexagon
                    CreateHexagon(new Vector2(doubleX, row));
                }

                //Left from middle to the left
                {
                    for (int col = 0; col > -(_cols / 2); --col)
                    {
                        int doubleX = col * 2;
                        --doubleX;
                        //Create the hexagon
                        CreateHexagon(new Vector2(doubleX, row));
                    }
                }
            }

        }
    }

    private void CreateHexagon(Vector2 doubleCoordinates)
    {
        //Instantiate the hexagon
        GameObject newHexa = Instantiate(_hexagon);

        //Set the hexagon to the right parent
        newHexa.transform.parent = _hexagonParent.transform;

        //Get the hexagonmodel
        var hexagonModel = newHexa.GetComponent<HexagonModel>();

        //Get the hexagondimension
        var hexDimensions = hexagonModel.CalculateWidthDepth(newHexa.transform.localScale.x);

        //Set the coordinates to the hexagonmodel
        hexagonModel.DoubleCoordinates = doubleCoordinates;
        hexagonModel.Qrs = CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates);

        //Set the hexagon position
        newHexa.transform.position = CoordinatesHelper.DoubleCoordinatesToWorld(new Vector2(doubleCoordinates.x, doubleCoordinates.y), hexDimensions);
    }
}
