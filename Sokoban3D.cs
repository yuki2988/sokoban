/*------------------------------------------------------------------------------------------------------------------
 * Sokoban3D.cs
 * 
 * �q�ɔԂ̃��C���X�N���v�g
 * �ϐ���}�e���A���ȊO�̏�����������Ă���
 * 
 * �쐬����: 2022/10�`11
 * �쐬��: �␣ �C��
 * ------------------------------------------------------------------------------------------------------------------
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sokoban3D : MonoBehaviour
{
    #region �O���f�[�^
    [SerializeField, Header("�O���f�[�^�������Ă���(�}�e���A��)")]
    MaterialData _materialData = default;
    [SerializeField, Header("�O���f�[�^�������Ă���(���b�V��)")]
    MeshData _meshData = default;
    [SerializeField, Header("�O���f�[�^�������Ă���(�ϐ�)")]
    ParaData _paraData = default;
    #endregion

    #region �ϐ�
    //�X�e�[�W�\�����L�q���ꂽ�e�L�X�g�t�@�C��
    [SerializeField, Header("�X�e�[�W���O����������")]
    private TextAsset[] _stageFile =default;

    [SerializeField, Header("�N���A�̃e�L�X�g����")]
    private GameObject _clearText;
    private GameObject _player;

    //�����X�e�[�W�̒����烉���_���őI�Ԏ��̒��I�p�ϐ�
    private int _randomInt = default;
    //���I�őI�΂ꂽ�X�e�[�W�̔z��ԍ�
    private int _stageInt = default;
    //�s��
    private int _line = default;
    //��
    private int columns = default;
    //�N���A�J�E���g�p�̃u���b�N�̐�
    private int _blockCount;

    //�^�C�������Ǘ�����񎟌��z��
    private TileType[,] _tileList;

    //���S�ʒu
    private Vector3 _middleOffset;
    
    //���݂̍��W
    private Vector3Int _currentPlayerPos = new Vector3Int();

    //�e���W�ɑ��݂���Q�[���I�u�W�F�N�g���Ǘ�����z��
    //���ꂼ��̃I�u�W�F�N�g���A���W�̏�������
    private Dictionary<GameObject, Vector3Int> _gameObjectPosTable = new Dictionary<GameObject, Vector3Int>();

    //�Q�[�����N���A�������ǂ���
    private bool _isFinish = default;

    //���O��ύX����p�̕ϐ���
    private string _objName = default;
    #endregion

    private enum TileType
    {
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
         * 0.�����Ȃ���ԁ@�@�@�@�@�@�@�@�@(NONE)
         * 1.�n�ʁi��������j              (GROUND)
         * 2.�ړI�n�i�u���b�N�̏I�_�j      (TARGET)
         * 3.�����v���C���[�̏ꏊ          (PLAYER)
         * 4.�����������u���b�N�̏ꏊ      (BLOCK)
         *-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
         * */
        NONE,//��
        GROUND,//�n��
        TARGET,//�ړI�n
        PLAYER,//�v���C���[
        BLOCK,//�u���b�N

        PLAYER_ABOVE_TARGET,//�v���C���[�i�ړI�n�̏�j
        BLOCK_ABOVE_TARGET //�u���b�N�i�ړI�n�̏�j
    }
    [SerializeField, Header("��Ԃ�0�`6�ŊǗ�")]
    TileType _tileType;

    // �����̎��
    private enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT,
    }
    [SerializeField, Header("���͂ɑ΂�������Ǘ�")]
    Direction _direction;

    
    private void Awake()
    {
        //�e�L�X�g�f�[�^��ǂݍ���
        LoadTileData();
        //�X�e�[�W���쐬
        CreateStage();
    }

    private void Update()
    {
        //����ł��Ȃ�����
        if (_isFinish)
        {
            if (_clearText.activeSelf == false)
            { 
                _clearText.SetActive(true); 
            }
            return;
        }

        //��
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) { MoveJudgement(Direction.UP); }
        //��
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) { MoveJudgement(Direction.DOWN); }
        //��
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) { MoveJudgement(Direction.LEFT); }      
        //�E
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { MoveJudgement(Direction.RIGHT); }
    }



    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>LoadTileData</para>
    /// 
    /// <para>�^�C���̏���ǂݍ���</para>
    /// </summary>
    private void LoadTileData()
    {
        _randomInt = _stageFile.Length;
        _stageInt = Mathf.Clamp(Random.Range(0, _randomInt), 0, _stageFile.Length);
        //�^�C���̏�����s���Ƃɕ����@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�󔒂��ȗ�����
        string[] lines = _stageFile[_stageInt].text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //���̐��̌v�Z                                      �ȗ��Ȃ�
        string[] nums = lines[0].Split(new char[] { ',' }, System.StringSplitOptions.None);
        //�^�C���̗񐔂ƍs����ێ�
        //��
        _line = lines.Length;
        Debug.LogError(_line);
        //�s
        columns = nums.Length;
        Debug.LogError(columns);

        //�^�C�����
        _tileList = new TileType[columns, _line];
        for (int z = 0; z < _line; z++)
        {
            string str = lines[z];
            //�J���}�ŋ�؂�@�@�@�@�@�@�@�@�@�@�@�ȗ��Ȃ�
            nums = str.Split(new char[] { ',' }, System.StringSplitOptions.None);
            for (int x = 0; x < columns; x++)
            {
                //�����𐔒l�ɕϊ�����
                _tileList[x, z] = (TileType)int.Parse(nums[x]);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///<para> CreateStage</para>
    /// 
    ///<para> �X�e�[�W�쐬</para>
    /// </summary>
    private void CreateStage()
    {
        //�X�e�[�W�̈ʒu���v�Z
        _middleOffset.x = (columns * _paraData.GetScale() * _paraData.GetObjCenter())
                            - (_paraData.GetScale() * _paraData.GetObjCenter());
        Debug.Log(_middleOffset.x);
        _middleOffset.y = _paraData.GetScale() * _paraData.GetObjCenter();
        Debug.Log(_middleOffset.y);
        _middleOffset.z = (_line * _paraData.GetScale() * _paraData.GetObjCenter())
                            - (_paraData.GetScale() * _paraData.GetObjCenter());
        Debug.Log(_middleOffset.z);

        //�n�ʁA�ړI�n�A�v���C���[�A�u���b�N�̂ǂꂩ�𔻒肵�A���ꂼ��Ɏw�肳�ꂽ�X�v���C�g��ݒ肷��
        for (int z = 0; z < _line; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                _tileType = _tileList[x, z];
                Debug.Log(_tileList[x, z]);

                //��
                if (_tileType == TileType.NONE)
                {
                    _objName = "wall" + z + "_" + x;
                    GameObject wall = new GameObject(_objName);
                    AddMeshes(wall);
                    wall.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    wall.GetComponent<MeshRenderer>().material = _materialData.WallMaterial();
                    wall.transform.position = GetDisplayPosition(x,_paraData.GetConstInt(), z);
                    continue;
                }

                //�n��
                _objName = "tile" + z + "_" + x;
                GameObject tile = new GameObject(_objName);
                tile.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                AddMeshes(tile);
                tile.GetComponent<MeshRenderer>().material = _materialData.GroundMaterial();
                tile.transform.position = GetDisplayPosition(x, -(_paraData.GetConstInt()), z);

                /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                 * 
                 * �����܂ł͋��ʕ���
                 * ���͒n�ʂɉ����v�f�̕���
                 * 
                 * -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                 * */


                //�ړI�n
                if (_tileType == TileType.TARGET)
                {
                    GameObject destination = new GameObject("destination");
                    AddMeshes(destination);
                    destination.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    destination.GetComponent<MeshRenderer>().material = _materialData.TargetMaterial();
                    destination.transform.position = GetDisplayPosition(x, 0 ,z);
                }

                //�v���C���[
                else if (_tileType == TileType.PLAYER)
                {
                    _player = new GameObject("Player");
                    AddMeshes(_player);
                    _player.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    _player.GetComponent<MeshRenderer>().material = _materialData.PlayerMaterial();
                    _player.transform.position = GetDisplayPosition(x, 0 ,z);
                    _gameObjectPosTable.Add(_player, new Vector3Int(x, 0,z));
                }

                //�u���b�N
                else if (_tileType == TileType.BLOCK)
                {
                    _blockCount++;
                    GameObject block = new GameObject("Block" + _blockCount);
                    AddMeshes(block);
                    block.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    block.GetComponent<MeshRenderer>().material = _materialData.BlockMaterial();
                    block.tag = "Block";
                    block.transform.position = GetDisplayPosition(x, 0,z);
                    _gameObjectPosTable.Add(block, new Vector3Int(x, 0,z));
                }
            }
        }
    }
    //-------------------------------------------------------------------------------------
    /// <summary>
    /// <para>AddMeshes</para>
    /// 
    /// <para>���b�V�����I�u�W�F�N�g�ɒǉ����郁�\�b�h</para>
    /// </summary>
    /// <param name="gameObject"></param>
    private void AddMeshes(GameObject gameObject)
    {
        //���b�V���ǉ�
        gameObject.AddComponent<MeshRenderer>();
    }
    //------------------------------------------------------------------------------------
    /// <summary>
    ///<para> GetDisplayPosition</para>
    /// 
    /// <para>�\���ʒu���v�Z���郁�\�b�h</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private Vector3 GetDisplayPosition(int x, int y, int z)
    {
        return new Vector3
        (
            x * _paraData.GetScale() - _middleOffset.x,
            y * _middleOffset.y,
            z * (-_paraData.GetScale()) + _middleOffset.z
        );
    }

    //---------------------------------------------------------------------------------------
    /// <summary>
    /// <para>GetGameObjectAtPosition</para>
    /// 
    /// <para>�Q�[���I�u�W�F�N�g��Ԃ����\�b�h</para>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject GetGameObjectAtPosition(Vector3Int position)
    {
        //Key��GameObject��\���AValue��Vector3Int��\��
        foreach (KeyValuePair<GameObject,Vector3Int> pair in _gameObjectPosTable)
        {
            if (pair.Value == position)
            {
                return pair.Key;
            }
        }
        return null;
    }

    //-------------------------------------------------------------------------------------
    /// <summary>
    /// <para>IsValidPosition</para>
    /// 
    /// <para>�w�肳�ꂽ�ʒu���X�e�[�W�����ǂ���</para>
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsValidPosition(Vector3Int pos)
    {
        //�s�ԍ��Ɨ�ԍ��Ō��Ă���
        if (0 <= pos.x && pos.x < columns && 0 <= pos.z && pos.z< _line)
        {
            if (_tileList[pos.x, pos.z] != TileType.NONE) { return true; }
        }
        return false;
    }

    //--------------------------------------------------------------------------------------
    /// <summary>
    /// <para>IsBlock</para>
    /// 
    /// <para>�^�C�����u���b�N���ǂ���</para>
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsBlock(Vector3Int pos)
    {
        _tileType = _tileList[pos.x, pos.z];
        return _tileType == TileType.BLOCK || _tileType == TileType.BLOCK_ABOVE_TARGET;
    }

    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// <para>MoveJudgement</para>
    /// 
    /// <para>�w�肵�������Ƀv���C���[���ړ��ł��邩�ǂ���</para>
    /// </summary>
    /// <param name="direction"></param>
    private void MoveJudgement(Direction direction)
    {
        _currentPlayerPos = _gameObjectPosTable[_player];
        Vector3Int nextPlayerPos = GetNextPosition(_currentPlayerPos, direction);

        //�X�e�[�W�O
        if (!IsValidPosition(nextPlayerPos)){ return; }

        //�u���b�N�����݂���ꍇ
        if (IsBlock(nextPlayerPos))
        {
            Vector3Int nextBlockPos = GetNextPosition(nextPlayerPos, direction);

            //�u���b�N�̈ړ��悪�X�e�[�W�����u���b�N�����݂��Ȃ��Ƃ�
            //�܂�u���b�N���ړ��ł���Ƃ�
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                //�ړ�����u���b�N���擾
                GameObject Block = GetGameObjectAtPosition(nextPlayerPos);

                //�v���C���[�̈ړ���̃^�C���̏����X�V
                UpdatePosition(nextPlayerPos);

                //�ړ�����
                Block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y,nextBlockPos.z);

                //�u���b�N�̈ʒu���X�V
                _gameObjectPosTable[Block] = nextBlockPos;



                //�u���b�N�̈ړ�����X�V
                if (_tileList[nextBlockPos.x, nextBlockPos.z] == TileType.GROUND)
                {
                    //�ړ��悪�n�ʂȂ�u���b�N�̔ԍ��ɍX�V
                    _tileList[nextBlockPos.x, nextBlockPos.z] = TileType.BLOCK;
                }

                else if (_tileList[nextBlockPos.x, nextBlockPos.z] == TileType.TARGET)
                {
                    //�ړ��悪�ړI�n�Ȃ�u���b�N�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tileList[nextBlockPos.x, nextBlockPos.z] = TileType.BLOCK_ABOVE_TARGET;
                }

                //�v���C���[�̂��X�V
                UpdatePosition(_currentPlayerPos);
                //�v���C���[���ړ�
                _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y,nextPlayerPos.z);
                //�v���C���[�̈ʒu���X�V
                _gameObjectPosTable[_player] = nextPlayerPos;

                //�v���C���[�̈ړ���̔ԍ����X�V
                if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.GROUND)
                {
                    //�ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                    _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER;
                }
                else if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.TARGET)
                {
                    //�ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER_ABOVE_TARGET;
                }
            }
        }

        //�v���C���[�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
        else
        {
            //�v���C���[�̌��ݒn�̃^�C���̏����X�V
            UpdatePosition(_currentPlayerPos);
            _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y,nextPlayerPos.z);
            //�v���C���[�̈ʒu���X�V
            _gameObjectPosTable[_player] = nextPlayerPos;

            //�v���C���[�̈ړ���̔ԍ����X�V
            if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.GROUND)
            {
                //�ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER;
            }
            else if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.TARGET)
            {
                //�ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER_ABOVE_TARGET;
            }
        }

        //�Q�[�����N���A�������ǂ����m�F
        CheckCompletion();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>GetNextPosition</para>
    /// 
    /// <para>�w�肳�ꂽ�����̈ʒu��Ԃ�</para>
    /// </summary>
    /// <param name="posision"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3Int GetNextPosition(Vector3Int posision, Direction direction)
    {
        switch (direction)
        {
            // ��
            case Direction.UP:
                posision.z -= _paraData.GetConstInt();
                break;

            // �E
            case Direction.RIGHT:
                posision.x += _paraData.GetConstInt();
                break;

            // ��
            case Direction.DOWN:
                posision.z += _paraData.GetConstInt();
                break;

            // ��
            case Direction.LEFT:
                posision.x -= _paraData.GetConstInt();
                break;
        }
        return posision;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>UpdatePosition</para>
    /// 
    /// <para>�w�肳�ꂽ�ʒu�̃^�C�����X�V</para>
    /// </summary>
    /// <param name="pos"></param>
    private void UpdatePosition(Vector3Int pos)
    {
        //�w�肳�ꂽ�ʒu�̃^�C���̔ԍ����擾
        _tileType = _tileList[pos.x, pos.z];

        //�v���C���[�܂��̓u���b�N�̏ꍇ
        if (_tileType == TileType.PLAYER || _tileType == TileType.BLOCK)
        {
            //�n�ʂɕύX
            _tileList[pos.x, pos.z] = TileType.GROUND;
        }

        //�ړI�n�ɏ���Ă���v���C���[�������̓u���b�N�̏ꍇ
        else if (_tileType == TileType.PLAYER_ABOVE_TARGET || _tileType == TileType.BLOCK_ABOVE_TARGET)
        {
            // �ړI�n�ɕύX
            _tileList[pos.x, pos.z] = TileType.TARGET;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>CheckCompletion</para>
    /// 
    /// <para>�N���A�`�F�b�N</para>
    /// </summary>
    private void CheckCompletion()
    {
        int Count = 0;
        for (int z = 0; z < _line; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (_tileList[x, z] == TileType.BLOCK_ABOVE_TARGET) { Count++; }
            }
        }

        //���ׂẴu���b�N��TARGET�̏�ɏ���Ă��邩�ǂ���
        if (Count == _blockCount)
        {
            //�Q�[���N���A�t���O���I��
            _isFinish = true;
        }
    }


    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///<para> Reset</para>
    /// 
    /// <para>�Q�[������蒼��</para>
    /// </summary>
    public void Reset()
    {
        //�V�[����ǂݒ���
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
