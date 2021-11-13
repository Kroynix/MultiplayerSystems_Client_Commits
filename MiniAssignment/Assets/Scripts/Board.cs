using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{


   [Header ("Input Settings : ")]
   [SerializeField] private LayerMask boxesLayerMask ;
   [SerializeField] private float touchRadius ;

   [Header ("Mark Sprites : ")]
   [SerializeField] private Sprite spriteX ;
   [SerializeField] private Sprite spriteO ;

    public Mark[] marks ;



   private Camera cam;
   private Mark currentMark;
   private bool canPlay;
   //private LineRenderer lineRenderer;
   private int marksCount = 0 ;


    // Finding Box by Index
    public int Index;
    private Box[] boxes;
    private Box targetBox;

    // Start is called before the first frame update
    void Start()
    {
        boxes = FindObjectsOfType<Box>();

      cam = Camera.main;

      //lineRenderer = GetComponent<LineRenderer> ();
      //lineRenderer.enabled = false;

      currentMark = Mark.X;

      marks = new Mark[9];

      canPlay = true;

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            HitBox(FindBox(Index));
        //Debug.Log(targetBox.name);

        if (canPlay && Input.GetMouseButtonUp (0)) {
            Vector2 touchPosition = cam.ScreenToWorldPoint (Input.mousePosition);

        Collider2D hit = Physics2D.OverlapCircle (touchPosition, touchRadius, boxesLayerMask);


        if(canPlay)
            if (hit) 
                FindObjectOfType<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.SendMoveToServer + "," + hit.GetComponent<Box>().index);


        //Debug.Log(hit.GetComponent<Box>().index);
        // Use this if we we're trying to set it, internally here.
        // But for this we are gonna send which BoxIndex we clicked on to the server 
        // Send it back to both players and Find the box by the Index then call
        // HitBox
        //HitBox (hit.GetComponent <Box>());
      }

    }

    void HitBox (Box box) 
    {
        if(!box.isMarked)
        {
            marks[box.index] = currentMark;

            box.SetMarked(GetSprite(), currentMark);
            // Check for winner
            bool won = CheckIfWin();

            if(won)
            {
                Debug.Log(currentMark.ToString() + "Wins.");
                canPlay = false;
                return;
            }
            SwitchPlayer();
        }

    }

    private void SwitchPlayer()
    {
        currentMark = (currentMark == Mark.X) ? Mark.O : Mark.X;
    }

    private bool CheckIfWin () 
    {
        return
        AreBoxesMatched (0, 1, 2) || AreBoxesMatched (3, 4, 5) || AreBoxesMatched (6, 7, 8) ||
        AreBoxesMatched (0, 3, 6) || AreBoxesMatched (1, 4, 7) || AreBoxesMatched (2, 5, 8) ||
        AreBoxesMatched (0, 4, 8) || AreBoxesMatched (2, 4, 6);
    }


    private bool AreBoxesMatched(int i, int j, int k)
    {
        Mark m = currentMark;
        bool match = (marks[i] == m && marks[j] == m && marks[k] == m);
        return match;
    }

    private Sprite GetSprite()
    {
        return(currentMark == Mark.X) ? spriteX : spriteO;
    }


    Box FindBox(int _Index)
    {
        if (_Index != null)
        {
            foreach(Box box in boxes)
            {
                if (box.index == _Index)
                {
                    Debug.Log(box.name);
                    return box;
                }
            }
        }
        return null;
    }

    
}
