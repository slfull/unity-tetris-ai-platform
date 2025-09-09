using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBGPiece : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        //透明化一點
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;

            float transparency = 0.3f;
            color.a = transparency;

            spriteRenderer.color = color;
        }
    }
    void Update()
    {
        //低於螢幕下就刪除
        float height = this.transform.position.y;
        if (height < -5.0f)
        {
            Destroy(gameObject);
        }
    }


}