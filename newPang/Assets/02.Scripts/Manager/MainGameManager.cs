using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum GameType
{
    Battle,
    CoOp
}
public class MainGameManager : SingletonClass<MainGameManager>
{
    public PhotonView pv;
    bool host;

    public GameType gameType;

    StageBuilder stageBuilder;
    public Stage stage;
    public Board board;

    public Text comboText;

    int m_Combo;

    public Transform playerLocator;
    public Transform otherPlayerLocator;

    public CharacterBattle playerCb;
    public CharacterBattle otherPlayerCb;

    public int combo
    {
        get { return m_Combo; }
        set
        {
            CancelInvoke("BreakCombo");
            if (value > 0)
            {
                Invoke("BreakCombo", 2f);
            }

            m_Combo = value;

            if (comboText != null)
            {
                comboText.text = m_Combo.ToString();
            }

            if (gameType == GameType.Battle)
            {
                if (m_Combo > 0 && m_Combo % 10 == 0)
                {
                    while (true)
                    {
                        int row = Random.Range(0, board.maxRow);
                        int col = Random.Range(0, board.maxCol);

                        if (!board.blocks[row, col].IsMatchable(true) || board.blocks[row, col].type != BlockType.BASIC)
                        {
                            continue;
                        }
                        else
                        {
                            board.blocks[row, col].SetQuestType(BlockQuestType.SHUFFLE);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void BreakCombo()
    {
        combo = 0;

    }

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();

        ObjectPoolManager.GetInstance.LoadAllGameObject();
        ObjectPoolManager.GetInstance.InitializeObjPool("Block", 1, 1, 200);
        ObjectPoolManager.GetInstance.InitializeObjPool("Cell", 1, 1, 100);

        switch (gameType)
        {
            case GameType.Battle:
                BuildStage(1);
                break;
            case GameType.CoOp:
                if (PhotonNetwork.IsMasterClient)
                {
                    host = true;
                    BuildStage(1);
                }
                break;
            default:
                break;
        }

    }

    void BuildStage(int stageNumber)
    {
        stageBuilder = new StageBuilder(stageNumber); // 스테이지 번호
        stageBuilder.ComposeStage(stage);

        board.SetCellPosition();
        board.MatchingCheckShuffle();

        while (!board.PangCheck())
        {
            board.AllShuffle();
            board.MatchingCheckShuffle();
        }
    }

    public void PlayerCharacterCreate()
    {
        //DataManager.GetInstance.ChangeCharacter(DataManager.GetInstance.userData.customs["character"], playerLocator);

        GameObject go = PhotonManager.GetInstance.InstantiateWithPhoton(playerLocator.gameObject, DataManager.GetInstance.userData.customs["character"], Vector3.zero);

        CharacterBase cb = go.GetComponent<CharacterBase>();
        playerCb = cb.gameObject.AddComponent<CharacterBattle>();
        cb.pv.RPC("EquipAccessory", RpcTarget.All, DataManager.GetInstance.userData.customs["face"]);
        cb.pv.RPC("EquipAccessory", RpcTarget.All, DataManager.GetInstance.userData.customs["hand"]);
        cb.pv.RPC("EquipAccessory", RpcTarget.All, DataManager.GetInstance.userData.customs["bag"]);
        cb.pv.RPC("EquipAccessory", RpcTarget.All, DataManager.GetInstance.userData.customs["head"]);
        cb.pv.RPC("EquipAccessory", RpcTarget.All, DataManager.GetInstance.userData.customs["etc"]);

        cb.transform.SetParent(playerLocator);
        cb.InitTransform();

        cb.pv.RPC("SetOtherPlayer", RpcTarget.Others);
    }

    public bool IsHost()
    {
        return host;
    }

    public bool IsCoOpHost()
    {
        return gameType == GameType.CoOp && IsHost();
    }

    public bool IsCoOpRemote()
    {
        return gameType == GameType.CoOp && !IsHost();
    }
}
