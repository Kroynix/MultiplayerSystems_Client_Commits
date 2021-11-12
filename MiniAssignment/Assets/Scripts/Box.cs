using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public int index;
    public Mark mark;
    public bool isMarked;
    private SpriteRenderer spriteRend;


    void Awake()
    {
        spriteRend = GetComponent<SpriteRenderer>();

        index = transform.GetSiblingIndex();
        isMarked = false;
        mark = Mark.None;
    }


    public void SetMarked(Sprite sprite, Mark mark)
    {
        isMarked = true;
        this.mark = mark;

        spriteRend.sprite = sprite;

        GetComponent<CircleCollider2D>().enabled = false;
    }

}

public enum Mark {
    None,
    X,
    O
}
