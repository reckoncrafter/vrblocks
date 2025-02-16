using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class MiniMapBlockSpawner : MonoBehaviour
{
    public GameObject mapBlock;
    public GameObject goalSphere;
    public TurtleMovement turtle;
    public MapBlockScriptableObject mapValues;
    public GameObject disableLeftHandModelOnGrab;
    public GameObject disableRightHandModelOnGrab;

    private float miniMapScale = 0.1f;
    private Vector3 com;

    void Start()
    {
        if (transform.childCount == 0)
        {
            SpawnEntities();
        }

        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(HideGrabbingHand);
        grabInteractable.selectExited.AddListener(ShowGrabbingHand);
    }
    private void CalculateCenterOfMass()
    {
        Vector3 startPositionOffset = mapValues.blockScale / 2;
        foreach (Vector3 mapBlockTransform in mapValues.spawnPoints)
        {
            com += startPositionOffset + Vector3.Scale(mapBlockTransform, mapValues.blockScale);
        }
        com /= mapValues.spawnPoints.Length;
    }
    public void SpawnEntities()
    {
        CalculateCenterOfMass();
        Vector3 startPositionOffset = (mapValues.blockScale / 2) - com;

        GameObject currentEntity = null;
        for (int i = 0; i < mapValues.spawnPoints.Length; i++)
        {
            Vector3 coords = startPositionOffset + Vector3.Scale(mapValues.spawnPoints[i], mapValues.blockScale);
            currentEntity = Instantiate(mapBlock, coords, Quaternion.identity);

            currentEntity.transform.SetParent(gameObject.transform, false);

            currentEntity.name = mapValues.blockPrefabName + i;
            currentEntity.transform.localScale = mapValues.blockScale;
        }

        // Triggerable Goal
        Vector3 goalCoords = startPositionOffset + Vector3.Scale(mapValues.goalSpawnPoint, mapValues.blockScale);
        GameObject generatedGoalSphere = Instantiate(goalSphere, goalCoords, Quaternion.identity);
        generatedGoalSphere.transform.SetParent(gameObject.transform, false);
        generatedGoalSphere.transform.localScale = mapValues.goalScale;
        generatedGoalSphere.name = mapValues.goalPrefabName;
        //

        // Vector3 turtleCoords = startPositionOffset + Vector3.Scale(mapValues.turtleSpawnPoint, mapValues.blockScale);
        // TurtleMovement turtleEntity = Instantiate(turtle, turtleCoords, Quaternion.Euler(0, mapValues.turtleRotation, 0));

        // turtleEntity.transform.SetParent(gameObject.transform, false);

        // turtleEntity.name = mapValues.turtlePrefabName;

        // turtleEntity.movementDuration = mapValues.movementDuration;
        // turtleEntity.animationSpeed = mapValues.animationSpeed;


        // if (currentEntity != null)
        // {
        //     turtleEntity.moveDistance = Vector3.Scale(currentEntity.GetComponent<BoxCollider>().bounds.size, mapValues.blockScale);
        // }

        
        SphereCollider sCollider = transform.gameObject.AddComponent<SphereCollider>();
        sCollider.center = Vector3.zero;
        sCollider.radius = 1.0f;
    }
    public void HideGrabbingHand(SelectEnterEventArgs args)
    {
        Debug.Log(args.interactorObject.ToString());
        if(args.interactorObject.transform.tag == "Left Hand")
        {
            disableLeftHandModelOnGrab.SetActive(false);
        }
        else if(args.interactorObject.transform.tag == "Right Hand")
        {
            disableRightHandModelOnGrab.SetActive(false);
        }
    }
    public void ShowGrabbingHand(SelectExitEventArgs args)
    {
        if(args.interactorObject.transform.tag == "Left Hand")
        {
            disableLeftHandModelOnGrab.SetActive(true);
        }
        else if(args.interactorObject.transform.tag == "Right Hand")
        {
            disableRightHandModelOnGrab.SetActive(true);
        }
    }
    public void ClearMap()
    {
        // https://discussions.unity.com/t/why-does-foreach-work-only-1-2-of-a-time/91068/2
        foreach(Transform child in transform.Cast<Transform>().ToList())
        {
            DestroyImmediate(child.gameObject);
        }

        if(transform.gameObject.GetComponents<SphereCollider>().Length > 0){
            foreach(SphereCollider sCollider in transform.gameObject.GetComponents<SphereCollider>())
            DestroyImmediate(sCollider);
        }
    }
}
