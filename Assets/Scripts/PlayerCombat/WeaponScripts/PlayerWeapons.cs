using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    // Component References
    private FieldOfView fieldOfView;
    private Animator playerAnimator;
    [HideInInspector] public GameManager gameManager;

    void Start() {
        playerAnimator = this.GetComponent<Animator>();
        fieldOfView = this.GetComponent<FieldOfView>();
         

        if (gameManager == null) {
            gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        }

        slashVisual = GetComponent<AutoTurnOff>();
    }

    [Header("Blaster Parameters")]
    public float chargeTime = 0f;
    [SerializeField] private float chargeTimeThreshold = 1f; // the amount of time you need to hold the charge button before the rail cannon activates
    [SerializeField] private float chargeRateModifier = 1.2f;
    public bool ChargeThresholdReached { get { return chargeTime >= chargeTimeThreshold; } }
    [SerializeField] private float blasterDamage = 1f;
    [SerializeField] private float attackKnockback = 50f;

    [Header("Blaster Range Modifiers")]
    [SerializeField] private float maxRailAngle = 110f;
    [SerializeField] private float minRailAngle = 8f;
    [SerializeField] private float maxRailRange = 22f;
    [SerializeField] private float minRailRange = 5f;

    [Header("Melee Strike Modifiers")]
    public bool canGrapple = true;
    [SerializeField] private float meleeStrikeRange = 6f;
    [SerializeField] private float meleeStrikeAngle = 360f;
    [SerializeField] private float meleeStrikeDamage = 3f;

    [Header("Dodge Modifiers")]
    [SerializeField] private int currentDodgeCounter = 0;
    [SerializeField] private int maxDodges = 2;
    public bool CanDodge { get { return currentDodgeCounter < maxDodges; } }
    
    [Header ("Effect Transforms")]
    [SerializeField] private Transform firingLocation;

    [Header("Blaster States")]
    public State chargingState;
    public State firingState;
    public State blasterRecoveryState;

    [Header("Blaster Effects")]
    [SerializeField] private GameObject firingEffect;
    [SerializeField] private GameObject chargeEffect;
    [SerializeField] private GameObject chargeFireEffect;

    [Header("Grappler States")]
    public State grapplingState;
    public State grappleCancelState;
    public State grappleRecoveryState;

    [Header("Dodge States")]
    public State dodgeState;
    public State dodgeRecoveryState;

    private AutoTurnOff slashVisual;
    private Vector3 FacingDirection { get { 
            Vector2 dir = Calc.Vector2FromTheta(transform.rotation.eulerAngles.y + 90, ANGLE_TYPE.DEGREES);
            dir.x *= -1;

            return new Vector3(dir.x, 0, dir.y);
        }
    }

    // Blaster Functions

    // Enters charged state. Called once when blaster is held.
    public void EnterChargeState() {
        playerAnimator.SetTrigger( "ChargeBlaster" );

        fieldOfView.ViewAngle = maxRailAngle;
        fieldOfView.ViewRadius = minRailRange;
    }

    // Called continuously while charge state is active.
    public void ChargeRailCannon(bool renderCone = true) {
        //Debug.Log("Charging...");
        chargeTime += Time.deltaTime;
        playerAnimator.SetFloat("ChargeTime", chargeTime);
        
        // Render the cone if the renderCone is not explicitly turned off.
        if (renderCone) {
            if (ChargeThresholdReached) {
                // Draw cone AoE
                fieldOfView.DrawFieldOfView();
                ParticleEmitter.current.SpawnParticleEffect(chargeEffect, firingLocation.position ,Quaternion.identity);

                // Adjust field of view angle based on total charge time.
                if (fieldOfView.ViewAngle > minRailAngle) {
                    fieldOfView.ViewAngle = maxRailAngle * ( 1 / (chargeTime * chargeRateModifier) * 0.6f );
                } else {
                    fieldOfView.ViewAngle = minRailAngle;
                }

                // Adjust field of view range based on total charge time.
                if (fieldOfView.ViewRadius < maxRailRange) {
                    fieldOfView.ViewRadius = minRailRange * ((chargeTime * chargeRateModifier) * 1.2f );
                } else {
                    fieldOfView.ViewRadius = maxRailRange;
                }
            }
        }
    }

    // When charge state is released, and charge time is below threshold, Light Blaster is fired.
    public void FireBlaster() {
        //Debug.Log("Fire Light Blaster");
        playerAnimator.SetTrigger("LightBlaster");

        gameManager.ShootBullet( transform.localRotation );
        ParticleEmitter.current.SpawnParticleEffect(this.transform, firingEffect, firingLocation.position);
        EffectController.main.CameraShake(0.1f, 0.02f);
        
        ResetCharge();
    }

    public void FireRailCannon() {
        SoundManager.StartClipOnActor(AudioClips.singleton.chargeShotShot, GameManager.main.player, 6f, false);

        //Debug.Log("Fire Heavy Blaster");
        playerAnimator.SetTrigger("HeavyBlaster");

        // Get list of enemies from game manager, check their position vs. FieldOfView. Deal damage if within cone.
        //List<EnemyActor> enemyList = gameManager.EnemyList;
        //List<Actor> objectList = gameManager.ObjectList;
        List<Actor> enemyActors = gameManager.GameData.enemyActors;

        gameManager.player.AddPushForce(Utils.Vector3ToVector2XZ(-FacingDirection), 50f);
        ParticleEmitter.current.SpawnParticleEffect( chargeFireEffect, firingLocation.position, this.transform.rotation * Quaternion.Euler(0, 90, 0));

        int hitEnemies = 0;

        // Iterate through enemy actor list.
        foreach (Actor actor in enemyActors) {

            if (actor.team != Team.ENEMIES)
                continue;
            
            if ((fieldOfView.WithinRadius(actor.transform.position, fieldOfView.ViewRadius) && fieldOfView.WithinAngle(actor.transform.position, FacingDirection, fieldOfView.ViewAngle))) {
                // || (fieldOfView.WithinRadius(enemy.transform.position, fieldOfView.ViewRadius) && fieldOfView.WithinAngle(enemy.transform.position, FacingDirection, fieldOfView.ViewAngle))) {
                hitEnemies++;

                // The charge will always do at least thrice the damage of the blaster
                DamageSystem.DealDamage(actor, blasterDamage * (3 + chargeTime));

                if (actor.stats.movement != Movement.NONE) {
                    actor.AddPushForce(transform.DirectionToTarget(actor.transform.position),attackKnockback);
                }
            }
        }
    
        //// Iterate through destructable object list.
        //foreach (Actor structure in objectList) {
            
        //    if (fieldOfView.WithinRadius(structure.transform.position, fieldOfView.ViewRadius) && fieldOfView.WithinAngle(structure.transform.position, FacingDirection, fieldOfView.ViewAngle)) {
        //        hitEnemies++;

        //        // The charge will always do at least thrice the damage of the blaster
        //        DamageSystem.DealDamage(structure, blasterDamage * (3 + chargeTime));
        //        // structure.AddPushForce(transform.DirectionToTarget(structure.transform.position), attackKnockback);

        //    }
        //}

        EffectController.main.CameraShake(0.3f, 0.3f);
        if (hitEnemies > 0) {
            // EffectController.main.HitStop(0.3f);
            
        }

        ResetCharge();
        fieldOfView.ClearViewMesh();
    }

    // Grapple Functions
    public void FireGrapple() {
        Debug.Log("Fire Grapple");
        playerAnimator.SetTrigger("GrappleEnter");

        gameManager.ShootGrapple(Utils.Vector3ToVector2XZ(FacingDirection));
    }

    public void Grappling() {
        if (gameManager.player.currMovement == Movement.WALKING) {
            playerAnimator.SetBool("Grappling", false);
            
        } else if (gameManager.Grappler.GrappleLanded) {
            playerAnimator.SetBool("Grappling", true);
        }
    }
    
    public void CancelGrapple() {

        Debug.Log("Cancel Grapple");
        playerAnimator.SetTrigger("GrappleCancel");
        playerAnimator.SetBool("Grappling", false);
        gameManager.Grappler.EndGrapple();
        canGrapple = false;

        fieldOfView.ViewAngle = meleeStrikeAngle;
        fieldOfView.ViewRadius = meleeStrikeRange;
        // fieldOfView.DrawFieldOfView();

        // Get list of enemies from game manager, check their position vs. Melee Raidus. Deal damage if within cone.
        //List<EnemyActor> enemyList = gameManager.EnemyList;
        //List<Actor> objectList = gameManager.ObjectList;

        List<Actor> enemyActors = gameManager.GameData.enemyActors;
        
        gameManager.player.AddPushForce(Utils.Vector3ToVector2XZ(FacingDirection), 50f);

        int hitEnemies = 0;

        slashVisual.Activate();
        slashVisual.target.GetComponent<ParticleSystem>().Play();
        AudioSource.PlayClipAtPoint(AudioClips.singleton.fireLand,transform.position);

        // Iterate through enemy actor list.
        foreach (Actor actor in enemyActors) {
            
            //if(actor.team != Team.ENEMIES) {
            //    continue;
            //}

            if (fieldOfView.WithinRadius(actor.transform.position, meleeStrikeRange)) {
                hitEnemies++;
                DamageSystem.DealDamage(actor, meleeStrikeDamage);

                if (actor.stats.movement != Movement.NONE) {
                    actor.AddPushForce(transform.DirectionToTarget(actor.transform.position),attackKnockback);
                }
            }

        }

        //// Iterate through destructable object list.
        //foreach (Actor structure in objectList) {
            
        //    if (fieldOfView.WithinRadius(structure.transform.position, meleeStrikeRange)) {
        //        hitEnemies++;

        //        // The charge will always do at least thrice the damage of the blaster
        //        DamageSystem.DealDamage(structure, meleeStrikeDamage);
        //        // structure.AddPushForce(transform.DirectionToTarget(structure.transform.position), attackKnockback);

        //    }
        //}

        if (hitEnemies > 0) {
            // EffectController.main.HitStop(0.3f);
            EffectController.main.CameraShake(0.1f, 0.1f);
        }
    }

    // Dodge Functions

    public void Dodge(Vector2 inputDir, float dodgeForce) {
        Debug.Log("Dodge");
        playerAnimator.SetTrigger("Dodge");
        
        // 1. Increment dodge counter
        currentDodgeCounter++;

        SoundManager.StartClipOnActor(AudioClips.singleton.dash, GameManager.main.player, 0.025f, false);

        // 2. Apply force to the player
        if (inputDir != Vector2.zero) {
            gameManager.player.StartNewPush(inputDir, dodgeForce);
        } else {
            gameManager.player.StartNewPush(-Utils.Vector3ToVector2XZ(FacingDirection), dodgeForce);
        }
        
        // gameManager.player.AddPushForce(inputDir, dodgeForce);
    }

    public void ResetDodge() {
        currentDodgeCounter = 0;
    }

    public void ResetGrapple() {
        canGrapple = true;
    }

    private void ResetCharge() {
        chargeTime = 0f;
        fieldOfView.ViewAngle = maxRailAngle;
        fieldOfView.ViewRadius = minRailRange;

        playerAnimator.SetFloat("ChargeTime", chargeTime);
    }
}
