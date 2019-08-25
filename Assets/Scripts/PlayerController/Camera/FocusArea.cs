using UnityEngine;

public struct FocusArea
{
    public Vector2 Center { get; set; }
    public Vector2 MovedThisFrame { get; set; }

    private float _left,
        _right,
        _top,
        _bottom;

    public FocusArea(Bounds targetBounds, Vector2 size)
    {
        _left = targetBounds.center.x - size.x / 2;
        _right = targetBounds.center.x + size.x / 2;
        _bottom = targetBounds.min.y;
        _top = targetBounds.min.y + size.y;

        MovedThisFrame = Vector2.zero;
        
        Center = new Vector2((_left + _right)/2, (_top + _bottom)/2);
    }

    public void Update(Bounds targetBounds)
    {
        float shiftX = 0f;
        if (targetBounds.min.x < _left)
        {
            shiftX = targetBounds.min.x - _left;
        }else if(targetBounds.max.x > _right)
        {
            shiftX = targetBounds.max.x - _right; 
        }

        _left += shiftX;
        _right += shiftX;

        float shiftY = 0f;
        if (targetBounds.min.y < _bottom)
        {
            shiftY = targetBounds.min.y - _bottom;
        }
        else if (targetBounds.max.y > _top)
        {
            shiftY = targetBounds.max.y - _top;
        }

        _bottom += shiftY;
        _top += shiftY;

        Center = new Vector2((_left + _right) / 2, (_top + _bottom) / 2);
        MovedThisFrame = new Vector2(shiftX, shiftY);
    }
}
