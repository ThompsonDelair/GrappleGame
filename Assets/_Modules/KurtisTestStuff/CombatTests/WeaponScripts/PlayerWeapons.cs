using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    // Component References
    private FieldOfView fieldOfView;
    private Animator playerAnimator;
    public GameManager gameManager;

    void Start() {
        playerAnimator = this.GetComponent<Animator>();
        fieldOfView = this.GetComponent<FieldOfView>();

        if (gameManager == null) {
            gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        }
    }

    [Header("Blaster Parameters")]
    public float fireRate;
    public float chargeTime = 0f;
    [SerializeField] private float chargeTimeThreshold = 1f; // the amount of time you need to hold the charge button before the rail cannon activates
    public bool ChargeThresholdReached { get { return chargeTime >= chargeTimeThreshold; } }
    [SerializeField] private float blasterDamage = 1f;
    [SerializeField] private float attackKnockback = 100f;

    [Header("Blaster Range Modifiers")]
    [SerializeField] private float maxRailAngle = 110f;
    [SerializeField] private float minRailAngle = 8f;
    [SerializeField] private float maxRailRange = 22f;
    [SerializeField] private float minRailRange = 5f;

    [Header("Melee Strike Modifiers")]
    [SerializeField] private float meleeStrikeRange = 6f;
    [SerializeField] private float meleeStrikeDamage = 3f;
    

    [Header("Blaster States")]
    public State chargingState;
    public State firingState;
    public State blasterRecoveryState;

    [Header("Grappler States")]
    public State grapplingState;
    public State grappleCancelState;

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

        // Render the cone if no parameter is sent.
        if (renderCone) {
            if (ChargeThresholdReached) {
                // Draw cone AoE
                fieldOfView.DrawFieldOfView();

                // Adjust field of view angle based on total charge time.
                if (fieldOfView.ViewAngle > minRailAngle) {
                    fieldOfView.ViewAngle = maxRailAngle * ( 1 / chargeTime * 0.6f );
                } else {
                    fieldOfView.ViewAngle = minRailAngle;
                }

                // Adjust field of view range based on total charge time.
                if (fieldOfView.ViewRadius < maxRailRange) {
                    fieldOfView.ViewRadius = minRailRange * ( chargeTime * 1.2f );
                } else {
                    fieldOfView.ViewRadius = maxRailRange;
                }
            }
        }

    }

    // When charge state is released, and charge time is below threshold, Light Blaster is fired.
    public void FireBlaster() {
        Debug.Log("Fire Light Blaster");
        playerAnimator.SetTrigger("LightBlaster");

        gameManager.ShootBullet( transform.localRotation );
        
        ResetCharge();
    }

    public void FireRailCannon() {
        Debug.Log("Fire Heavy Blaster");
        playerAnimator.SetTrigger("HeavyBlaster");

        // Get list of enemies from game manager, check their position vs. FieldOfView. Deal damage if within cone.
        List<EnemyActor> enemyList = gameManager.EnemyList;

        gameManager.player.AddPushForce(Utils.Vector3ToVector2XZ(-FacingDirection), 50f);

        int hitEnemies = 0;
        foreach (Actor enemy in enemyList) {
            
            if (fieldOfView.WithinRadius(enemy.transform.position, fieldOfView.ViewRadius) && fieldOfView.WithinAngle(enemy.transform.position, FacingDirection, fieldOfView.ViewAngle)) {
                hitEnemies++;

                // The charge will always do at least thrice the damage of the blaster
                DamageSystem.DealDamage(enemy, blasterDamage * (chargeTime >= 1f ? (chargeTime + 2) : 3f));
                enemy.AddPushForce(transform.DirectionToTarget(enemy.transform.position), attackKnockback);

            }
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
        if (gameManager.player.movement == Movement.WALKING) {
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

        // Get list of enemies from game manager, check their position vs. Melee Raidus. Deal damage if within cone.
        List<EnemyActor> enemyList = gameManager.EnemyList;

        gameManager.player.AddPushForce(Utils.Vector3ToVector2XZ(FacingDirection), 50f);

        int hitEnemies = 0;
        foreach (Actor enemy in enemyList) {
            
            if (fieldOfView.WithinRadius(enemy.transform.position, meleeStrikeRange)) {
                hitEnemies++;
                DamageSystem.DealDamage(enemy, meleeStrikeDamage);
                enemy.AddPushForce(transform.DirectionToTarget(enemy.transform.position), attackKnockback);
            }
        }
    }

    private void ResetCharge() {
        chargeTime = 0f;
        fieldOfView.ViewAngle = maxRailAngle;
        fieldOfView.ViewRadius = minRailRange;

        playerAnimator.SetFloat("ChargeTime", chargeTime);
    }
}
