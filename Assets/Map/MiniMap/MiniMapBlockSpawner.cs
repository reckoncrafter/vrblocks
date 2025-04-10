using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class MiniMapBlockSpawner : MonoBehaviour
{
    public GameObject mapBlock;
    public GameObject goalObject;
    public TurtleMovement turtle;
    public MapBlockScriptableObject mapValues;
    public GameObject disableLeftHandModelOnGrab;
    public GameObject disableRightHandModelOnGrab;
    public Vector3 startPositionOffset;
    public Vector3 turtleUpdateOffset = new Vector3(-0.25f, -0.25f, -0.25f);
    public float miniMapScale = 0.1f;
    private Rigidbody minimapRB;
    private bool minimapAtRest = true;
    private Vector3 initPos;
    private PIDController minimapMagnetPIDx;
    private PIDController minimapMagnetPIDy;
    private PIDController minimapMagnetPIDz;

    void Start()
    {
        if (transform.childCount == 0)
        {
            SpawnEntities();
        }

        minimapRB = transform.GetComponent<Rigidbody>();
        initPos = transform.position;
        minimapMagnetPIDx = new PIDController();
        minimapMagnetPIDy = new PIDController();
        minimapMagnetPIDz = new PIDController();

        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(HideGrabbingHand);
        grabInteractable.selectExited.AddListener(ShowGrabbingHand);
        
        startPositionOffset = (mapValues.blockScale / 2) - CalculateCenterOfMass();
    }
    void Update()
    {
        // Return minimap to original position
        if ((initPos - transform.position).magnitude >= 0.08)
        {
            minimapRB.AddForce(
                new Vector3(
                    minimapMagnetPIDx.Update(Time.deltaTime, transform.position.x, initPos.x),
                    minimapMagnetPIDy.Update(Time.deltaTime, transform.position.y, initPos.y),
                    minimapMagnetPIDz.Update(Time.deltaTime, transform.position.z, initPos.z)
                ).normalized * (initPos - transform.position).sqrMagnitude,
                ForceMode.Force
            );
            minimapAtRest = false;
        }
        else if (!minimapAtRest)
        {
            minimapRB.velocity = Vector3.zero;
            minimapAtRest = true;
        }
    }

    private Vector3 CalculateCenterOfMass()
    {
        Vector3 com = new Vector3();
        Vector3 startPositionOffset = mapValues.blockScale / 2;
        foreach (Vector3 mapBlockTransform in mapValues.spawnPoints)
        {
            com += startPositionOffset + Vector3.Scale(mapBlockTransform, mapValues.blockScale);
        }
        com /= mapValues.spawnPoints.Length;
        return com;
    }

    public void SpawnEntities()
    {
        // Calculate COM and justify the minimap model centered here (and rotate in player's hand properly)
        Vector3 com = CalculateCenterOfMass();
        Vector3 startPositionOffset = (mapValues.blockScale / 2) - com;

        for (int i = 0; i < mapValues.spawnPoints.Length; i++)
        {
            Vector3 coords = startPositionOffset + Vector3.Scale(mapValues.spawnPoints[i], mapValues.blockScale);
            GameObject currentEntity = Instantiate(mapBlock, coords, Quaternion.identity);

            currentEntity.transform.SetParent(gameObject.transform, false);

            currentEntity.name = mapValues.blockPrefabName + i;
            currentEntity.transform.localScale = mapValues.blockScale;
            
            DestroyImmediate(currentEntity.GetComponent<Collider>());
        }

        // Goal Sphere
        Vector3 goalCoords = startPositionOffset + Vector3.Scale(mapValues.goalSpawnPoint, mapValues.blockScale);
        GameObject generatedGoalObject = Instantiate(goalObject, goalCoords, Quaternion.Euler(mapValues.goalRotation));
        generatedGoalObject.transform.SetParent(gameObject.transform, false);
        generatedGoalObject.transform.localScale = mapValues.goalScale;
        generatedGoalObject.transform.position += mapValues.goalPositionOffset * miniMapScale;
        generatedGoalObject.name = mapValues.goalPrefabName;

        DestroyImmediate(generatedGoalObject.GetComponent<Collider>());

        // Turtle
        Vector3 turtlePositionOffset = new Vector3(0, -0.2f, 0);
        Vector3 turtleCoords = startPositionOffset + turtlePositionOffset + Vector3.Scale(mapValues.turtleSpawnPoint, mapValues.blockScale);
        TurtleMovement turtleEntity = Instantiate(turtle, turtleCoords, Quaternion.Euler(0, mapValues.turtleRotation, 0));
        turtleEntity.transform.SetParent(gameObject.transform, false);
        turtleEntity.name = mapValues.turtlePrefabName;
        foreach (Component c in turtleEntity.GetComponents<Component>().ToList())
        {
            if (!typeof(Transform).IsAssignableFrom(c.GetType()))
            {
                DestroyImmediate(c);
            }
        }

        SphereCollider sCollider = transform.gameObject.AddComponent<SphereCollider>();
        sCollider.center = Vector3.zero;
        sCollider.radius = 1.0f;
    }
    public void HideGrabbingHand(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("Left Hand"))
        {
            disableLeftHandModelOnGrab.SetActive(false);
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            disableRightHandModelOnGrab.SetActive(false);
        }
    }
    public void ShowGrabbingHand(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("Left Hand"))
        {
            disableLeftHandModelOnGrab.SetActive(true);
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            disableRightHandModelOnGrab.SetActive(true);
        }
    }
    public void ClearMap()
    {
        // https://discussions.unity.com/t/why-does-foreach-work-only-1-2-of-a-time/91068/2
        // forgot you dont mutate the same list you're looping through
        foreach (Transform child in transform.Cast<Transform>().ToList())
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (SphereCollider sCollider in transform.gameObject.GetComponents<SphereCollider>().ToList())
        {
            DestroyImmediate(sCollider);
        }
    }
}
