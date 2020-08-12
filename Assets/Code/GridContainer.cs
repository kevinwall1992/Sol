using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class GridContainer : UIElement
{
    Vector2 AdjustedMargin
    {
        get
        {
            Vector2 adjusted_margin = Vector2.one * Margin;

            if (Justify)
            {
                if (IsVertical)
                    adjusted_margin.x = ((RectTransform.rect.width -
                                        Stride.x * ColumnCount) /
                                        (ColumnCount - 1))
                                        .RoundDown();
                else
                    adjusted_margin.y = ((RectTransform.rect.height -
                                        Stride.y * ColumnCount) /
                                        (ColumnCount - 1))
                                        .RoundDown();
            }

            return adjusted_margin;
        }
    }

    public Vector2 Stride;
    public float Margin;

    public bool IsVertical = true;
    public bool UseFirstElementAsStride = true;
    public bool Justify = true;

    public int ColumnCount
    {
        get
        {
            float row_length;
            float row_stride;
            if (IsVertical)
            {
                row_length = RectTransform.rect.width;
                row_stride = Mathf.Abs(Stride.x);
            }
            else
            {
                row_length = RectTransform.rect.height;
                row_stride = Mathf.Abs(Stride.y);
            }

            return 1 + ((row_length - row_stride) / 
                       (row_stride + (IsVertical ? Margin : Margin)))
                       .RoundDown();
        }
    }

    void Update()
    {
        RectTransform.pivot = RectTransform.pivot.Round();

        Stride.x = (RectTransform.pivot.x == 0 ? 1 : -1) * Mathf.Abs(Stride.x);
        Stride.y = (RectTransform.pivot.y == 0 ? 1 : -1) * Mathf.Abs(Stride.y);

        Margin = Mathf.Abs(Margin);

        if (transform.childCount == 0)
            return;


        if (UseFirstElementAsStride ||
            Stride.x == 0 || Stride.y == 0)
            Stride = (transform.GetChild(0).transform as RectTransform).rect.size;

        foreach (Transform child in transform)
        {
            int column = child.GetSiblingIndex() % ColumnCount;
            int row = child.GetSiblingIndex() / ColumnCount;

            Vector2Int stride_factor;
            if (IsVertical)
                stride_factor = new Vector2Int(column, row);
            else
                stride_factor = new Vector2Int(row, column);

            child.localPosition = 
                (Vector3)((Stride + Stride.Mapped(value => value < 0 ? -1 : 1) * AdjustedMargin) * stride_factor);
        }
    }
}
