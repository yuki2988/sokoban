/*------------------------------------------------------------------------------------------------------------------
 * Sokoban3D.cs
 * 
 * 倉庫番のメインスクリプト
 * 変数やマテリアル以外の処理が書かれている
 * 
 * 作成期間: 2022/10～11
 * 作成者: 岩瀬 佑希
 * ------------------------------------------------------------------------------------------------------------------
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sokoban3D : MonoBehaviour
{
    #region 外部データ
    [SerializeField, Header("外部データを持ってくる(マテリアル)")]
    MaterialData _materialData = default;
    [SerializeField, Header("外部データを持ってくる(メッシュ)")]
    MeshData _meshData = default;
    [SerializeField, Header("外部データを持ってくる(変数)")]
    ParaData _paraData = default;
    #endregion

    #region 変数
    //ステージ構造が記述されたテキストファイル
    [SerializeField, Header("ステージを外部から入れる")]
    private TextAsset[] _stageFile =default;

    [SerializeField, Header("クリアのテキストを代入")]
    private GameObject _clearText;
    private GameObject _player;

    //既存ステージの中からランダムで選ぶ時の抽選用変数
    private int _randomInt = default;
    //抽選で選ばれたステージの配列番号
    private int _stageInt = default;
    //行数
    private int _line = default;
    //列数
    private int columns = default;
    //クリアカウント用のブロックの数
    private int _blockCount;

    //タイル情報を管理する二次元配列
    private TileType[,] _tileList;

    //中心位置
    private Vector3 _middleOffset;
    
    //現在の座標
    private Vector3Int _currentPlayerPos = new Vector3Int();

    //各座標に存在するゲームオブジェクトを管理する配列
    //それぞれのオブジェクトが、座標の情報を持つ
    private Dictionary<GameObject, Vector3Int> _gameObjectPosTable = new Dictionary<GameObject, Vector3Int>();

    //ゲームをクリアしたかどうか
    private bool _isFinish = default;

    //名前を変更する用の変数名
    private string _objName = default;
    #endregion

    private enum TileType
    {
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
         * 0.何もない状態　　　　　　　　　(NONE)
         * 1.地面（床がある）              (GROUND)
         * 2.目的地（ブロックの終点）      (TARGET)
         * 3.初期プレイヤーの場所          (PLAYER)
         * 4.動かす初期ブロックの場所      (BLOCK)
         *-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
         * */
        NONE,//無
        GROUND,//地面
        TARGET,//目的地
        PLAYER,//プレイヤー
        BLOCK,//ブロック

        PLAYER_ABOVE_TARGET,//プレイヤー（目的地の上）
        BLOCK_ABOVE_TARGET //ブロック（目的地の上）
    }
    [SerializeField, Header("状態を0～6で管理")]
    TileType _tileType;

    // 方向の種類
    private enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT,
    }
    [SerializeField, Header("入力に対する方向管理")]
    Direction _direction;

    
    private void Awake()
    {
        //テキストデータを読み込む
        LoadTileData();
        //ステージを作成
        CreateStage();
    }

    private void Update()
    {
        //操作できない条件
        if (_isFinish)
        {
            if (_clearText.activeSelf == false)
            { 
                _clearText.SetActive(true); 
            }
            return;
        }

        //上
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) { MoveJudgement(Direction.UP); }
        //下
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) { MoveJudgement(Direction.DOWN); }
        //左
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) { MoveJudgement(Direction.LEFT); }      
        //右
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { MoveJudgement(Direction.RIGHT); }
    }



    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>LoadTileData</para>
    /// 
    /// <para>タイルの情報を読み込む</para>
    /// </summary>
    private void LoadTileData()
    {
        _randomInt = _stageFile.Length;
        _stageInt = Mathf.Clamp(Random.Range(0, _randomInt), 0, _stageFile.Length);
        //タイルの情報を一行ごとに分割　　　　　　　　　　　　　　　　　　　　　　　空白を省略する
        string[] lines = _stageFile[_stageInt].text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //横の数の計算                                      省略なし
        string[] nums = lines[0].Split(new char[] { ',' }, System.StringSplitOptions.None);
        //タイルの列数と行数を保持
        //列
        _line = lines.Length;
        Debug.LogError(_line);
        //行
        columns = nums.Length;
        Debug.LogError(columns);

        //タイル情報
        _tileList = new TileType[columns, _line];
        for (int z = 0; z < _line; z++)
        {
            string str = lines[z];
            //カンマで区切る　　　　　　　　　　　省略なし
            nums = str.Split(new char[] { ',' }, System.StringSplitOptions.None);
            for (int x = 0; x < columns; x++)
            {
                //文字を数値に変換する
                _tileList[x, z] = (TileType)int.Parse(nums[x]);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///<para> CreateStage</para>
    /// 
    ///<para> ステージ作成</para>
    /// </summary>
    private void CreateStage()
    {
        //ステージの位置を計算
        _middleOffset.x = (columns * _paraData.GetScale() * _paraData.GetObjCenter())
                            - (_paraData.GetScale() * _paraData.GetObjCenter());
        Debug.Log(_middleOffset.x);
        _middleOffset.y = _paraData.GetScale() * _paraData.GetObjCenter();
        Debug.Log(_middleOffset.y);
        _middleOffset.z = (_line * _paraData.GetScale() * _paraData.GetObjCenter())
                            - (_paraData.GetScale() * _paraData.GetObjCenter());
        Debug.Log(_middleOffset.z);

        //地面、目的地、プレイヤー、ブロックのどれかを判定し、それぞれに指定されたスプライトを設定する
        for (int z = 0; z < _line; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                _tileType = _tileList[x, z];
                Debug.Log(_tileList[x, z]);

                //無
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

                //地面
                _objName = "tile" + z + "_" + x;
                GameObject tile = new GameObject(_objName);
                tile.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                AddMeshes(tile);
                tile.GetComponent<MeshRenderer>().material = _materialData.GroundMaterial();
                tile.transform.position = GetDisplayPosition(x, -(_paraData.GetConstInt()), z);

                /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                 * 
                 * ここまでは共通部分
                 * 下は地面に加わる要素の分岐
                 * 
                 * -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                 * */


                //目的地
                if (_tileType == TileType.TARGET)
                {
                    GameObject destination = new GameObject("destination");
                    AddMeshes(destination);
                    destination.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    destination.GetComponent<MeshRenderer>().material = _materialData.TargetMaterial();
                    destination.transform.position = GetDisplayPosition(x, 0 ,z);
                }

                //プレイヤー
                else if (_tileType == TileType.PLAYER)
                {
                    _player = new GameObject("Player");
                    AddMeshes(_player);
                    _player.AddComponent<MeshFilter>().mesh = _meshData.CubeMesh();
                    _player.GetComponent<MeshRenderer>().material = _materialData.PlayerMaterial();
                    _player.transform.position = GetDisplayPosition(x, 0 ,z);
                    _gameObjectPosTable.Add(_player, new Vector3Int(x, 0,z));
                }

                //ブロック
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
    /// <para>メッシュをオブジェクトに追加するメソッド</para>
    /// </summary>
    /// <param name="gameObject"></param>
    private void AddMeshes(GameObject gameObject)
    {
        //メッシュ追加
        gameObject.AddComponent<MeshRenderer>();
    }
    //------------------------------------------------------------------------------------
    /// <summary>
    ///<para> GetDisplayPosition</para>
    /// 
    /// <para>表示位置を計算するメソッド</para>
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
    /// <para>ゲームオブジェクトを返すメソッド</para>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject GetGameObjectAtPosition(Vector3Int position)
    {
        //KeyはGameObjectを表し、ValueはVector3Intを表す
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
    /// <para>指定された位置がステージ内かどうか</para>
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsValidPosition(Vector3Int pos)
    {
        //行番号と列番号で見ている
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
    /// <para>タイルがブロックかどうか</para>
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
    /// <para>指定した方向にプレイヤーが移動できるかどうか</para>
    /// </summary>
    /// <param name="direction"></param>
    private void MoveJudgement(Direction direction)
    {
        _currentPlayerPos = _gameObjectPosTable[_player];
        Vector3Int nextPlayerPos = GetNextPosition(_currentPlayerPos, direction);

        //ステージ外
        if (!IsValidPosition(nextPlayerPos)){ return; }

        //ブロックが存在する場合
        if (IsBlock(nextPlayerPos))
        {
            Vector3Int nextBlockPos = GetNextPosition(nextPlayerPos, direction);

            //ブロックの移動先がステージ内かつブロックが存在しないとき
            //つまりブロックが移動できるとき
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                //移動するブロックを取得
                GameObject Block = GetGameObjectAtPosition(nextPlayerPos);

                //プレイヤーの移動先のタイルの情報を更新
                UpdatePosition(nextPlayerPos);

                //移動処理
                Block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y,nextBlockPos.z);

                //ブロックの位置を更新
                _gameObjectPosTable[Block] = nextBlockPos;



                //ブロックの移動先を更新
                if (_tileList[nextBlockPos.x, nextBlockPos.z] == TileType.GROUND)
                {
                    //移動先が地面ならブロックの番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.z] = TileType.BLOCK;
                }

                else if (_tileList[nextBlockPos.x, nextBlockPos.z] == TileType.TARGET)
                {
                    //移動先が目的地ならブロック（目的地の上）の番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.z] = TileType.BLOCK_ABOVE_TARGET;
                }

                //プレイヤーのを更新
                UpdatePosition(_currentPlayerPos);
                //プレイヤーを移動
                _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y,nextPlayerPos.z);
                //プレイヤーの位置を更新
                _gameObjectPosTable[_player] = nextPlayerPos;

                //プレイヤーの移動先の番号を更新
                if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.GROUND)
                {
                    //移動先が地面ならプレイヤーの番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER;
                }
                else if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.TARGET)
                {
                    //移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER_ABOVE_TARGET;
                }
            }
        }

        //プレイヤーの移動先にブロックが存在しない場合
        else
        {
            //プレイヤーの現在地のタイルの情報を更新
            UpdatePosition(_currentPlayerPos);
            _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y,nextPlayerPos.z);
            //プレイヤーの位置を更新
            _gameObjectPosTable[_player] = nextPlayerPos;

            //プレイヤーの移動先の番号を更新
            if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.GROUND)
            {
                //移動先が地面ならプレイヤーの番号に更新
                _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER;
            }
            else if (_tileList[nextPlayerPos.x, nextPlayerPos.z] == TileType.TARGET)
            {
                //移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                _tileList[nextPlayerPos.x, nextPlayerPos.z] = TileType.PLAYER_ABOVE_TARGET;
            }
        }

        //ゲームをクリアしたかどうか確認
        CheckCompletion();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>GetNextPosition</para>
    /// 
    /// <para>指定された方向の位置を返す</para>
    /// </summary>
    /// <param name="posision"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3Int GetNextPosition(Vector3Int posision, Direction direction)
    {
        switch (direction)
        {
            // 上
            case Direction.UP:
                posision.z -= _paraData.GetConstInt();
                break;

            // 右
            case Direction.RIGHT:
                posision.x += _paraData.GetConstInt();
                break;

            // 下
            case Direction.DOWN:
                posision.z += _paraData.GetConstInt();
                break;

            // 左
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
    /// <para>指定された位置のタイルを更新</para>
    /// </summary>
    /// <param name="pos"></param>
    private void UpdatePosition(Vector3Int pos)
    {
        //指定された位置のタイルの番号を取得
        _tileType = _tileList[pos.x, pos.z];

        //プレイヤーまたはブロックの場合
        if (_tileType == TileType.PLAYER || _tileType == TileType.BLOCK)
        {
            //地面に変更
            _tileList[pos.x, pos.z] = TileType.GROUND;
        }

        //目的地に乗っているプレイヤーもしくはブロックの場合
        else if (_tileType == TileType.PLAYER_ABOVE_TARGET || _tileType == TileType.BLOCK_ABOVE_TARGET)
        {
            // 目的地に変更
            _tileList[pos.x, pos.z] = TileType.TARGET;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <para>CheckCompletion</para>
    /// 
    /// <para>クリアチェック</para>
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

        //すべてのブロックがTARGETの上に乗っているかどうか
        if (Count == _blockCount)
        {
            //ゲームクリアフラグをオン
            _isFinish = true;
        }
    }


    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///<para> Reset</para>
    /// 
    /// <para>ゲームをやり直す</para>
    /// </summary>
    public void Reset()
    {
        //シーンを読み直す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
