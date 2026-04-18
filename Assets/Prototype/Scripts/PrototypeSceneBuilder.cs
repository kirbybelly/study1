using System.IO;
using UnityEngine;

public class PrototypeSceneBuilder : MonoBehaviour
{
    private const string CafeteriaWallDiffusePath = "Assets/Art/Environment/Textures/Walls/textures/beige_wall_001_diff_4k.jpg";
    private const string CafeteriaWallHeightPath = "Assets/Art/Environment/Textures/Walls/textures/beige_wall_001_disp_4k.png";
    private const string CafeteriaWallRoughnessPath = "Assets/Art/Environment/Textures/Walls/textures/beige_wall_001_rough_4k.jpg";

    private Material wallMaterial;
    private Material floorMaterial;
    private Material cafeteriaMaterial;
    private Material saunaMaterial;
    private Material restroomMaterial;
    private Material metalMaterial;
    private Material glassMaterial;
    private Material accentMaterial;
    private PhysicsMaterial bouncyMaterial;
    private GameObject root;

    private void Start()
    {
        CleanupLegacyObjects();
        SetupEnvironment();
        BuildScene();
    }

    private void CleanupLegacyObjects()
    {
        foreach (GameObject existing in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (existing == gameObject)
            {
                continue;
            }

            if (existing.transform.parent != null)
            {
                continue;
            }

            if (transform.IsChildOf(existing.transform))
            {
                continue;
            }

            if (existing.CompareTag("MainCamera"))
            {
                continue;
            }

            Destroy(existing);
        }
    }

    private void SetupEnvironment()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.76f, 0.78f, 0.82f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.74f, 0.78f, 0.82f);
        RenderSettings.fogStartDistance = 26f;
        RenderSettings.fogEndDistance = 70f;

        wallMaterial = CreateMaterial(new Color(0.84f, 0.84f, 0.8f));
        floorMaterial = CreateMaterial(new Color(0.36f, 0.36f, 0.38f));
        cafeteriaMaterial = CreateMaterial(new Color(0.76f, 0.69f, 0.5f));
        saunaMaterial = CreateMaterial(new Color(0.6f, 0.4f, 0.24f));
        restroomMaterial = CreateMaterial(new Color(0.82f, 0.86f, 0.88f));
        metalMaterial = CreateMaterial(new Color(0.7f, 0.73f, 0.76f));
        glassMaterial = CreateMaterial(new Color(0.62f, 0.86f, 0.94f), 0.45f);
        accentMaterial = CreateMaterial(new Color(0.79f, 0.28f, 0.23f));

        ApplyTextureSet(cafeteriaMaterial, CafeteriaWallDiffusePath, CafeteriaWallHeightPath, CafeteriaWallRoughnessPath, new Vector2(3f, 1.2f));

        bouncyMaterial = new PhysicsMaterial("PrototypeBouncy")
        {
            dynamicFriction = 0.24f,
            staticFriction = 0.18f,
            bounciness = 0.42f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        root = new GameObject("PrototypeSceneRoot");

        Transform playerRoot = transform.parent;
        if (playerRoot != null)
        {
            playerRoot.position = new Vector3(-6.2f, 0.05f, 0f);
            playerRoot.rotation = Quaternion.Euler(0f, 82f, 0f);
        }
        else
        {
            transform.position = new Vector3(-6.2f, 0.05f, 0f);
            transform.rotation = Quaternion.Euler(0f, 82f, 0f);
        }

        CreateDirectionalLight();
    }

    private void BuildScene()
    {
        CreateBaseFloor();
        BuildCafeteria();
        BuildSaunaCorridor();
        BuildRestroom();
        BuildNPCs();
    }

    private void CreateBaseFloor()
    {
        GameObject floor = CreatePrimitive("WorldFloor", PrimitiveType.Cube, floorMaterial, new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 32f), root.transform);
        floor.layer = LayerMask.NameToLayer("Default");
    }

    private void BuildCafeteria()
    {
        Transform cafeteria = CreateRoomShell("Cafeteria", new Vector3(0f, 0f, 0f), 18f, 4.2f, 12f, cafeteriaMaterial, new Color(0.82f, 0.8f, 0.74f));
        CreateOpeningOnXWall(cafeteria, "WallRight", 8.88f, 4.2f, 12f, 0f, 2.8f, cafeteriaMaterial);

        CreatePrimitive("ServingCounter", PrimitiveType.Cube, accentMaterial, new Vector3(-5.8f, 1.1f, 4.2f), new Vector3(4.8f, 2.2f, 1.1f), cafeteria);
        CreatePrimitive("CounterShelf", PrimitiveType.Cube, metalMaterial, new Vector3(-5.8f, 2.15f, 4.75f), new Vector3(4.8f, 0.16f, 0.4f), cafeteria);
        CreatePrimitive("MenuBoard", PrimitiveType.Cube, metalMaterial, new Vector3(-5.8f, 3f, 4.95f), new Vector3(4.6f, 0.7f, 0.08f), cafeteria);
        CreatePrimitive("WallTrimBack", PrimitiveType.Cube, metalMaterial, new Vector3(0f, 1f, -5.82f), new Vector3(17.6f, 0.06f, 0.08f), cafeteria);
        CreatePrimitive("WallTrimFront", PrimitiveType.Cube, metalMaterial, new Vector3(-1.2f, 1f, 5.82f), new Vector3(14.8f, 0.06f, 0.08f), cafeteria);
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Vector3 tablePosition = new Vector3(-1.5f + (col * 4.6f), 0.92f, -2.4f + (row * 4.2f));
                BuildLunchTable(cafeteria, tablePosition);
            }
        }

        for (int i = 0; i < 7; i++)
        {
            float offset = -7.2f + i * 2.3f;
            CreateCeilingLight(cafeteria, new Vector3(offset, 3.75f, 0f), new Vector3(1.5f, 0.12f, 0.5f));
        }

    }

    private void BuildSaunaCorridor()
    {
        Transform corridor = CreateRoomShell("SaunaCorridor", new Vector3(17f, 0f, 0f), 16f, 4f, 7f, saunaMaterial, new Color(0.49f, 0.31f, 0.2f));
        Transform turnCorridor = CreateRoomShell("SaunaTurnCorridor", new Vector3(23f, 0f, -9.5f), 7f, 4f, 12f, saunaMaterial, new Color(0.46f, 0.29f, 0.19f));

        CreateOpeningOnXWall(corridor, "WallLeft", -7.88f, 4f, 7f, 0f, 2.6f, saunaMaterial);
        CreateOpeningOnZWall(corridor, "WallBack", -3.5f, 4f, 16f, 5.8f, 3.2f, saunaMaterial);
        CreateOpeningOnZWall(turnCorridor, "WallFront", 6f, 4f, 7f, 0f, 3.2f, saunaMaterial);
        CreateOpeningOnZWall(turnCorridor, "WallBack", -6f, 4f, 7f, 0f, 2.8f, saunaMaterial);

        CreatePrimitive("BenchLeft", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(-1.5f, 0.45f, -2.2f), new Vector3(3f, 0.9f, 0.7f), corridor);
        CreatePrimitive("BenchRight", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(2.4f, 0.45f, 2.1f), new Vector3(3.2f, 0.9f, 0.7f), corridor);
        CreatePrimitive("TowelShelf", PrimitiveType.Cube, metalMaterial, new Vector3(1.6f, 2f, -2.9f), new Vector3(2.4f, 0.2f, 0.5f), corridor);
        CreatePrimitive("FrontDesk", PrimitiveType.Cube, accentMaterial, new Vector3(-4.7f, 1f, 2.5f), new Vector3(2f, 2f, 1f), corridor);
        CreatePrimitive("DeskTop", PrimitiveType.Cube, metalMaterial, new Vector3(-4.7f, 2.05f, 2.5f), new Vector3(2f, 0.12f, 1f), corridor);
        CreatePrimitive("FloorRunner", PrimitiveType.Cube, accentMaterial, new Vector3(2.8f, 0.03f, -0.4f), new Vector3(4f, 0.02f, 1.4f), corridor);

        for (int i = 0; i < 4; i++)
        {
            CreatePrimitive("Locker_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(4.2f, 1.2f, -2.7f + i * 1.6f), new Vector3(1.1f, 2.4f, 0.65f), corridor);
        }

        CreatePrimitive("TurnCornerBench", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(-1.4f, 0.45f, 4.4f), new Vector3(2f, 0.9f, 0.7f), turnCorridor);
        CreatePrimitive("TurnShelfA", PrimitiveType.Cube, metalMaterial, new Vector3(1.5f, 1.75f, 3.6f), new Vector3(1.6f, 0.18f, 0.4f), turnCorridor);
        CreatePrimitive("TurnShelfB", PrimitiveType.Cube, metalMaterial, new Vector3(1.5f, 1.75f, 1.2f), new Vector3(1.6f, 0.18f, 0.4f), turnCorridor);
        CreatePrimitive("TurnRunner", PrimitiveType.Cube, accentMaterial, new Vector3(0f, 0.03f, 0f), new Vector3(1.4f, 0.02f, 9.5f), turnCorridor);

        for (int i = 0; i < 3; i++)
        {
            CreateCeilingLight(corridor, new Vector3(-5.4f + i * 5.2f, 3.55f, 0f), new Vector3(1.4f, 0.12f, 0.45f));
        }

        for (int i = 0; i < 3; i++)
        {
            CreateCeilingLight(turnCorridor, new Vector3(0f, 3.55f, 4.2f - i * 4.2f), new Vector3(0.45f, 0.12f, 1.4f));
        }

    }

    private void BuildRestroom()
    {
        Transform restroom = CreateRoomShell("MensRestroom", new Vector3(23f, 0f, -19.5f), 12f, 4f, 10f, restroomMaterial, new Color(0.77f, 0.8f, 0.82f));
        CreateOpeningOnZWall(restroom, "WallFront", 5f, 4f, 12f, 0f, 2.8f, restroomMaterial);

        CreatePrimitive("MirrorStrip", PrimitiveType.Cube, glassMaterial, new Vector3(-1.8f, 2f, -4.7f), new Vector3(5.4f, 1.3f, 0.08f), restroom);
        CreatePrimitive("SinkCounter", PrimitiveType.Cube, metalMaterial, new Vector3(-1.8f, 0.92f, -4.25f), new Vector3(5.6f, 0.95f, 1.2f), restroom);
        CreatePrimitive("TileBand", PrimitiveType.Cube, metalMaterial, new Vector3(0f, 1.55f, -4.84f), new Vector3(11.6f, 0.06f, 0.06f), restroom);
        CreatePrimitive("EntryMat", PrimitiveType.Cube, accentMaterial, new Vector3(0f, 0.03f, 3.6f), new Vector3(1.6f, 0.02f, 2.2f), restroom);

        for (int i = 0; i < 3; i++)
        {
            float x = -4.1f + i * 2.2f;
            CreatePrimitive("Sink_" + i, PrimitiveType.Cylinder, wallMaterial, new Vector3(x, 1.2f, -4.18f), new Vector3(0.42f, 0.08f, 0.42f), restroom);
            CreatePrimitive("Faucet_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(x, 1.45f, -4.58f), new Vector3(0.16f, 0.24f, 0.16f), restroom);
        }

        for (int i = 0; i < 3; i++)
        {
            float z = -2.6f + i * 2.6f;
            CreatePrimitive("Urinal_" + i, PrimitiveType.Cube, wallMaterial, new Vector3(4.35f, 1.25f, z), new Vector3(0.7f, 1.35f, 0.48f), restroom);
            CreatePrimitive("DividerTop_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(3.72f, 1.35f, z + 0.95f), new Vector3(0.08f, 1.5f, 1.3f), restroom);
        }

        for (int i = 0; i < 2; i++)
        {
            float z = -1.7f + i * 3.8f;
            BuildToiletStall(restroom, new Vector3(-0.35f, 0f, z));
        }

        CreatePrimitive("TrashCan", PrimitiveType.Cylinder, accentMaterial, new Vector3(-5.1f, 0.48f, 3.4f), new Vector3(0.45f, 0.46f, 0.45f), restroom);
        CreatePrimitive("HandDryer", PrimitiveType.Cube, metalMaterial, new Vector3(-5.3f, 1.55f, -1.6f), new Vector3(0.55f, 0.55f, 0.24f), restroom);
        CreatePrimitive("SoapDispenser", PrimitiveType.Cube, accentMaterial, new Vector3(1f, 1.55f, -4.72f), new Vector3(0.22f, 0.38f, 0.12f), restroom);

        for (int i = 0; i < 3; i++)
        {
            CreateCeilingLight(restroom, new Vector3(-3.6f + i * 3.6f, 3.55f, 0.4f), new Vector3(1.2f, 0.12f, 0.45f));
        }
    }

    private void BuildNPCs()
    {
        BuildFriend(new Vector3(-2f, 0f, -2.8f), new Color(0.17f, 0.42f, 0.75f));
        BuildFriend(new Vector3(0.3f, 0f, -3.2f), new Color(0.76f, 0.35f, 0.22f));
        BuildFriend(new Vector3(2.4f, 0f, -2.5f), new Color(0.22f, 0.58f, 0.34f));
        BuildSaunaWorker(new Vector3(12.8f, 0f, 1.7f));
    }

    private void BuildLunchTable(Transform parent, Vector3 tablePosition)
    {
        CreatePrimitive("TableTop", PrimitiveType.Cube, wallMaterial, tablePosition, new Vector3(2.2f, 0.12f, 1.3f), parent);
        CreatePrimitive("TableLegA", PrimitiveType.Cube, metalMaterial, tablePosition + new Vector3(0.85f, -0.5f, 0.45f), new Vector3(0.12f, 1f, 0.12f), parent);
        CreatePrimitive("TableLegB", PrimitiveType.Cube, metalMaterial, tablePosition + new Vector3(-0.85f, -0.5f, 0.45f), new Vector3(0.12f, 1f, 0.12f), parent);
        CreatePrimitive("TableLegC", PrimitiveType.Cube, metalMaterial, tablePosition + new Vector3(0.85f, -0.5f, -0.45f), new Vector3(0.12f, 1f, 0.12f), parent);
        CreatePrimitive("TableLegD", PrimitiveType.Cube, metalMaterial, tablePosition + new Vector3(-0.85f, -0.5f, -0.45f), new Vector3(0.12f, 1f, 0.12f), parent);

        CreateLunchChair(parent, new Vector3(tablePosition.x, 0.1f, tablePosition.z - 1.05f));
        CreateLunchChair(parent, new Vector3(tablePosition.x, 0.1f, tablePosition.z + 1.05f));
        CreateLunchChair(parent, new Vector3(tablePosition.x - 1.25f, 0.1f, tablePosition.z + 0.2f), 90f);
        CreateLunchChair(parent, new Vector3(tablePosition.x + 1.25f, 0.1f, tablePosition.z - 0.2f), -90f);

        CreateTray(parent, tablePosition + new Vector3(-0.45f, 1.05f, 0.18f));
        CreateTray(parent, tablePosition + new Vector3(0.35f, 1.05f, -0.16f));
    }

    private void CreateLunchChair(Transform parent, Vector3 position, float rotationY = 0f)
    {
        Transform chair = new GameObject("LunchChair").transform;
        chair.SetParent(parent);
        chair.localPosition = position;
        chair.localRotation = Quaternion.Euler(0f, rotationY, 0f);

        Rigidbody body = chair.gameObject.AddComponent<Rigidbody>();
        ConfigureDynamicBody(body, 2.8f);

        CreatePrimitive("Seat", PrimitiveType.Cube, accentMaterial, Vector3.up * 0.46f, new Vector3(0.65f, 0.12f, 0.65f), chair);
        CreatePrimitive("Back", PrimitiveType.Cube, accentMaterial, new Vector3(0f, 0.95f, -0.26f), new Vector3(0.65f, 0.8f, 0.12f), chair);
        CreatePrimitive("Leg1", PrimitiveType.Cube, metalMaterial, new Vector3(0.23f, 0.2f, 0.23f), new Vector3(0.08f, 0.4f, 0.08f), chair);
        CreatePrimitive("Leg2", PrimitiveType.Cube, metalMaterial, new Vector3(-0.23f, 0.2f, 0.23f), new Vector3(0.08f, 0.4f, 0.08f), chair);
        CreatePrimitive("Leg3", PrimitiveType.Cube, metalMaterial, new Vector3(0.23f, 0.2f, -0.23f), new Vector3(0.08f, 0.4f, 0.08f), chair);
        CreatePrimitive("Leg4", PrimitiveType.Cube, metalMaterial, new Vector3(-0.23f, 0.2f, -0.23f), new Vector3(0.08f, 0.4f, 0.08f), chair);
        ApplyBouncyMaterial(chair);
    }

    private void CreateTray(Transform parent, Vector3 position)
    {
        Transform tray = new GameObject("LunchTray").transform;
        tray.SetParent(parent);
        tray.localPosition = position;
        tray.localRotation = Quaternion.identity;

        Rigidbody body = tray.gameObject.AddComponent<Rigidbody>();
        ConfigureDynamicBody(body, 1.05f);

        CreatePrimitive("TrayBase", PrimitiveType.Cube, metalMaterial, Vector3.zero, new Vector3(0.7f, 0.06f, 0.48f), tray);
        CreatePrimitive("Rice", PrimitiveType.Cylinder, wallMaterial, new Vector3(-0.18f, 0.09f, 0.04f), new Vector3(0.12f, 0.03f, 0.12f), tray);
        CreatePrimitive("Soup", PrimitiveType.Cylinder, accentMaterial, new Vector3(0.16f, 0.09f, -0.05f), new Vector3(0.12f, 0.05f, 0.12f), tray);
        CreatePrimitive("SideDish", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(0.02f, 0.09f, 0.13f), new Vector3(0.18f, 0.05f, 0.12f), tray);
        ApplyBouncyMaterial(tray);
    }

    private void BuildFriend(Vector3 position, Color shirtColor)
    {
        Transform friend = new GameObject("FriendNPC").transform;
        friend.SetParent(root.transform);
        friend.position = position;

        Material shirtMaterial = CreateMaterial(shirtColor);
        CreatePrimitive("LegL", PrimitiveType.Capsule, floorMaterial, new Vector3(-0.12f, 0.52f, 0f), new Vector3(0.22f, 0.5f, 0.22f), friend);
        CreatePrimitive("LegR", PrimitiveType.Capsule, floorMaterial, new Vector3(0.12f, 0.52f, 0f), new Vector3(0.22f, 0.5f, 0.22f), friend);
        CreatePrimitive("Body", PrimitiveType.Capsule, shirtMaterial, new Vector3(0f, 1.28f, 0f), new Vector3(0.5f, 0.62f, 0.34f), friend);
        CreatePrimitive("Head", PrimitiveType.Sphere, CreateMaterial(new Color(0.94f, 0.79f, 0.63f)), new Vector3(0f, 2.02f, 0f), new Vector3(0.42f, 0.42f, 0.42f), friend);
        CreatePrimitive("Hair", PrimitiveType.Cube, CreateMaterial(new Color(0.13f, 0.1f, 0.08f)), new Vector3(0f, 2.16f, -0.02f), new Vector3(0.46f, 0.16f, 0.46f), friend);
    }

    private void BuildSaunaWorker(Vector3 position)
    {
        Transform worker = new GameObject("SaunaWorker").transform;
        worker.SetParent(root.transform);
        worker.position = position;
        worker.rotation = Quaternion.Euler(0f, -90f, 0f);

        Material robe = CreateMaterial(new Color(0.92f, 0.91f, 0.87f));
        CreatePrimitive("LegL", PrimitiveType.Capsule, floorMaterial, new Vector3(-0.12f, 0.52f, 0f), new Vector3(0.22f, 0.5f, 0.22f), worker);
        CreatePrimitive("LegR", PrimitiveType.Capsule, floorMaterial, new Vector3(0.12f, 0.52f, 0f), new Vector3(0.22f, 0.5f, 0.22f), worker);
        CreatePrimitive("Body", PrimitiveType.Capsule, robe, new Vector3(0f, 1.25f, 0f), new Vector3(0.56f, 0.7f, 0.38f), worker);
        CreatePrimitive("Head", PrimitiveType.Sphere, CreateMaterial(new Color(0.94f, 0.79f, 0.63f)), new Vector3(0f, 2.02f, 0f), new Vector3(0.44f, 0.44f, 0.44f), worker);
        CreatePrimitive("Cap", PrimitiveType.Cylinder, accentMaterial, new Vector3(0f, 2.28f, 0f), new Vector3(0.32f, 0.08f, 0.32f), worker);
    }

    private void BuildToiletStall(Transform restroom, Vector3 localOrigin)
    {
        CreatePrimitive("StallSideA", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(0f, 1.4f, -0.9f), new Vector3(0.08f, 2.8f, 1.8f), restroom);
        CreatePrimitive("StallSideB", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(1.75f, 1.4f, -0.9f), new Vector3(0.08f, 2.8f, 1.8f), restroom);
        CreatePrimitive("StallDoor", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(0.88f, 1.4f, -1.75f), new Vector3(1.68f, 2.8f, 0.08f), restroom);
        CreatePrimitive("ToiletBowl", PrimitiveType.Cylinder, wallMaterial, localOrigin + new Vector3(0.9f, 0.58f, -0.45f), new Vector3(0.36f, 0.22f, 0.36f), restroom);
        CreatePrimitive("ToiletBack", PrimitiveType.Cube, wallMaterial, localOrigin + new Vector3(0.9f, 0.95f, -0.02f), new Vector3(0.55f, 0.66f, 0.24f), restroom);
        CreatePrimitive("FlushBox", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(0.9f, 1.55f, 0.05f), new Vector3(0.24f, 0.24f, 0.12f), restroom);
    }

    private Transform CreateRoomShell(string name, Vector3 center, float width, float height, float depth, Material wallMat, Color ceilingColor)
    {
        Transform room = new GameObject(name).transform;
        room.SetParent(root.transform);
        room.position = center;

        Material ceilingMat = CreateMaterial(ceilingColor);
        CreatePrimitive("Floor", PrimitiveType.Cube, floorMaterial, new Vector3(0f, 0.05f, 0f), new Vector3(width, 0.1f, depth), room);
        CreatePrimitive("Ceiling", PrimitiveType.Cube, ceilingMat, new Vector3(0f, height, 0f), new Vector3(width, 0.1f, depth), room);
        CreatePrimitive("WallLeft", PrimitiveType.Cube, wallMat, new Vector3(-(width * 0.5f), height * 0.5f, 0f), new Vector3(0.25f, height, depth), room);
        CreatePrimitive("WallRight", PrimitiveType.Cube, wallMat, new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.25f, height, depth), room);
        CreatePrimitive("WallBack", PrimitiveType.Cube, wallMat, new Vector3(0f, height * 0.5f, -(depth * 0.5f)), new Vector3(width, height, 0.25f), room);
        CreatePrimitive("WallFront", PrimitiveType.Cube, wallMat, new Vector3(0f, height * 0.5f, depth * 0.5f), new Vector3(width, height, 0.25f), room);

        return room;
    }

    private void CreateDoorFrame(Transform parent, Vector3 localPosition, Vector3 size, Material frameMaterial)
    {
        float sideWidth = Mathf.Max(0.2f, size.x);
        float openingWidth = size.z;
        float height = size.y;

        CreatePrimitive("DoorSideL", PrimitiveType.Cube, frameMaterial, localPosition + new Vector3(0f, 0f, -(openingWidth * 0.5f)), new Vector3(sideWidth, height, 0.32f), parent);
        CreatePrimitive("DoorSideR", PrimitiveType.Cube, frameMaterial, localPosition + new Vector3(0f, 0f, openingWidth * 0.5f), new Vector3(sideWidth, height, 0.32f), parent);
        CreatePrimitive("DoorTop", PrimitiveType.Cube, frameMaterial, localPosition + new Vector3(0f, height * 0.5f, 0f), new Vector3(sideWidth, 0.3f, openingWidth + 0.32f), parent);
    }

    private void CreateCeilingLight(Transform parent, Vector3 localPosition, Vector3 scale)
    {
        GameObject fixture = CreatePrimitive("CeilingLight", PrimitiveType.Cube, CreateMaterial(new Color(0.95f, 0.95f, 0.87f)), localPosition, scale, parent);
        GameObject lightObject = new GameObject("AreaLight");
        lightObject.transform.SetParent(fixture.transform);
        lightObject.transform.localPosition = Vector3.zero;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 8f;
        light.intensity = 4f;
        light.color = new Color(1f, 0.94f, 0.82f);
        light.shadows = LightShadows.None;
    }

    private void CreateSteamPanel(Transform parent, Vector3 localPosition, Vector3 scale)
    {
        CreatePrimitive("SteamGlass", PrimitiveType.Cube, glassMaterial, localPosition, scale, parent);

        for (int i = 0; i < 5; i++)
        {
            float offsetX = -1.1f + i * 0.55f;
            CreatePrimitive("SteamHint_" + i, PrimitiveType.Sphere, CreateMaterial(new Color(0.9f, 0.92f, 0.94f, 0.75f)), localPosition + new Vector3(offsetX, 0.35f * Mathf.Sin(i), 0.1f), new Vector3(0.26f, 0.2f, 0.05f), parent);
        }
    }

    private void CreateSign(Transform parent, string text, Vector3 localPosition, Color color, int fontSize = 32)
    {
        GameObject sign = new GameObject("Sign");
        sign.transform.SetParent(parent);
        sign.transform.localPosition = localPosition;
        sign.transform.localRotation = Quaternion.identity;

        TextMesh mesh = sign.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.characterSize = 0.08f;
        mesh.fontSize = fontSize;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
    }

    private void CreateDirectionalLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.SetParent(root.transform);
        lightObject.transform.rotation = Quaternion.Euler(50f, -32f, 0f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.4f;
        light.color = new Color(0.92f, 0.92f, 0.9f);
    }

    private void RemoveWall(Transform parent, string wallName)
    {
        Transform wall = parent.Find(wallName);
        if (wall != null)
        {
            Destroy(wall.gameObject);
        }
    }

    private void CreateOpeningOnXWall(Transform parent, string wallName, float wallX, float roomHeight, float roomDepth, float openingCenterZ, float openingWidth, Material wallMat)
    {
        RemoveWall(parent, wallName);

        float halfDepth = roomDepth * 0.5f;
        float lowerLength = Mathf.Max(0f, (openingCenterZ - (openingWidth * 0.5f)) + halfDepth);
        float upperLength = Mathf.Max(0f, halfDepth - (openingCenterZ + (openingWidth * 0.5f)));

        if (lowerLength > 0.01f)
        {
            float centerZ = -halfDepth + (lowerLength * 0.5f);
            CreatePrimitive(wallName + "_Lower", PrimitiveType.Cube, wallMat, new Vector3(wallX, roomHeight * 0.5f, centerZ), new Vector3(0.25f, roomHeight, lowerLength), parent);
        }

        if (upperLength > 0.01f)
        {
            float centerZ = openingCenterZ + (openingWidth * 0.5f) + (upperLength * 0.5f);
            CreatePrimitive(wallName + "_Upper", PrimitiveType.Cube, wallMat, new Vector3(wallX, roomHeight * 0.5f, centerZ), new Vector3(0.25f, roomHeight, upperLength), parent);
        }
    }

    private void CreateOpeningOnZWall(Transform parent, string wallName, float wallZ, float roomHeight, float roomWidth, float openingCenterX, float openingWidth, Material wallMat)
    {
        RemoveWall(parent, wallName);

        float halfWidth = roomWidth * 0.5f;
        float leftLength = Mathf.Max(0f, (openingCenterX - (openingWidth * 0.5f)) + halfWidth);
        float rightLength = Mathf.Max(0f, halfWidth - (openingCenterX + (openingWidth * 0.5f)));

        if (leftLength > 0.01f)
        {
            float centerX = -halfWidth + (leftLength * 0.5f);
            CreatePrimitive(wallName + "_Left", PrimitiveType.Cube, wallMat, new Vector3(centerX, roomHeight * 0.5f, wallZ), new Vector3(leftLength, roomHeight, 0.25f), parent);
        }

        if (rightLength > 0.01f)
        {
            float centerX = openingCenterX + (openingWidth * 0.5f) + (rightLength * 0.5f);
            CreatePrimitive(wallName + "_Right", PrimitiveType.Cube, wallMat, new Vector3(centerX, roomHeight * 0.5f, wallZ), new Vector3(rightLength, roomHeight, 0.25f), parent);
        }
    }

    private GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Material material, Vector3 localPosition, Vector3 localScale, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localRotation = Quaternion.identity;
        primitive.transform.localScale = localScale;
        primitive.GetComponent<Renderer>().material = material;
        return primitive;
    }

    private void ApplyBouncyMaterial(Transform rootTransform)
    {
        foreach (Collider collider in rootTransform.GetComponentsInChildren<Collider>())
        {
            collider.material = bouncyMaterial;
        }
    }

    private void ConfigureDynamicBody(Rigidbody body, float mass)
    {
        body.mass = mass;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearDamping = 0.15f;
        body.angularDamping = 0.05f;
        body.maxAngularVelocity = 45f;
    }

    private Material CreateMaterial(Color color, float alpha = 1f)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        color.a = alpha;
        material.color = color;

        if (alpha < 0.99f)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_AlphaClip", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return material;
    }

    private void ApplyTextureSet(Material material, string diffusePath, string heightPath, string roughnessPath, Vector2 tiling)
    {
        Texture2D diffuse = LoadTextureFromProjectPath(diffusePath, false);
        if (diffuse != null)
        {
            material.color = Color.white;
            material.mainTexture = diffuse;
            material.SetTexture("_BaseMap", diffuse);
            material.mainTextureScale = tiling;
            material.SetTextureScale("_BaseMap", tiling);
        }

        Texture2D height = LoadTextureFromProjectPath(heightPath, true);
        if (height != null)
        {
            material.SetTexture("_ParallaxMap", height);
            material.SetTextureScale("_ParallaxMap", tiling);
            material.SetFloat("_Parallax", 0.02f);
            material.EnableKeyword("_PARALLAXMAP");
        }

        Texture2D roughness = LoadTextureFromProjectPath(roughnessPath, true);
        if (roughness != null)
        {
            Texture2D metallicSmoothness = BuildSmoothnessTexture(roughness);
            material.SetTexture("_MetallicGlossMap", metallicSmoothness);
            material.SetTextureScale("_MetallicGlossMap", tiling);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 1f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
        }
    }

    private Texture2D LoadTextureFromProjectPath(string assetPath, bool linear)
    {
        string relativePath = assetPath.Replace("Assets/", string.Empty).Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, linear);
        if (!texture.LoadImage(bytes, false))
        {
            Destroy(texture);
            return null;
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private Texture2D BuildSmoothnessTexture(Texture2D roughnessTexture)
    {
        Texture2D smoothnessTexture = new Texture2D(roughnessTexture.width, roughnessTexture.height, TextureFormat.RGBA32, false, true);
        Color[] sourcePixels = roughnessTexture.GetPixels();
        Color[] convertedPixels = new Color[sourcePixels.Length];

        for (int i = 0; i < sourcePixels.Length; i++)
        {
            float smoothness = 1f - sourcePixels[i].grayscale;
            convertedPixels[i] = new Color(0f, 0f, 0f, smoothness);
        }

        smoothnessTexture.SetPixels(convertedPixels);
        smoothnessTexture.Apply();
        smoothnessTexture.wrapMode = TextureWrapMode.Repeat;
        smoothnessTexture.filterMode = FilterMode.Bilinear;
        return smoothnessTexture;
    }
}
