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

    public Mark[] marks;
    private Camera cam;
    private Mark currentMark;
    public bool canPlay;
    //private LineRenderer lineRenderer;
    private int marksCount = 0 ;


    // Finding Box by Index
    public int Index;
    private Box[] boxes;
    private Box targetBox;

    // Game Objects
    GameObject xwin, owin;

    // Start is called before the first frame update
    void Start()
    {
        boxes = FindObjectsOfType<Box>();
        cam = Camera.main;

        //lineRenderer = GetComponent<LineRenderer> ();
        //lineRenderer.enabled = false;

        currentMark = Mark.O;
        marks = new Mark[9];
        canPlay = true;



        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();


        // Get Reference to all needed Game Objects
        foreach (GameObject go in allObjects)
        {
            // Game Objects - Gameplay
            if (go.name == "Xwin")
                xwin = go;
            else if (go.name == "Owin")
                owin = go;
        }



    }


    // Update is called once per frame
    void Update()
    {

        if (canPlay && Input.GetMouseButtonUp (0)) 
        {
            Vector2 touchPosition = cam.ScreenToWorldPoint (Input.mousePosition);

            Collider2D hit = Physics2D.OverlapCircle (touchPosition, touchRadius, boxesLayerMask);


            if(canPlay)
            {
                if (hit) 
                {
                    HitBox (hit.GetComponent <Box>(), Mark.O);
                    FindObjectOfType<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.Match + "," + GameSignifiers.SendMoveToServer + "," + hit.GetComponent<Box>().index);
                    Debug.Log(ClientToServerSignifiers.Match + "," + GameSignifiers.SendMoveToServer + "," + hit.GetComponent<Box>().index);
                    //canPlay = false;
                    
                }
            }
        }
    }

    public void HitBox (Box box, Mark mark) 
    {
        if(!box.isMarked)
        {
            marks[box.index] = mark;

            box.SetMarked(GetSprite(mark), mark);
            // Check for winner
            bool won = CheckIfWin(mark);

            if(won)
            {
                displayWinner(mark);
                canPlay = false;
                return;
            }
        }

    }



    private void displayWinner(Mark mark)
    {

        if(mark == Mark.X)
        {
            xwin.SetActive(true);
        }
        else
        {
            owin.SetActive(true);
        }
        
    }



    private bool CheckIfWin (Mark mark) 
    {
        return
        AreBoxesMatched (0, 1, 2, mark) || AreBoxesMatched (3, 4, 5, mark) || AreBoxesMatched (6, 7, 8, mark) ||
        AreBoxesMatched (0, 3, 6, mark) || AreBoxesMatched (1, 4, 7, mark) || AreBoxesMatched (2, 5, 8, mark) ||
        AreBoxesMatched (0, 4, 8, mark) || AreBoxesMatched (2, 4, 6, mark);
    }


    private bool AreBoxesMatched(int i, int j, int k, Mark mark)
    {
        Mark m = mark;
        bool match = (marks[i] == m && marks[j] == m && marks[k] == m);
        return match;
    }

    private Sprite GetSprite(Mark mark)
    {
        return(mark == Mark.X) ? spriteX : spriteO;
    }


    public Box FindBox(int _Index)
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
