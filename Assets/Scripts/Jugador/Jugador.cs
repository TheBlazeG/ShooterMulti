using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using TMPro;
using Newtonsoft.Json;
using NUnit.Framework;

public class Jugador : NetworkBehaviour
{
    #region Parametros
    private Rigidbody _rb;
    private InfoJugador _usernamePanel;

    [Header("Movimiento")]
    private Vector3 _moveDirection = new Vector3();
    public float moveSpeed = 10;
    public float speedLimit = 10;
    
    [Header("Gravedad")]
    public float gravityNormal = 50f;
    public float gravityJump = 9.81f;
    private bool _isJumping;

    [Header("Camara")]
    public Transform transformCam;
    public float camSpeed = 10f;
    private float _pitch = 0;
    private float _yaw = 0;
    private Vector2 _camInput = new Vector2();
    public float maxPitch = 60;
    public InputAction lookAction;

    [Header("Weapons")]
    public Transform transformCannon;

    [SyncVar(hook =nameof(OnKillChanged))]
    private int kills = 0;

    [Header("HP"), SyncVar(hook = nameof(HealthChanged))]
    private int hp = 5;
    private int maxHp;
    public Transform healthBar;
    [SyncVar(hook = nameof(AliveHasChanged))]
    private bool isAlive = true;

    public float respawnTime = 5;

    [Header("Hats")]
    public GameObject[] hats;
    [SerializeField]
    private GameObject hatSocket;
    [SyncVar(hook =nameof(SpawnHat))]
    private int hatIndex = 10;

    [Header("NameTag")]
    public TextMeshPro nameTagObject;
    [SyncVar(hook =nameof(NameChanged))]
    private string username;

    [Header("Team"), SyncVar(hook = nameof(OnChangeTeam))]
    private Teams myTeam = Teams.None;

    #endregion
    #region Unity
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        maxHp = hp;
    }
    
    void FixedUpdate()
    {
        if(!isLocalPlayer)return;
        Vector3 flat = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Quaternion orientation = Quaternion.LookRotation(flat);
        Vector3 worldMoveDirection = orientation * _moveDirection;
        _rb.AddForce(worldMoveDirection * moveSpeed, ForceMode.Impulse);

        Vector3 horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > speedLimit)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * speedLimit;
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.z);
        }

        if (!_isJumping)
        {
            _rb.AddForce(Vector3.down * gravityNormal, ForceMode.Acceleration);
        }

    }
    void Update()
    {
        //_camInput = lookAction.ReadValue<Vector2>();
        _camInput = Mouse.current.delta.ReadValue();
        _yaw += _camInput.x * camSpeed * Time.deltaTime;
        _pitch += _camInput.y * camSpeed * Time.deltaTime;

        _pitch = _pitch > maxPitch ? maxPitch : _pitch < (-maxPitch) ? -maxPitch : _pitch;
        transform.eulerAngles = new Vector3(0, _yaw,0);
        transformCam.eulerAngles = new Vector3(-_pitch, transformCam.rotation.eulerAngles.y, transformCam.rotation.eulerAngles.z);
    }
    #endregion
    #region PewPew
    [Command]
    private void CommandShoot(Vector3 origin,Vector3 direction )
    {
        if (Physics.Raycast(origin,direction, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject.TryGetComponent<Jugador>(out Jugador hitPlayer)==true)
            {
                

                if(hitPlayer.TakeDamage(1,myTeam))
                {
                    kills++;
                }
            }
        }
    }

    private void OnKillChanged(int oldKills, int newKills)
    {
        Debug.Log("ETC...");
    }

    [Server]
    public bool TakeDamage(int amount,Teams elTeam)
    {
        if (hp<=0|| elTeam ==myTeam)
        {
            hp = 0;
            return false;
        }
        hp -= amount;
        if (hp<=0)
        {
            KillPlayer();
            return true;
        }
        return false;
    }
    #endregion
    #region Hats
    [Command]
    private void SpawnHatCommand(int newHatIndex)
    {

        hatIndex = newHatIndex;

    }


    
    private void SpawnHat(int oldHat, int newHat)
    {
        
            GameObject hat = Instantiate(hats[newHat], hatSocket.transform);
        if (!isLocalPlayer)
        {
            return;
        }
        hat.SetActive(false);
        
        

    }

    

    #endregion


    #region HP
    private void HealthChanged(int oldHealth , int newHealth)
    {
        healthBar.localScale = new Vector3(healthBar.localScale.x, (float)newHealth / maxHp, healthBar.localScale.z);
    }
   

    [Server]
    private void KillPlayer()
    {
        isAlive = false;
    }

    private void AliveHasChanged(bool oldBool, bool newBool)
    {
        if (newBool==false)
        {
transform.localScale = new Vector3(1, 0.3f, 1);
        transformCam.gameObject.SetActive(false);
        gameObject.GetComponent<PlayerInput>().enabled = false;
        healthBar.gameObject.SetActive(false);
            if (!isLocalPlayer)
            {
                return;
            }
            Invoke("CommandRespawn", respawnTime);
        }
        else
        {
            transform.localScale = Vector3.one;
            healthBar.gameObject.SetActive(true);
            transform.position=ShooterNetworkManager.singleton.GetStartPosition().position;
            if(!isLocalPlayer)
            {
                return;
            }
            gameObject.GetComponent<PlayerInput>().enabled = true;
            transformCam.gameObject.SetActive(true);

        }
        
    }
    #endregion
    #region Input

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (!context.performed)
        {
            return;
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        Vector3 targetPoint;
        if(Physics.Raycast(ray,out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }

        Vector3 direction = (targetPoint - transformCannon.position).normalized;

        CommandShoot(transformCannon.position,direction);
    }

    public void SetMovement(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer)return;
        Debug.Log("Moving");
        var dir = context.ReadValue<Vector2>().normalized;
        _moveDirection = new Vector3(dir.x, 0, dir.y);
        
    }

    public void SetLookDirection(InputAction.CallbackContext context)
    {
        //_camInput = context.ReadValue<Vector2>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Cursor.lockState = CursorLockMode.Locked;
        _usernamePanel = GameObject.FindGameObjectWithTag("Username").GetComponent<InfoJugador>();
        name = _usernamePanel.PideUsuario();
        SpawnHatCommand(_usernamePanel.HatIndexRequest());
        CommandChangeName(_usernamePanel.PideUsuario());
        _usernamePanel.gameObject.SetActive(false);
       

    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        lookAction = playerInput.actions.actionMaps[0].actions[1];
        transformCam.gameObject.SetActive(true);
        nameTagObject.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
    }
    #endregion

    #region Mirror
    public override void OnStartServer()
    {
        base.OnStartServer();
        CommandSetTeam();
    }

    #endregion

    [Command]
    private void CommandChangeName(string myName)
    {
        username = myName;
    }

   private void NameChanged(string oldName, string newName)
    {
        nameTagObject.text = newName;
        name = newName;
    }

    [Command]
    private void CommandRespawn()
    {
        isAlive = true;
        hp = maxHp;
    }

    [Server]
    private void CommandSetTeam()
    {
        myTeam = TeamManager.singleton.GetBalancedTeam();
        TeamManager.singleton.RegisterPlayer(this, myTeam);
    }

    private void OnChangeTeam(Teams oldTeam, Teams newTeam)
    {
        SetLook(newTeam);
    }
    private void SetLook(Teams elTeam)
    {
        Debug.Log("Soy " + elTeam.ToString() + " gurl");
    }

    }
