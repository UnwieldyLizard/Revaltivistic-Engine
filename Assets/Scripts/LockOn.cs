using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOn : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    private RectTransform markerTransform;
    private Image markerImage;
    [SerializeField] private Color32 lockedOn;
    [SerializeField] private Color32 notLockedOn;

    void Awake() {
        markerTransform = marker.GetComponent<RectTransform>();
        markerImage = marker.GetComponent<Image>();
    }

    public void SetMarkerPosition(Vector2 position) {
        markerTransform.position = position;
    }

    public void SetTracking() {
        markerImage.color = lockedOn;
    }

    public void SetNotTracking() {
        markerImage.color = notLockedOn;
    }
}
