using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StationToolbox : Page
{
    public StationToolboxButton SelectedButton = null;

    public IEnumerable<StationToolboxButton> Buttons
    { get { return GetComponentsInChildren<StationToolboxButton>(); } }

    Grid Grid { get { return GetComponentInChildren<Grid>(); } }

    void Start()
    {
        Window.MinimumSize = Vector2Int.zero;
    }

    void Update()
    {
        float content_width = Grid.ColumnCount * 
                              (Mathf.Abs(Grid.Stride.x) + Grid.Margin) + 
                              Grid.Margin;

        float content_height = Grid.RowCount * 
                               (Mathf.Abs(Grid.Stride.y) + Grid.Margin) + 
                               Grid.Margin;

        int edge_thickness = The.Style.StandardEdgeThickness;

        Window.natural_size =
            new Vector2Int(edge_thickness * 2,
                           edge_thickness + Window.DefaultTitleBarHeight) +
            new Vector2(content_width, content_height).Round();
    }
}
