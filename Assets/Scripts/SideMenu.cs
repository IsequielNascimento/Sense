using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SideMenu : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform sideMenuRectTransform;
    private float height;
    private float startPositionY;
    private float startingAnchoredPositionY;

    public enum Side { top, bottom}
    public Side side;

    // Start is called before the first frame update
    void Start() {
        height = Screen.height;
    }

    public void OnDrag(PointerEventData eventData) {
        sideMenuRectTransform.anchoredPosition = new Vector2(Mathf.Clamp(startingAnchoredPositionY - (startPositionY - eventData.position.y), GetMinPosition(), GetMaxPosition()), 0);
    }

    public void OnPointerDown(PointerEventData eventData) {
        StopAllCoroutines();
        startPositionY = eventData.position.y;
        startingAnchoredPositionY = sideMenuRectTransform.anchoredPosition.y;
    }

    public void OnPointerUp(PointerEventData eventData) {
        StartCoroutine(HandleMenuSlide(.25f, sideMenuRectTransform.anchoredPosition.y, isAfterHalfPoint() ? GetMinPosition() : GetMaxPosition()));
    }

    private bool isAfterHalfPoint() {
        if (side == Side.bottom)
            return sideMenuRectTransform.anchoredPosition.y < height;
        else
            return sideMenuRectTransform.anchoredPosition.y < 0;
    }

    private float GetMinPosition() {
        if(side == Side.bottom)
            return height / 2;
        return -height * .4f;
    }

    private float GetMaxPosition() {
        if(side == Side.bottom)
            return height * 1.4f;
        return height / 2;
    }

    private IEnumerator HandleMenuSlide(float slideTime, float startingY, float targetY) {
        for (float i = 0; i <= slideTime; i+= .025f) {
            sideMenuRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startingY, targetY, i / slideTime), 0);
            yield return new WaitForSecondsRealtime(.025f);
        }

    }

}