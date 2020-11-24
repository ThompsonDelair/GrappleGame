using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GrappleHit { NONE, TERRAIN, ACTOR_SMALLER, ACTOR_LARGER }

public delegate float EaseDelegate(float t);

[RequireComponent(typeof(DebugControl))]
public class GameManager : MonoBehaviour
{
    //private CSharpCodeProvider codeProvider;
    //private CompilerParameters cp;

    public bool drawTerrain;
    public bool drawNavMesh;
    public bool drawGrappleRange;

    public static GameManager main; 
    private Camera mainCamera;

    [Header("Player Config Fields")]
    public float playerTurnSpeed = 0.1f;
    public Actor player;
    public StatManager playerStats;
    public Animator playerAnimator;
    public float playerBulletDamage = 1f;

    //List<Actor> enemies;

    //public List<Actor> EnemyList { get { return enemies; } }
    //public List<Actor> ObjectList { get { return map.objects; } }

    //List<Actor> allActors = new List<Actor>();

    //public List<Bullet> bullets = new List<Bullet>();
    public GameObject playerBulletHit;



    //public List<Area> Areas { get { return areas; } }

    //public List<Area> areas = new List<Area>();

    Grapple grapple;
    public Grapple Grappler { get { return grapple; } }
    Transform cursor;

    // ! -- Input Buffer Component.
    InputBuffer inputBuffer;

    [Header("Enemy Spawn Prefabs")]
    [SerializeField] private GameObject smallEnemy = null;
    [SerializeField] private GameObject bigJoe = null;

    //public Map map;

    public bool hazardColliding;

    int playerHazardTimer = 40;

    //public LineRenderer lineRenderer;

    public GameData gameData;

    // THIS MUST BE MOVED TO DAMAGESYSTEM AND ACCESSED THRU SOME KIND OF DATASTRUCT LATER, SHOULDNT BE HERE, JUST BEIN HACKY FOR NOW -All Caps Zora
 //   [Header("Health Params")]
    //public float playerHealth;
    //public float lightEnemyHealth;
    //public float basicEnemyHealth;
    //public float heavyEnemyHealth;
 

    private void Awake() {



        AudioListener.volume = 0.3f;

        Time.timeScale = 1f;


        main = this;
        cursor = GameObject.Find("Cursor").transform;
        mainCamera = Camera.main;

        inputBuffer = GetComponent<InputBuffer>();
        
        Init();
        //lineRenderer = GetComponent<LineRenderer>();

        playerStats = player.transform.GetComponent<StatManager>();

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (drawTerrain)
            DrawTerrain();

        InputUpdate();

        MovementSystem.MovementUpdate(gameData);
        CollisionSystem.CollisionUpdate(gameData);
        DamageSystem.HealthUpdate(gameData.allActors);
        AbilitySystem.Update(gameData);

        //DamageSystem.HealthUpdate(map.objects);

    }

    private void FixedUpdate() {
        if (player.currMovement == Movement.FLYING) {
            grapple.GrappleUpdate(gameData.map);
        }
    }

    void Init() {
        gameData = new GameData();
        FindActors();
        FindObjects();
        gameData.map.InitNavMesh(GameObject.Find("MapRoot").transform);
        gameData.map.FindAreas(GameObject.Find("Areas").transform);
        gameData.map.FindZones();
        gameData.map.SetDoorHalfEdges();
        
    }

    //void HazardCheck() {
    //    if (hazardColliding) {
    //        playerStats.AddModifier(StatManager.StatType.Speed, -0.9f);
    //        player.velocity = player.velocity.normalized * playerStats.GetStat(StatManager.StatType.Speed);
    //        playerHazardTimer--;
    //        if (playerHazardTimer <= 0) {
    //            hazardColliding = false;
    //            playerHazardTimer = 40;
    //        }

    //        if (player.currMovement == Movement.FLYING) {
    //            grapple.EndGrapple();
    //        }

    //        //Debug.Log("Collision...");
    //    } else {
    //       playerStats.RemoveModifier(StatManager.StatType.Speed, -0.9f);
    //    }
    //}

    void DrawTerrain() {
        for(int i = 0; i < gameData.map.terrainEdges.Count; i++) {
            //Layer[] types = map.VertTypes[i];
            TerrainEdge e = gameData.map.terrainEdges[i];
            Color c;

            if (e.layer == Layer.BLOCK_FLY) {
                c = CustomColors.darkRed;
            } else {
                c = Color.black;
            }
            Debug.DrawLine(e.vertA_pos3D,e.vertB_pos3D,c);
        }
    }

    // Assigns the player gameobject a corresponding Actor, does the same for all enemies in enemies GameObject
    void FindActors() {
        Transform dudes = GameObject.Find("Dudes").transform;        

        Transform p = dudes.Find("Player");
        ActorStats stats = p.GetComponent<ActorInfo>().stats;
        player = new Actor(p,stats,Team.PLAYER);
        grapple = new Grapple(player);
        gameData.allActors.Add(player);
        gameData.player = player;
        if(player == null) {
            Debug.LogError("cant find player");
        }        

        //List<Actor> enemyList = new List<Actor>();
        
        int count = 0;

        foreach (Transform child in dudes.Find("Enemies")) {
            AddActorFromGameobject(child.gameObject);
            count++;
        }
        //enemies = enemyList;
    }

    public void FindObjects() {
        Transform objectRoot = GameObject.Find("Objects").transform;
        Transform[] children = Utils.GetChildren(objectRoot);
        for (int i = 0; i < children.Length; i++) {
            //ActorNameComponent name = children[i].GetComponent<ActorNameComponent>();
            ActorStats stats = children[i].GetComponent<ActorInfo>().stats;

            AddActorFromGameobject(children[i].gameObject);

            //if (stats.mainAttack == "TargetedShot") // Hacky way to segregate other objects from enemy sentry for now
            //{
            //    Actor a = new Actor(children[i],stats,Team.ENEMIES);
            //    //Debug.Log("THIS GUY: " + i + enemyActor.transform.gameObject.name);
            //    enemyObjects.Add(enemyActor);
            //    objects.Add(enemyActor);
            //} else {
            //    Actor a = new Actor(children[i],stats,Layer.ENEMIES);
            //    objects.Add(a);
            //}

        }
    }

    // 
    //public Ability AbilityStringToClass(string abilityName)
    //{
    //    Debug.Log("BUILDING " + abilityName + " ABILITY CLASS FOR ");
    //    if(abilityName == "Lunge")
    //    {
    //        Ability ability = new Lunge();
    //        return ability;
    //    }
    //    else if (abilityName == "TargetedShot")
    //    {
            
    //        Ability ability = new TargetedShot();
    //        return ability;
    //    }

    //    return null;
    //}


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
            //DrawNavTriAdjacent(Utils.Vector3ToVector2XZ(worldMousePos));
        }

        Vector2 dir = Calc.Vector2FromTheta(player.transform.rotation.eulerAngles.y + 90,ANGLE_TYPE.DEGREES);
        dir.x *= -1;
        Debug.DrawLine(player.position3D,player.position3D + Utils.Vector2XZToVector3(dir.normalized * 2),Color.green);


        // Check for Grapple Input
        // if (inputBuffer.ActionTriggered(InputName.Grapple, true) && player.movement == Movement.WALKING)
        // {
        //     Debug.Log("Grapple");
        //     //dir.x *= -1;
        //     ShootGrapple(dir);
        // }

        // quit app
        if (inputBuffer.ActionTriggered(InputName.Quit,true)) {
            Application.Quit();
        }
    }

    // Instantiates bullet and adds bullet to global list of bullets in game
    // Attaches bullet gameobject to bullet class
    public void ShootBullet(Quaternion targetRotation)
    {
        GameObject gameObjBullet = (GameObject)Instantiate(Resources.Load("Prefabs/ProtoBullet"));
        Bullet bullet = new Bullet(gameObjBullet.transform, 0.5f,Team.ENEMIES,playerBulletDamage);
        bullet.position3D = player.position3D;
        gameData.bullets.Add(bullet);

        player.PlayAudioClip(AudioClips.singleton.gunShot);
        // Set to shoot in that direction
        bullet.transform.rotation = targetRotation;
    }

    public void ShootGrapple(Vector2 dir) {
        playerAnimator.SetBool("Running", false);
        grapple.StartGrapple(dir,gameData.map);
    }

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

    
    public Actor SpawnActor(GameObject prefab, Vector2 pos2D) {

        if(prefab.GetComponent<ActorInfo>() == null) {
            Debug.LogError("this object doesn't have an actor component");
            return null;
        }

        GameObject g = Instantiate(prefab);

        Actor a = AddActorFromGameobject(g);

        a.position2D = pos2D;

        return a;
    }

    public Actor AddActorFromGameobject(GameObject go) {
        ActorStats s = go.GetComponent<ActorInfo>().stats;
        Actor a = new Actor(go.transform,s,Team.ENEMIES);
        gameData.allActors.Add(a);
        gameData.enemyActors.Add(a);
        return a;
    }

    public Actor SpawnRandomEnemy(Vector2 pos) {
        int rand = UnityEngine.Random.Range(0,12);
        GameObject prefab = (rand == 0) ? bigJoe : smallEnemy;
        GameObject go = Instantiate(prefab,Utils.Vector2XZToVector3(pos),Quaternion.identity);
        Actor a = AddActorFromGameobject(go);
        return a;
    }
       
    public void DestroyActor(Actor a) {
        if(a.team == Team.ENEMIES) {
            gameData.enemyActors.Remove(a);
            gameData.allActors.Remove(a);
            Destroy(a.transform.gameObject);
        } else if (a.team == Team.PLAYER) // PLAYER DEATH CASE Using hacky method of disabling updates for now until proper death anim and conditions are made
        {
            gameData.allActors.Remove(a);

            // Update healthbar text to help convey death occured for now
            UnityEngine.UI.Text text = a.transform.Find("PlayerCanvas").GetChild(0).GetComponent<Text>();
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

    public void DestroyGameobject(GameObject go) {
        Destroy(go);
    }

    public GameObject GetNewSentryBullet()
    {
        return (GameObject)Instantiate(Resources.Load("Prefabs/EvilBullet"));
    }

    public void OnDrawGizmos() {
        if(gameData != null) {
            gameData.map.DrawTerrainEdgesGizmos();
        }


        if (drawGrappleRange && GameObject.Find("Player") != null) {
            Vector3 playerPos = GameObject.Find("Player").transform.position;
            Vector2[] circles = Calc.CircularPointsAroundPosition(Utils.Vector3ToVector2XZ(playerPos),20,Grapple.grappleRange);
            for (int i = 0; i < circles.Count(); i++) {
                Debug.DrawLine(Utils.Vector2XZToVector3(circles[i]),Utils.Vector2XZToVector3(circles[(i + 1) % circles.Count()]),CustomColors.green);
            }
            Debug.DrawLine(playerPos,playerPos + Vector3.forward * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.back * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.left * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.right * Grapple.grappleRange,CustomColors.green);
        }
    }

    //public void BulletMovementUpdate(List<Bullet> bullets) {
    //    for (int i = 0; i < bullets.Count; i++) {
    //        if (bullets[i].transform != null) {
    //            bullets[i].transform.position += bullets[i].transform.rotation * transform.forward * Time.deltaTime * bullets[i].speed;
    //        }
    //    }
    //}
}


