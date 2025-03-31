using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(EmoteBoard))]
public class EmoteBoardMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EmoteBoard emoteBoard = (EmoteBoard)target;

        if(GUILayout.Button("MoveForward"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.MoveForward));
        }
        if(GUILayout.Button("RotateRight"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.RotateRight));
        }
        if(GUILayout.Button("RotateLeft"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.RotateLeft));
        }
        if(GUILayout.Button("Jump"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.Jump));
        }
        if(GUILayout.Button("Success"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.Success));
        }
        if(GUILayout.Button("Failure"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.Failure));
        }
        if(GUILayout.Button("Happy"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.Happy));
        }
        if(GUILayout.Button("Sad"))
        {
            emoteBoard.StartCoroutine(emoteBoard.InsertEmote(EmoteBoard.Emotes.Sad));
        }
    }
}

#endif
public class EmoteBoard : MonoBehaviour
{
    public enum Emotes
    {
        MoveForward,
        RotateRight,
        RotateLeft,
        Jump,
        Success,
        Failure,
        Happy,
        Sad
    }

    public Dictionary<Emotes, Sprite> emoteSprites = new Dictionary<Emotes, Sprite>();
    public Queue<Sprite> board = new Queue<Sprite>();

    public float emoteLifeTime = 2.0f;

    public GameObject forwardArrow;

    public void Emote(Emotes em)
    {
        StartCoroutine(InsertEmote(em));
    }

    public IEnumerator InsertEmote(Emotes em)
    {
        board.Enqueue(emoteSprites[em]);
        UpdateBoard();
        if (em == Emotes.MoveForward){ forwardArrow.SetActive(true); }
        yield return new WaitForSeconds(emoteLifeTime);
        board.Dequeue();
        UpdateBoard();
        if (em == Emotes.MoveForward){ forwardArrow.SetActive(false); }
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform);   
    }

    void Start()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("StatusIcons/");
        emoteSprites.Add(Emotes.MoveForward, sprites[0]);
        emoteSprites.Add(Emotes.RotateLeft, sprites[1]);
        emoteSprites.Add(Emotes.RotateRight, sprites[2]);
        emoteSprites.Add(Emotes.Jump, sprites[3]);
        emoteSprites.Add(Emotes.Success, sprites[4]);
        emoteSprites.Add(Emotes.Failure, sprites[5]);
        emoteSprites.Add(Emotes.Happy, sprites[6]);
        emoteSprites.Add(Emotes.Sad, sprites[7]);
    }

    void UpdateBoard()
    {
        Debug.Log("EmoteBoard.UpdateBoard() called");
        if(transform.childCount > 0)
        {
            foreach(Transform child in transform)
            {
                Debug.Log($"EmoteBoard.UpdateBoard: destroying {child.gameObject.name}");
                Destroy(child.gameObject);
            }
        }

        Vector3 offset = new Vector3(0.0f, 0.3f, 0.0f);
        foreach(Sprite s in board)
        {
            Debug.Log($"EmoteBoard.UpdateBoard: instantiating {s}");
            GameObject emoteContainer = new GameObject(s.name);
            SpriteRenderer spriteRenderer = emoteContainer.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = s;
            emoteContainer.transform.parent = transform;
            emoteContainer.transform.position = transform.position + offset;
            emoteContainer.transform.rotation = transform.rotation;
            emoteContainer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            offset = offset + new Vector3(0.25f, 0.0f, 0.0f);
        }
    }
}
