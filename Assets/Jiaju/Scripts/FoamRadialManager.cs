 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamRadialManager : MonoBehaviour
{
    public Sprite m_centerSprite;
    public SpriteRenderer m_centerText;

    [HideInInspector]
    public bool IsToolOption;

    [HideInInspector]
    public Sprite m_inUseSprite;

    [HideInInspector]
    public SpriteRenderer m_iconChild;

    private SpriteRenderer _BGRenderer;
    private bool _isHightlighted = false;

    private Sprite _iconOGSprite = null;

    // Start is called before the first frame update
    void Start()
    {
        _BGRenderer = GetComponent<SpriteRenderer>();
        _BGRenderer.color = FoamUtils.RadialIconBGNormalColor;

        if (IsToolOption && m_iconChild)
        {
            _iconOGSprite = m_iconChild.sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HightlightIcon()
    {
        if (!_isHightlighted)
        {
            _BGRenderer.color = FoamUtils.RadialIconBGHighlightColor;
            m_centerText.sprite = m_centerSprite;
            _isHightlighted = true;
        }
    }

    public void DeHighlightIcon()
    {
        if (_isHightlighted)
        {
            _BGRenderer.color = FoamUtils.RadialIconBGNormalColor;
            _isHightlighted = false;
        }
    }

    public void SetIconInUse()
    {
        if (IsToolOption && m_iconChild && m_inUseSprite)
        {
            m_iconChild.sprite = m_inUseSprite;
        }
    }

    public void SetIconOG()
    {
        if (_iconOGSprite && m_iconChild)
        {
            m_iconChild.sprite = _iconOGSprite;
        }
    }
}
