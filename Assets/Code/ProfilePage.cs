using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ProfilePage : Page
{
    public RectTransform ProfilePictureContainer;

    public RectTransform DescriptionContainer;
    
    void Update()
    {
        ProfilePictureContainer.sizeDelta = 
            ProfilePictureContainer.sizeDelta.XChangedTo(
                DescriptionContainer.localPosition.x -
                ProfilePictureContainer.localPosition.x - 
                4);
    }
}
