using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TroopView : MonoBehaviour
{
    private TroopModel _model = null;
    private UITroopHPManager _troopHPUI = null;
    private SkinnedMeshRenderer _skinnedMeshRenderer = null;
    private Material _currentUnactedMaterial = null;
    private List<Material> _troopMaterials = null;
    private int _colorIndex = 0;

    public event Action<TroopModel, TroopModel> OnChargeArrived;

    //Shield
    [SerializeField] private GameObject _shield = null;

    //Arrow
    [SerializeField] private GameObject _arrow = null;

    //Acted Materials
    [SerializeField] private Material _frenchActedMaterial = null;
    [SerializeField] private Material _flemishActedMaterial = null;


    [Header("Death Materials")]
    [SerializeField] private Material _frenchDeath = null;
    [SerializeField] private Material _flemishDeath = null;
    [SerializeField] private Material _outlineDeath = null;

    [Header("Unity Events")]
    public UnityEvent OnSpawn;
    public UnityEvent OnMoveStart;
    public UnityEvent OnMoveEnd;
    public UnityEvent OnDead;
    public UnityEvent OnDeployed; //For the spawn particle
    public UnityEvent OnShotArrow;

    private Quaternion _beginRotation = Quaternion.identity;

    public GameObject Shield
    {
        get { return _shield; }
        set { _shield = value; }
    }

    private void Awake()
    {
        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        _troopMaterials = _skinnedMeshRenderer.materials.ToList();

        for (int i = 0; i < _troopMaterials.Count; i++)
        {
            Material mat = _troopMaterials[i];

            //Find the current French/Flemish and Metal Material to save and store their materials and index within the material array
            if (mat.name == "M_Gradient_FL (Instance)")
            {
                _currentUnactedMaterial = mat;
                _colorIndex = i;
            }

            if (mat.name == "M_Gradient_FR (Instance)")
            {
                _currentUnactedMaterial = mat;
                _colorIndex = i;
            }
        }

        _model = GetComponent<TroopModel>();
        _troopHPUI = GetComponentInChildren<UITroopHPManager>();
    }

    private void Start()
    {
        OnSpawn?.Invoke();
        //Hide shield
        if(_shield)
            _shield.SetActive(false);

        //Set the beginning rotation
        _beginRotation = transform.rotation;

        //Set the Troop's Act State to True;
        _model.HasActed = true;
    }

    private void OnEnable()
    {
        _model.OnTroopDeath += KillTroop;
        _model.OnTroopAct += UpdateOutline;
    }

    private void OnDisable()
    {
        _model.OnTroopDeath -= KillTroop;
        _model.OnTroopAct -= UpdateOutline;
    }

    //public void MoveTroop(Vector3 location, int speed, bool charge)
    internal void MoveTroop(object sender, TroopMovedEventArgs e)
    {
        //transform.position = CoordinatesHelper.DoubleCoordinatesToWorld(e.ToPosition.DoubleCoordinates, e.ToPosition.Dimensions);

        StartCoroutine(MoveToPos(CoordinatesHelper.DoubleCoordinatesToWorld(e.ToPosition.DoubleCoordinates, e.ToPosition.Dimensions), e.TroopData.MovementSpeed, e.Charge, e.Defender, e.Attacker));
    }

    public IEnumerator MoveToPosAfterDelay(Vector3 location, int speed, bool charge, TroopModel defender, TroopModel attacker, float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        //Start moving
        StartCoroutine(MoveToPos(location, speed, charge, defender, attacker));
    }

    public IEnumerator MoveToPos(Vector3 location, int speed, bool charge, TroopModel defender, TroopModel attacker)
    {
        if(!GetComponent<TroopModel>().IsDead)
        {
            //Rotate troop
            transform.LookAt(location);

            //Rotate the HP UI towards the camera
            _troopHPUI.LookAtCamera();

            //The radius the troop can be from it's target location to stop.
            const float acceptRadius = 0.8f;

            //The movement vector
            Vector3 toNewLoc = location - transform.position;

            //To check if we haven't overshot it
            Vector3 initialDir = toNewLoc;

            //Invoke the Move Event
            OnMoveStart?.Invoke();

            //Speed
            float currentSpeed = 0.0f;
            float minSpeed = 1.0f;//The minimum speed

            float decelDist = toNewLoc.magnitude * 0.25f;//The distance from where the troop needs to decelerate
            float accelDist = toNewLoc.magnitude * 0.75f;//The distance the troop needs to accelerate

            float acceleration = (speed * speed) / accelDist; //Acceleration formula
            float decelaration = ((speed * speed) - (minSpeed * minSpeed)) / (decelDist * 2.0f);//Deceleration formula

            //Check if we haven't arrived yet
            while (toNewLoc.sqrMagnitude > acceptRadius * acceptRadius)
            {
                //Check if we should decelerate;
                if (!charge && toNewLoc.sqrMagnitude < decelDist * decelDist)
                {
                    //Slow down if didn't hit the minimum speed yet
                    if (currentSpeed > minSpeed)
                    {
                        //Slow down
                        currentSpeed -= decelaration * Time.deltaTime;
                    }
                }

                //Update actualTime
                else
                {
                    //Accelerate when you aren't at top speed
                    if (currentSpeed < speed)
                        currentSpeed += acceleration * Time.deltaTime;
                }

                //Update position
                transform.position += toNewLoc.normalized * currentSpeed * Time.deltaTime;

                //Update the movement vector
                toNewLoc = location - transform.position;

                float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(initialDir, toNewLoc));

                //check if it is within the threshhold
                if(Mathf.Abs(angle) > 30)
                {
                    //Set it's transform
                    transform.position = location;
                    
                    break;
                }

                yield return null;
            }

            //Set the location
            transform.position = location;

            //Invoke the attack if it is a charge
            if (charge) OnChargeArrived?.Invoke(attacker, defender);

            //Rotate back if no charge
            else RotateBack();

            //Rotate the HP UI towards the camera
            _troopHPUI.LookAtCamera();

            //Invoke the Move End Event
            OnMoveEnd?.Invoke();
        }
    }

    internal void UpdateOutline(object sender, EventArgs e)
    {
        //Since we are using multiple materials for a mesh, we will need to create an instance of the materials array, update the indexs in the instance, and the set the material array to the instance
        if (_model.HasActed)
        {
            var mats = _skinnedMeshRenderer.materials;

            if (_model.TroopData.Side == GameSides.Flemish)
                mats[_colorIndex] = _flemishActedMaterial;
            else
                mats[_colorIndex] = _frenchActedMaterial;
                //mats[_colorIndex] = _frenchActedMaterial;

            _skinnedMeshRenderer.materials = mats;
        }
        else
        {
            var mats = _skinnedMeshRenderer.materials;
            mats[_colorIndex] = _currentUnactedMaterial;
             _skinnedMeshRenderer.materials = mats;
        }
    }

    public void LookAtEnemy(TroopModel enemy)
    {
        if (GetComponent<TroopModel>().IsDead) return;

        //Rotate troop
       transform.LookAt(enemy.transform.position);

        //Rotate the HP UI towards the camera
        _troopHPUI.LookAtCamera();
    }

    public void SetStartRotation(float degrees)
    {
        if (GetComponent<TroopModel>().IsDead)
            return;

        //Rotate
        transform.Rotate(new Vector3(0.0f, degrees, 0.0f));

        //Update the startingrotation
        _beginRotation = transform.rotation;

        //Rotate the HP UI towards the camera
        _troopHPUI.LookAtCamera();
    }

    public void RotateBack()
    {
        //Check if he isn't dead
        if (GetComponent<TroopModel>().IsDead) return;

        //Rotate back
        GetComponent<Transform>().rotation = _beginRotation;
        
        //Troop HP rotate with it
        _troopHPUI.LookAtCamera();
    }

    public IEnumerator DelayedRotateBack(float delay)
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);

        //Start the function
        RotateBack();
    }

    //This is called from a StateMachineBehaviour which cannot start a coroutine
    public void StartDelayedCoroutine(float delay)
    {
        StartCoroutine(DelayedRotateBack(delay));
    }

    public void ShootArrow(bool isArcher, TroopModel attacker, TroopModel defender)
    {
        //Check if we have an arrow
        if (!_arrow) return;

        //Create the arrow
        var arrowGO = Instantiate(_arrow, transform);

        //Set the crossbow as parent
        arrowGO.transform.SetParent(gameObject.transform);

        //Arrow
        var arrowScript = arrowGO.GetComponent<Arrow>();

        if(arrowScript)
        {
            //Invoke the unity event
            OnShotArrow?.Invoke();

            //Shoot the arrow in straight line when it is not an archer
            if (!isArcher)
                arrowScript.StartStraightArrow(defender.transform.position, attacker, defender);
            //Shoot in a curve when it is an arrow
            else arrowScript.StartCurveArrow(defender.transform.position + new Vector3(0, defender.GetComponent<CapsuleCollider>().height, 0), attacker, defender);
        }
    }

    internal void KillTroop(object sender, EventArgs e)
    {
        gameObject.GetComponentInChildren<UITroopHPManager>().FadeOutImmedately();

        OnDead?.Invoke();
    }

    public void DissolveTroop()
    {
        var mats = _skinnedMeshRenderer.materials;

        for (int i = 0; i < mats.Length; i++)
        {
            if (i == _colorIndex)
            {
                if (_model.TroopData.Side == GameSides.Flemish)
                    mats[_colorIndex] = _flemishDeath;
                else
                    mats[_colorIndex] = _frenchDeath;

                mats[_colorIndex].SetFloat("_Dissolve_Amount", 0f);
            } else
            {
                mats[i] = _outlineDeath;
                mats[i].SetFloat("_Dissolve_Amount", 1f);
            }
        }

        _skinnedMeshRenderer.materials = mats;

        LeanTween.value(this.gameObject, UpdateDissolveAmount, 0f, 1f, 1.5f).setOnComplete(DestroyTroop);
    }

    private void UpdateDissolveAmount(float amount)
    {
        _skinnedMeshRenderer.materials[_colorIndex].SetFloat("_Dissolve_Amount", amount);
    }

    private void DestroyTroop()
    {
        Destroy(this.gameObject);
    }
}
