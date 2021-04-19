using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public RectTransform row;
    public RectTransform column;
    public float updateInterval = 0.5f;

    List<Column> columns;
    List<Row> rows;
    Dictionary<(int, int), Text> mapToText;
    readonly Dictionary<(int, int), string> updateQueue = new Dictionary<(int, int), string>();

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

    private void OnEnable()
    {
        StartCoroutine(UpdatePanel());
    }

    private void OnDisable()
    {
        StopCoroutine(UpdatePanel());
    }

    public void SetCellText(int x, int y, string text, bool instantUpdate = false)
    {
        if (instantUpdate)
        {
            SetCellText_Internal(x, y, text);
        }
        else
        {
            updateQueue[(x, y)] = text;
        }
    }

    private void SetCellText_Internal(int x, int y, string text)
    {
        tmpCellMap.x = x;
        tmpCellMap.y = y;

        if (!DictionaryContainsColAndRow(mapToText, x, y))
        {
            return;
        }

        Text textComponent = mapToText[(x, y)];

        if (textComponent.text != text)
        {
            textComponent.text = text;
        }
    }

    public void PopulateTable(int width, int height, float firstColWidth = 240, float secondColWidth = 80)
    {
        rows = new List<Row>(height);
        columns = new List<Column>(width);
        mapToText = new Dictionary<(int, int), Text>();

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
                mapToText.Add((x, y), textComponent);

                if (x == 0)
                {
                    columnGameObject.GetComponent<LayoutElement>().preferredWidth = firstColWidth;
                    columnGameObject.GetComponent<LayoutElement>().minWidth = firstColWidth;
                }
                else
                {
                    columnGameObject.GetComponent<LayoutElement>().preferredWidth = secondColWidth;
                    columnGameObject.GetComponent<LayoutElement>().minWidth = secondColWidth;
                }
            }
        }

        //NOTE(Brian): I deactivate the base column/row objects used for instantiation.
        column.gameObject.SetActive(false);
        row.gameObject.SetActive(false);
    }

    private bool DictionaryContainsColumn(Dictionary<(int, int), Text> dictionary, int col)
    {
        return dictionary.Any(x => x.Key.Item1 == col);
    }

    private bool DictionaryContainsColAndRow(Dictionary<(int, int), Text> dictionary, int col, int row)
    {
        //It's faster to check Col again than using DictionaryContainsColumn method
        return dictionary.Any(x => x.Key.Item1 == col && x.Key.Item2 == row);
    }

    private IEnumerator UpdatePanel()
    {
        while (true)
        {
            foreach (var keyValuePair in updateQueue)
            {
                SetCellText_Internal(keyValuePair.Key.Item1, keyValuePair.Key.Item2, keyValuePair.Value);
            }

            updateQueue.Clear();

            yield return WaitForSecondsCache.Get(updateInterval);
        }
    }
}