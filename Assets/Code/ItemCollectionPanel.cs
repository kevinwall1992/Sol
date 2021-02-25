using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[ExecuteAlways]
public class ItemCollectionPanel : MonoBehaviour
{
    public IEnumerable<Item> Items { get; set; }

    public Grid Grid;
    public ScrollBar ScrollBar;
    public Switch ThumbnailSwitch;

    public int IconMargin, LineMargin;

    public Element SelectedElement;
    public Item SelectedItem
    {
        get { return SelectedElement == null ? null : SelectedElement.Item; }

        set
        {
            SelectedElement = 
                Elements.FirstOrDefault(element => element.Item == value);
        }
    }

    public IEnumerable<Element> Elements
    { get { return Grid.Elements.SelectComponents<Element>(); } }

    IEnumerable<ItemCollectionPanelThumbnail> Thumbnails
    { get { return Elements.SelectComponents<Element, ItemCollectionPanelThumbnail>(); } }

    IEnumerable<ItemCollectionPanelLine> Lines
    { get { return Elements.SelectComponents<Element, ItemCollectionPanelLine>(); } }

    bool IsDisplayingThumbnails
    {
        get
        {
            if (Thumbnails.Count() > 0)
                return true;
            else if (Lines.Count() > 0)
                return false;

            return ThumbnailSwitch.IsOn;
        }
    }

    IEnumerable<Item> ItemsListed
    {
        get
        {
            if (IsDisplayingThumbnails)
                return Thumbnails.Select(element => element.Item);
            else
                return Lines.Select(element => element.Item);
        }
    }

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        ClearElements();

        Grid.GetComparable = delegate(RectTransform transform)
        {
            Element element = transform.GetComponent<Element>();
            if (element.Item == null)
                return 0;

            return element.Item.Name;
        };
    }

    private void Update()
    {
        if (IsDisplayingThumbnails)
            Grid.Margin = IconMargin;
        else
            Grid.Margin = LineMargin;

        if (!Application.isPlaying)
            return;

        if(Items != null)
            ValidateElements(); 
    }

    void ClearElements()
    {
        foreach (ItemCollectionPanelThumbnail element in Thumbnails.ToList())
            GameObject.Destroy(element.gameObject);

        foreach (ItemCollectionPanelLine element in Lines.ToList())
            GameObject.Destroy(element.gameObject);
    }

    void ValidateElements()
    {
        if (IsDisplayingThumbnails != ThumbnailSwitch.IsOn)
            ClearElements();

        if (IsDisplayingThumbnails)
        {
            foreach (ItemCollectionPanelThumbnail element in Thumbnails.ToList())
                if (!Items.Contains(element.Item))
                    GameObject.Destroy(element.gameObject);
        }
        else
        {
            foreach (ItemCollectionPanelLine element in Lines.ToList())
                if (!Items.Contains(element.Item))
                    GameObject.Destroy(element.gameObject);
        }

        foreach (Item item in Items)
            if (!ItemsListed.Contains(item))
            {
                GameObject element;

                if (IsDisplayingThumbnails)
                {
                    ItemCollectionPanelThumbnail thumbnail_inventory_element =
                        GameObject.Instantiate(ThumbnailPrefab);
                    thumbnail_inventory_element.Item = item;
                    element = thumbnail_inventory_element.gameObject;
                }
                else
                {
                    ItemCollectionPanelLine line_inventory_element =
                        GameObject.Instantiate(LinePrefab);
                    line_inventory_element.Item = item;
                    element = line_inventory_element.gameObject;
                }

                element.transform.SetParent(Grid.transform, false);
            }
    }


    public class Element : UIElement
    {
        [HideInInspector]
        public Item Item;

        public bool IsSelected
        {
            get { return ItemPanel.SelectedElement == this; }

            set
            {
                if (value)
                    ItemPanel.SelectedElement = this;
                else if (IsSelected)
                    ItemPanel.SelectedElement = null;
            }
        }

        public Image SelectionOverlay;

        public ItemCollectionPanel ItemPanel
        { get { return GetComponentInParent<ItemCollectionPanel>(); } }

        protected virtual void Update()
        {
            if (WasClicked)
                IsSelected = true;

            SelectionOverlay.gameObject.SetActive(IsSelected);
        }
    }


    public ItemCollectionPanelThumbnail ThumbnailPrefab;
    public ItemCollectionPanelLine LinePrefab;
}
