using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int _rows = 8;

    [SerializeField]
    private int _columns = 8;

    [SerializeField]
    private float _cellSizeX = 1;

    [SerializeField]
    private float _cellSizeY = 1;

    [SerializeField]
    private GameObject _cellModel;
    void Start()
    {
        GenerateGrid();


    }

    private void GenerateGrid()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int column = 0; column < _columns; column++)
            {
                GameObject cell = Instantiate(_cellModel, transform);

                float posX = (float)column * _cellSizeX;
                float posY = (float)row * -_cellSizeY;

                cell.transform.position = new Vector3(posX, posY, 0);
            }
        }

        float gridW = _columns * _cellSizeX;
        float gridH = _rows * _cellSizeY;
        transform.position = new Vector3(-gridW * .5f + _cellSizeX * .5f, gridH * .5f + _cellSizeY * .5f, -0.5f);
    }

}
