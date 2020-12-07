using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager main;

    private Camera mainCamera;

    [Header("Player Config Fields")]
    public float playerTurnSpeed = 0.1f;
    public Actor player;
    public StatManager playerStats;
    public Animator playerAnimator;

    public GameObject playerBulletHit;

    private Grapple grapple;
    public Grapple Grappler { get { return grapple; } }
    private Transform cursor;

    // ! -- Input Buffer Component.
    InputBuffer inputBuffer;

    private GameData gameData;
    public GameData GameData { get { return gameData; } }

    private void Awake() {

        main = this;

        AudioListener.volume = 0.3f;
        Time.timeScale = 1f;
        
        cursor = GameObject.Find("Cursor").transform;
        mainCamera = Camera.main;

        inputBuffer = GetComponent<InputBuffer>();
        
        InitializeGame();

        playerStats = player.transform.GetComponent<StatManager>();


    }

    // Start is called before the first frame update
    void Start()
    {
        // Start ambient sound for scene
        SoundManager.StartAmbientSound(AudioClips.singleton.ambientCave1, 0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.currMovement == Movement.FLYING) {
            grapple.GrappleUpdate(gameData.map);
        }

        InputUpdate();

        MovementSystem.MovementUpdate(gameData);
        CollisionSystem.CollisionUpdate(gameData);
        DamageSystem.HealthUpdate(gameData.allActors);
        MapSystem.Update(gameData);
        BehaviorAbilitySystem.Update(gameData);
    }

    void InitializeGame() {
        gameData = new GameData();

        gameData.map.InitNavMesh(GameObject.Find("MapRoot").transform);
        gameData.map.FindAreas(GameObject.Find("Areas").transform,gameData);
        gameData.map.FindZones();
        gameData.map.SetDoorHalfEdges();
        FindActors();
        FindObjects();

        gameData.navGrid = new NavGrid();
        gameData.navGrid.Init(gameData.map);
    }

    // Assigns the player gameobject a corresponding Actor, does the same for all enemies in enemies GameObject
    void FindActors() {
        Transform dudes = GameObject.Find("Dudes").transform;        

        // find player game object
        Transform p = dudes.Find("Player");
        ActorStats stats = p.GetComponent<ActorInfo>().stats;
        player = new Actor(p,stats,Team.PLAYER);
        grapple = new Grapple(player);
        gameData.allActors.Add(player);
        gameData.player = player;


        if(player == null) {
            Debug.LogError("cant find player");
        }        
                
        // find enemy game objects
        int count = 0;

        foreach (Transform child in dudes.Find("Enemies")) {
            AddActorFromGameobject(child.gameObject);
            count++;
        }
    }

    // the game used to have moving entities and stationary entities in separate lists
    // stationary entities were called objects
    // this is only used for old levels
    public void FindObjects() {
        Transform objectRoot = GameObject.Find("Objects").transform;
        Transform[] children = Utils.GetChildren(objectRoot);
        for (int i = 0; i < children.Length; i++) {
            ActorStats stats = children[i].GetComponent<ActorInfo>().stats;
            AddActorFromGameobject(children[i].gameObject);
        }
    }

    public void PlayerMovementInput() {
        if (player.currMovement != Movement.WALKING)
        {
            return;
        }

        // Movement Update
        Vector2 moveDir = inputBuffer.GetMovementVector();
        player.velocity = (moveDir * playerStats.GetStat(StatManager.StatType.Speed));

        // Animation Update
        if (player.velocity.sqrMagnitude > Mathf.Epsilon) {
            playerAnimator.SetBool("Running", true);
        } else {
            playerAnimator.SetBool("Running", false);
        }

        // Rotation Update
        Vector3 playerDirection = new Vector3();

        // This will be used to manage the rotation if the player is simply running forward.
        //      In lieu of an explicit rotation, we want to turn forward and play a forward running animation.
        Vector3 moveDirection = Utils.Vector2XZToVector3(inputBuffer.GetMovementVector());;
        
        // 2. We can check the input method using the InputBuffer component.
        if (!inputBuffer.GamepadInputActive()) {

            // Mouse Rotations - A way I did this is to cast a ray from the main camera to a Plane we create in world space.
            //      We can use the point of intersection between the ray and the plane as a target to look towards.
            Vector3 aimDirection;
            if (RaycastWorldMousePos(out aimDirection))
            {
                cursor.position = aimDirection;
            }

            player.transform.LookAt(new Vector3(aimDirection.x, player.transform.position.y, aimDirection.z));
        
        } else {
            
            // Controller Rotations - Controller is real simple. If any input is being held on the Rotation Stick (in this case Right Thumbstick),
            //      We rotate towards that. The rate at which we turn can be defined in the inspector, but usually hovers close to 0.1f.
            playerDirection = Utils.Vector2XZToVector3(inputBuffer.GetRotationVector());
            if (playerDirection.sqrMagnitude > 0.0f) {
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(playerDirection, Vector3.up), playerTurnSpeed);
            
            // If there's no rotation on the Right Thumbstick, we want to check the Movement Direction. If it's non-zero, we'll turn towards that instead.
            } else if (moveDirection != Vector3.zero && !(playerDirection.sqrMagnitude > 0.0f)) {
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), playerTurnSpeed);
            }
            
        }

        // Get current move vectors for animation.
        //      This Inverted so that the axis (forward / backward, for instance) is relative to the way the character is facing.
        float moveX = player.transform.InverseTransformDirection(moveDirection).x * playerStats.GetStatModifier(StatManager.StatType.Speed);
        float moveZ = player.transform.InverseTransformDirection(moveDirection).z * playerStats.GetStatModifier(StatManager.StatType.Speed);

        playerAnimator.SetFloat("MoveX", moveX, .05f, Time.deltaTime);
        playerAnimator.SetFloat("MoveZ", moveZ, .05f, Time.deltaTime);

        // See what vector has a greater value to determine speed of motion.
        //      It's a value between 0 and 1 such that the animation is relative to max speed.
        if (Mathf.Abs(moveX) > Mathf.Abs(moveZ)) {
            playerAnimator.SetFloat("Speed",  Mathf.Abs(moveX), .05f, Time.deltaTime);
        } else {
            playerAnimator.SetFloat("Speed", Mathf.Abs(moveZ), .05f, Time.deltaTime);
        }
    }

    public void InputUpdate()
    {
        PlayerMovementInput();
        
        Vector3 worldMousePos;
        if (RaycastWorldMousePos(out worldMousePos))
        {
            cursor.position = worldMousePos;
        }

        // draw player facing direction for debug
        Vector2 dir = Calc.Vector2FromTheta(player.transform.rotation.eulerAngles.y + 90,ANGLE_TYPE.DEGREES);
        dir.x *= -1;
        Debug.DrawLine(player.position3D,player.position3D + Utils.Vector2XZToVector3(dir.normalized * 2),Color.green);

        // quit app
        if (inputBuffer.ActionTriggered(InputName.Quit,true)) {
            Application.Quit();
        }
    }

    public void ShootGrapple(Vector2 dir) {
        playerAnimator.SetBool("Running", false);
        grapple.StartGrapple(dir,gameData.map);
    }

    // Get the mouse pos intersecting at y = 0
    public bool RaycastWorldMousePos(out Vector3 worldPos) {

        // We create an invisible plane at world pos 0,0,0
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;
        
        // Here's the ray. We cast from the camera to the mouse position.
        Ray cameraRay = mainCamera.ScreenPointToRay(inputBuffer.GetMousePosition());

        // If we intersect with the ground plane, we can get it's point of intersection and return that.
        if(groundPlane.Raycast(cameraRay, out rayLength)) {
            worldPos = cameraRay.GetPoint(rayLength);
            return true;
        } 

        worldPos = Vector3.negativeInfinity;
        return false;

    }

    // spawns a prefab at the position and hooks up actor class
    public Actor SpawnActor(GameObject prefab, Vector2 pos2D) {

        if(prefab.GetComponent<ActorInfo>() == null) {
            Debug.LogError("this object doesn't have an actor component");
            return null;
        }

        GameObject g = Instantiate(prefab);

        Actor a = AddActorFromGameobject(g);

        a.position2D = pos2D;
        a.currZone = gameData.map.ZoneFromPoint(pos2D);

        return a;
    }

    // hooks up actor class to an already instantiated game object 
    public Actor AddActorFromGameobject(GameObject go) {
        ActorInfo i = go.GetComponent<ActorInfo>();
        ActorStats s = i.stats;
        Actor a = new Actor(go.transform,s,Team.ENEMIES);
        i.actor = a;
        a.currZone = gameData.map.ZoneFromPoint(Utils.Vector3ToVector2XZ(transform.position));
        gameData.allActors.Add(a);
        gameData.enemyActors.Add(a);
        return a;
    }

    // Instantiates bullet and adds bullet to global list of bullets in game
    // Attaches bullet gameobject to bullet class
    public void ShootBullet(Quaternion targetRotation) {
        Bullet bullet2 = SpawnBullet((GameObject)Resources.Load("Prefabs/ProtoBullet"),gameData.player.position2D);
        bullet2.transform.rotation = targetRotation;
        SoundManager.StartClipOnActor(AudioClips.singleton.gunShot,player,6f,false);
    }

    // instaniates a bullet prefab and attaches it to a bullet class
    public Bullet SpawnBullet(GameObject prefab, Vector2 pos) {
        GameObject go = GameObject.Instantiate(prefab);
        Bullet bullet = new Bullet(go.transform,go.GetComponent<BulletInfo>().stats);
        bullet.position2D = pos;
        gameData.bullets.Add(bullet);

        return bullet;
    }
       
    // destroys the actors gameobject and removes it from list of actors
    public void DestroyActor(Actor a) {
        if(a.team == Team.ENEMIES) {
            gameData.enemyActors.Remove(a);
            gameData.allActors.Remove(a);
            Destroy(a.transform.gameObject);
        } else if (a.team == Team.PLAYER) // PLAYER DEATH CASE Using hacky method of disabling updates for now until proper death anim and conditions are made
        {
            gameData.allActors.Remove(a);

            // Update healthbar text to help convey death occured for now
            UnityEngine.UI.Text text = GameObject.FindGameObjectWithTag("HealthText").GetComponent<Text>();
            text.text = "HP: DEAD";
            text.color = Color.red;
            text.fontSize += 5;

            // Hacky way to halt gameplay....
            this.enabled = false; // Disable gameMan entirely
            a.transform.gameObject.GetComponent<Animator>().enabled = false; // Incase animation hangs after disabling
            Time.timeScale = 0; // Pauses everything else time-wise that is independant from gameMan
            GameObject.Find("PlayerCanvas").GetComponent<UIFunctions>().ActivateDeathUI();
        }
    }

    // destroys bullet object and removes from list of bullets
    public void DestroyBullet(Bullet b) {
        gameData.bullets.Remove(b);
        Destroy(b.transform.gameObject);
    }

    // instantiates a prefab at the position and adds to list of areas
    public void SpawnArea(GameObject prefab,Vector2 pos) {
        GameObject go = Instantiate(prefab);
        Area area = go.GetComponent<Area>();
        area.transform.position = Utils.Vector2XZToVector3(pos);
        gameData.areas.Add(area);
    }

    // destroys area object and removes from list of areas
    public void DestroyArea(Area a) {
        gameData.areas.Remove(a);
        Destroy(a.transform.gameObject);
    }

    public void DestroyGameobject(GameObject go) {
        Destroy(go);
    }

    // destroys all enemies in the enemy actors list
    public void DestroyAllEnemies() {
        for(int i = gameData.enemyActors.Count-1; i >= 0 ; i--) {
            DestroyActor(gameData.enemyActors[i]);
        }
        
    }
}


