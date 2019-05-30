using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public RectTransform row;
    public RectTransform column;

    List<Column> columns;
    List<Row> rows;
    Dictionary<CellMap, Text> mapToText;

    public class CellMap : IEquatable<CellMap>
    {
        public int x;
        public int y;

        public bool Equals(CellMap other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            //NOTE(Brian): taken from https://stackoverflow.com/questions/682438/hash-function-providing-unique-uint-from-an-integer-coordinate-pair
            return (x * 0x1f1f1f1f) ^ y;
        }
    }

    public class Column
    {
        public Text text;
    }

    public class Row
    {
        public List<Column> tableColumns;
        public GameObject container;
    }

    CellMap tmpCellMap = new CellMap();

    public void SetCellText(int x, int y, string text)
    {
        tmpCellMap.x = x;
        tmpCellMap.y = y;

        if (!mapToText.ContainsKey(tmpCellMap))
        {
            return;
        }

        Text textComponent = mapToText[tmpCellMap];

        if (textComponent.text != text)
        {
            textComponent.text = text;
        }
    }

    public void PopulateTable(int width, int height)
    {
        rows = new List<Row>(height);
        columns = new List<Column>(width);
        mapToText = new Dictionary<CellMap, Text>();

        //NOTE(Brian): I'll reuse the same columns array reference for all the rows.
        for (int x = 0; x < width; x++)
        {
            columns.Add(new Column());
        }

        for (int y = 0; y < height; y++)
        {
            rows.Add(new Row() { tableColumns = columns });
            GameObject rowGameObject = Instantiate(row.gameObject, row.parent);
            rows[y].container = rowGameObject;

            for (int x = 0; x < width; x++)
            {
                GameObject columnGameObject = Instantiate(column.gameObject, rowGameObject.transform);
                Text textComponent = columnGameObject.GetComponent<Text>();
                columns[x].text = textComponent;
                textComponent.text = "";
                mapToText.Add(new CellMap() { x = x, y = y }, textComponent);

                if (x == 0)
                {
                    columnGameObject.GetComponent<LayoutElement>().preferredWidth = 240;
                    columnGameObject.GetComponent<LayoutElement>().minWidth = 240;
                }
                else
                {
                    columnGameObject.GetComponent<LayoutElement>().preferredWidth = 80;
                    columnGameObject.GetComponent<LayoutElement>().minWidth = 80;
                }
            }
        }

        //NOTE(Brian): I deactivate the base column/row objects used for instantiation.
        column.gameObject.SetActive(false);
        row.gameObject.SetActive(false);
    }
}
