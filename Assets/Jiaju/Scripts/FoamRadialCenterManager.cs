using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamRadialCenterManager : MonoBehaviour
{

    private SpriteRenderer _renderer;
    private Sprite _originalSprite;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = FoamUtils.IconNormalColor;
        _originalSprite = _renderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeHighlightCenter()
    {
        _renderer.sprite = _originalSprite;
    }
}
