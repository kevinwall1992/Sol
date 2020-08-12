using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ProfilePage : Page
{
    public RectTransform ProfilePictureContainer;
    public Image ProfilePicture;

    public RectTransform SquareProfilePictureContainer, 
                         BannerProfilePictureContainer;
    public Image SquareProfilePicture,
                 BannerProfilePicture;

    public RectTransform DescriptionContainer;
    

    void Update()
    {
        RectTransform profile_picture_rect_transform =
            ProfilePicture.transform as RectTransform;
        RectTransform square_profile_picture_rect_transform = 
            SquareProfilePicture.transform as RectTransform;
        RectTransform banner_profile_picture_rect_transform =
            BannerProfilePicture.transform as RectTransform;
        
        if (SquareProfilePicture.sprite != ProfilePicture.sprite)
        {
            SquareProfilePicture.sprite = ProfilePicture.sprite;
            square_profile_picture_rect_transform.sizeDelta = 
                profile_picture_rect_transform.sizeDelta;

            BannerProfilePicture.sprite = ProfilePicture.sprite;
            banner_profile_picture_rect_transform.sizeDelta =
                profile_picture_rect_transform.sizeDelta;
        }


        ProfilePictureContainer.sizeDelta = 
            ProfilePictureContainer.sizeDelta.XChangedTo(
                DescriptionContainer.localPosition.x -
                ProfilePictureContainer.localPosition.x - 
                4);

        float banner_percentage = 
            (ProfilePictureContainer.sizeDelta.x - SquareProfilePictureContainer.sizeDelta.x) / 
            (BannerProfilePictureContainer.sizeDelta.x - SquareProfilePictureContainer.sizeDelta.x);

        profile_picture_rect_transform.anchoredPosition = 
            square_profile_picture_rect_transform.anchoredPosition.Lerped(
                banner_profile_picture_rect_transform.anchoredPosition, 
                banner_percentage);

        profile_picture_rect_transform.sizeDelta =
            square_profile_picture_rect_transform.sizeDelta.Lerped(
                banner_profile_picture_rect_transform.sizeDelta,
                banner_percentage);
    }
}
