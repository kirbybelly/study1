using UnityEngine;

public class PrototypeSceneBuilder : MonoBehaviour
{
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
        RemoveWall(cafeteria, "WallRight");

        CreatePrimitive("ServingCounter", PrimitiveType.Cube, accentMaterial, new Vector3(-5.8f, 1.1f, 4.2f), new Vector3(4.8f, 2.2f, 1.1f), cafeteria);
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
        Transform turnCorridor = CreateRoomShell("SaunaTurnCorridor", new Vector3(23f, 0f, -7.5f), 7f, 4f, 12f, saunaMaterial, new Color(0.46f, 0.29f, 0.19f));

        RemoveWall(corridor, "WallLeft");
        RemoveWall(corridor, "WallRight");
        RemoveWall(turnCorridor, "WallFront");
        RemoveWall(turnCorridor, "WallBack");

        CreatePrimitive("BenchLeft", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(-1.5f, 0.45f, -2.2f), new Vector3(3f, 0.9f, 0.7f), corridor);
        CreatePrimitive("BenchRight", PrimitiveType.Cube, cafeteriaMaterial, new Vector3(2.4f, 0.45f, 2.1f), new Vector3(3.2f, 0.9f, 0.7f), corridor);
        CreatePrimitive("TowelShelf", PrimitiveType.Cube, metalMaterial, new Vector3(1.6f, 2f, -2.9f), new Vector3(2.4f, 0.2f, 0.5f), corridor);
        CreatePrimitive("FrontDesk", PrimitiveType.Cube, accentMaterial, new Vector3(-4.7f, 1f, 2.5f), new Vector3(2f, 2f, 1f), corridor);

        for (int i = 0; i < 3; i++)
        {
            CreatePrimitive("Locker_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(4.2f, 1.2f, -2.5f + i * 1.8f), new Vector3(1.1f, 2.4f, 0.65f), corridor);
        }

    }

    private void BuildRestroom()
    {
        Transform restroom = CreateRoomShell("MensRestroom", new Vector3(23f, 0f, -15f), 12f, 4f, 8f, restroomMaterial, new Color(0.77f, 0.8f, 0.82f));
        RemoveWall(restroom, "WallFront");

        CreatePrimitive("MirrorStrip", PrimitiveType.Cube, glassMaterial, new Vector3(-1.2f, 2f, -3.7f), new Vector3(4.4f, 1.3f, 0.08f), restroom);
        CreatePrimitive("SinkCounter", PrimitiveType.Cube, metalMaterial, new Vector3(-1.2f, 0.92f, -3.3f), new Vector3(4.6f, 0.95f, 1f), restroom);

        for (int i = 0; i < 3; i++)
        {
            float x = -3f + i * 1.8f;
            CreatePrimitive("Sink_" + i, PrimitiveType.Cylinder, wallMaterial, new Vector3(x, 1.2f, -3.25f), new Vector3(0.42f, 0.08f, 0.42f), restroom);
            CreatePrimitive("Faucet_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(x, 1.45f, -3.55f), new Vector3(0.16f, 0.24f, 0.16f), restroom);
        }

        for (int i = 0; i < 3; i++)
        {
            float z = -2f + i * 2f;
            CreatePrimitive("Urinal_" + i, PrimitiveType.Cube, wallMaterial, new Vector3(3.85f, 1.25f, z), new Vector3(0.7f, 1.35f, 0.48f), restroom);
            CreatePrimitive("Divider_" + i, PrimitiveType.Cube, metalMaterial, new Vector3(3.2f, 1.35f, z + 0.75f), new Vector3(0.08f, 1.5f, 1.1f), restroom);
        }

        for (int i = 0; i < 2; i++)
        {
            float z = -1.8f + i * 3.2f;
            BuildToiletStall(restroom, new Vector3(0.85f, 0f, z));
        }

        CreatePrimitive("TrashCan", PrimitiveType.Cylinder, accentMaterial, new Vector3(-4.5f, 0.48f, 2.9f), new Vector3(0.45f, 0.46f, 0.45f), restroom);
        CreatePrimitive("HandDryer", PrimitiveType.Cube, metalMaterial, new Vector3(-4.8f, 1.55f, -2.9f), new Vector3(0.55f, 0.55f, 0.24f), restroom);
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
        CreatePrimitive("StallSideA", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(0f, 1.4f, -0.75f), new Vector3(0.08f, 2.8f, 1.5f), restroom);
        CreatePrimitive("StallSideB", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(1.45f, 1.4f, -0.75f), new Vector3(0.08f, 2.8f, 1.5f), restroom);
        CreatePrimitive("StallDoor", PrimitiveType.Cube, metalMaterial, localOrigin + new Vector3(0.72f, 1.4f, -1.45f), new Vector3(1.36f, 2.8f, 0.08f), restroom);
        CreatePrimitive("ToiletBowl", PrimitiveType.Cylinder, wallMaterial, localOrigin + new Vector3(0.72f, 0.58f, -0.2f), new Vector3(0.36f, 0.22f, 0.36f), restroom);
        CreatePrimitive("ToiletBack", PrimitiveType.Cube, wallMaterial, localOrigin + new Vector3(0.72f, 0.95f, 0.18f), new Vector3(0.55f, 0.66f, 0.24f), restroom);
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
}
