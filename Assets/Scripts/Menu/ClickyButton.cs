using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickyButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _img;
    [SerializeField] private Sprite _spriteClicked, _spriteDefault;
    [SerializeField] private AudioClip _clipPressedDown, _clipReleasedUp;
    [SerializeField] private AudioSource _audioSourceMenuSFX;


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Button {this.name} Clicked");
        //SerializeField a function so that we can select what specific thing we want to invoke when clicked. Or just use the onClick from the attached object to do so there.
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _img.sprite = _spriteClicked;
        _audioSourceMenuSFX.PlayOneShot(_clipPressedDown);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _img.sprite = _spriteDefault;
        _audioSourceMenuSFX.PlayOneShot(_clipReleasedUp);
    }
}
