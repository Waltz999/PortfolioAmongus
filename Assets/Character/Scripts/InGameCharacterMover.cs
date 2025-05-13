using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static UnityEngine.GraphicsBuffer;

public enum EPlayerType
{
    Crew = 0,
    Imposter = 1,
    Ghost = 2,
    Crew_Alive = 0,
    Imposter_Alive = 1,
    Crew_Ghost = 2,
    Imposter_Ghost = 3
}

public class InGameCharacterMover : CharacterMover
{
    [SyncVar(hook = nameof(SetPlayerType_Hook))]
    public EPlayerType playerType;
    private void SetPlayerType_Hook(EPlayerType _, EPlayerType type)
    {
        if(hasAuthority && type == EPlayerType.Imposter)
        {
            InGameUIManager.Instance.KillButtonUI.Show(this);
            playerFinder.SetKillRange(GameSystem.Instance.killRange + 1f);
        }
    }

    [SerializeField]
    private PlayerFinder playerFinder;

    [SyncVar]
    private float killCooldown;
    public float KillCooldown { get { return killCooldown; } }
    public bool isKillable { get { return killCooldown < 0f && playerFinder.targets.Count != 0; } }
    public EPlayerColor foundDeadbodyColor;

    [SyncVar]
    public bool isReporter = false;

    [SyncVar]
    public bool isVote;

    [SyncVar]
    public int vote;

    [ClientRpc]
    public void RpcTeleport(Vector3 position)
    {
        transform.position = position;
    }

    public void SetNicknameColor(EPlayerType type)
    {
        if(playerType == EPlayerType.Imposter && type == EPlayerType.Imposter)
        {
            nicknameText.color = Color.red;
        }
    }

    public void SetKillCooldown()
    {
        if(isServer)
        {
            killCooldown = GameSystem.Instance.killCooldown;
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        if(hasAuthority)
        {
            IsMoveable = true;

            var myRoomPlayer = AmongUsRoomPlayer.MyRoomPlayer;

            myRoomPlayer.myCharacter = this;

            CmdSetPlayerCharacter(myRoomPlayer.nickname, myRoomPlayer.playerColor);
        }

        GameSystem.Instance.AddPlayer(this);
    }

    private void Update()
    {
        if(isServer && playerType == EPlayerType.Imposter)
        {
            killCooldown -= Time.deltaTime;
        }
    }

    [Command]
    private void CmdSetPlayerCharacter(string nickname, EPlayerColor color)
    {
        this.nickname = nickname;
        playerColor = color;
    }

    public void Kill()
    {
        CmdKill(playerFinder.GetFirstTarget().netId);
    }

    [Command]
    private void CmdKill(uint targetNetId)
    {
        InGameCharacterMover target = null;
        foreach(var player in GameSystem.Instance.GetPlayerList())
        {
            if(player.netId == targetNetId)
            {
                target = player;
            }
        }

        if(target != null)
        {
            RpcTeleport(target.transform.position);
            target.Dead(playerColor);
            killCooldown = GameSystem.Instance.killCooldown;
        }
    }

    public void Dead(EPlayerColor imposterColor)
    {
        playerType = EPlayerType.Ghost;
        RpcDead(imposterColor, playerColor);
        var manager = NetworkRoomManager.singleton as AmongUsRoomManager;
        var deadbody = Instantiate(manager.spawnPrefabs[1], transform.position, transform.rotation).GetComponent<DeadBody>();
        NetworkServer.Spawn(deadbody.gameObject);
        deadbody.RpcSetColor(playerColor);
    }

    [ClientRpc]
    private void RpcDead(EPlayerColor imposterColor, EPlayerColor crewColor)
    {
        if(hasAuthority)
        {
            animator.SetBool("isGhost", true);
            InGameUIManager.Instance.KillUI.Open(imposterColor, crewColor);

            var players = GameSystem.Instance.GetPlayerList();
            foreach(var player in players)
            {
                if((player.playerType & EPlayerType.Ghost) == EPlayerType.Ghost)
                {
                    player.SetVisibility(true);
                }
            }

            GameSystem.Instance.ChangeLightMode(EPlayerType.Ghost);
        }
        else
        {
            var myPlayer = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
            if(((int)myPlayer.playerType & 0x02) != (int)EPlayerType.Ghost)
            {
                SetVisibility(false);
            }
        }

        var collider = GetComponent<BoxCollider2D>();
        if (collider)
        {
            collider.enabled = false;
        }
    }

    public void Report()
    {
        CmdReport(foundDeadbodyColor);
    }

    [Command]
    public void CmdReport(EPlayerColor deadbodyColor)
    {
        isReporter = true;
        GameSystem.Instance.StartReportMeeting(deadbodyColor);
    }

    public void SetVisibility(bool isVisible)
    {
        if(isVisible)
        {
            var color = PlayerColor.GetColor(playerColor);
            color.a = 1f;
            spriteRenderer.material.SetColor("_PlayerColor", color);
            nicknameText.text = nickname;
        }
        else
        {
            var color = PlayerColor.GetColor(playerColor);
            color.a = 0f;
            spriteRenderer.material.SetColor("_PlayerColor", color);
            nicknameText.text = "";
        }
    }

    [Command]
    public void CmdVoteEjectPlayer(EPlayerColor ejectColor)
    {
        isVote = true;
        GameSystem.Instance.RpcSignVoteEject(playerColor, ejectColor);

        var players = FindObjectsOfType<InGameCharacterMover>();
        InGameCharacterMover ejectedPlayer = null;
        for(int i = 0; i < players.Length; i++)
        {
            if (players[i].playerColor == ejectColor)
            {
                ejectedPlayer = players[i];
            }
        }
        ejectedPlayer.vote += 1;
    }

    [Command]
    public void CmdSkipVote()
    {
        isVote = true;
        GameSystem.Instance.skipVotePlayerCount += 1;
        GameSystem.Instance.RpcSignSkipVote(playerColor); 
    }
}
