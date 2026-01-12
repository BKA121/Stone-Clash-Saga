using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MatchType
{
    None, Match3, Match4, Match5, TorLShape
}

public class StoneManager : MonoBehaviour
{
    public GameObject redDiamonPrefab, blueDiamonPrefab, greenDiamonPrefab,
                      purpleDiamonPrefab, yellowDiamonPrefab, icePrefab, stoneMatch5Prefab,
                      redMatch4Prefab, blueMatch4Prefab, greenMatch4Prefab, purpleMatch4Prefab, yellowMatch4Prefab,
                      redMatchTorLPrefab, blueMatchTorLPrefab, greenMatchTorLPrefab, purpleMatchTorLPrefab, yellowMatchTorLPrefab;

    public FirestoreReader firestoreReader;
    public StonePoolManager stonePoolManager;
    private LevelData levalData;
    private PathCaculator pathCaculator;
    public StoneBehaviour[,] boardStone;
    public TargetUIHandler uiHandler;
    public Sprite[] allStoneSprites;
    public Transform stoneContainer;
    public List<StoneType> normalType;

    public int row, column;
    public int curMove;
    public Dictionary<string, int> targetList;
    public int countMatch = 0;
    public int countStoneDestroy = 0;
    public static bool startFind = false;
    public bool isExecuteBomb = false;
    public bool isProcessing = false;
    public int countStoneFallOrSlide = 0;

    public void Init(LevelData data)
    {
        this.levalData = data;
        this.row = data.row;
        this.column = data.column;
        this.targetList = data.targetDict;
        this.curMove = data.moves;
        uiHandler.InitializeUI(targetList, allStoneSprites, curMove);
        boardStone = new StoneBehaviour[row, column];
        pathCaculator = new PathCaculator(boardStone, row, column);
    }

    // Dang ky vao board
    public void RegisterStone(StoneBehaviour stone, int row, int col)
    {
        boardStone[row, col] = stone;
    }

    // Thoat dang ky de roi
    public void UnRegisterStone(int row, int col)
    {
        boardStone[row, col] = null;
    }

    public async Task SpawnStone(int row, int column, List<(int x, int y)> positionBlockList, List<string> ruleList)
    {
        List<StoneType> stoneList = null;
        List<StoneType> stoneSpecialList = null;
        foreach (string rule in ruleList)
        {
            switch (rule)
            {
                case "spawn3Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_NormalType("spawn3Type");
                    stoneSpecialList = await firestoreReader.LoadRuleSpawn_x_SpecialType("spawn3Type");
                    break;
                case "spawn4Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_NormalType("spawn4Type");
                    stoneSpecialList = await firestoreReader.LoadRuleSpawn_x_SpecialType("spawn4Type");
                    break;
                case "spawn5Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_NormalType("spawn5Type");
                    stoneSpecialList = await firestoreReader.LoadRuleSpawn_x_SpecialType("spawn5Type");
                    break;
            }
        }
        if (stoneList != null && stoneSpecialList != null)
        {
            normalType = new List<StoneType>(stoneList);
            // Khoi tao pool
            Dictionary<StoneType, GameObject> stonePrefab = new Dictionary<StoneType, GameObject>();
            foreach (var i in stoneList)
            {
                stonePrefab[i] = GetStonePrefabByType(i);
            }
            stonePoolManager.InitPools(stonePrefab, 60);

            Dictionary<StoneType, GameObject> stonePrefabSpecial = new Dictionary<StoneType, GameObject>();
            foreach (var i in stoneSpecialList)
            {
                stonePrefabSpecial[i] = GetStonePrefabByType(i);
            }
            stonePoolManager.InitPools(stonePrefabSpecial, 7);

            SpawnStoneForNewGame(row, column, positionBlockList, stoneList);
        }
    }

    public GameObject GetStonePrefabByType(StoneType type)
    {
        switch (type)
        {
            case StoneType.Red: return redDiamonPrefab;
            case StoneType.Blue: return blueDiamonPrefab;
            case StoneType.Green: return greenDiamonPrefab;
            case StoneType.Purple: return purpleDiamonPrefab;
            case StoneType.Yellow: return yellowDiamonPrefab;

            case StoneType.RedMatch4: return redMatch4Prefab;
            case StoneType.BlueMatch4: return blueMatch4Prefab;
            case StoneType.GreenMatch4: return greenMatch4Prefab;
            case StoneType.PurpleMatch4: return purpleMatch4Prefab;
            case StoneType.YellowMatch4: return yellowMatch4Prefab;

            case StoneType.RedMatchTorL: return redMatchTorLPrefab;
            case StoneType.BlueMatchTorL: return blueMatchTorLPrefab;
            case StoneType.GreenMatchTorL: return greenMatchTorLPrefab;
            case StoneType.PurpleMatchTorL: return purpleMatchTorLPrefab;
            case StoneType.YellowMatchTorL: return yellowMatchTorLPrefab;

            case StoneType.StoneMatch5: return stoneMatch5Prefab;
            default: return null;
        }
    }

    public Vector2 UpdatePositionStone(int c, int r)
    {
        float cellSize = 100f;
        float offset = cellSize / 2f;
        float finalX = (c * cellSize) + offset;
        float finalY = (r * cellSize) + offset;
        return new Vector2(finalX, finalY);
    }

    public void SpawnStoneForNewGame(int row, int column, List<(int x, int y)> positionBlockList, List<StoneType> stoneList)
    {
        foreach (var pos in positionBlockList)
        {
            Vector2 posIce = UpdatePositionStone(pos.x, pos.y);
            GameObject ice = Instantiate(icePrefab, stoneContainer);
            ice.GetComponent<RectTransform>().anchoredPosition = posIce;
            StoneBehaviour stone = ice.GetComponent<StoneBehaviour>();
            stone.c = pos.x;
            stone.r = pos.y;
            RegisterStone(stone, pos.y, pos.x);

        }

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (boardStone[r, c] != null) continue;
                List<StoneType> availableStone = new List<StoneType>(stoneList);
                StoneType type = StoneType.Red;
                while (availableStone.Count > 0)
                {
                    int index = Random.Range(0, availableStone.Count);
                    type = availableStone[index];
                    if (!PreventInitialMatch3(r, c, type))
                    {
                        availableStone.RemoveAt(index);
                    }
                    else break;
                }
                Vector2 position = UpdatePositionStone(c, r);
                GameObject stone = stonePoolManager.GetStoneByType(type, position, c, r);
                RegisterStone(stone.GetComponent<StoneBehaviour>(), r, c);
            }
        }
    }

    public bool PreventInitialMatch3(int i, int j, StoneType type)
    {
        if (i < 2 && j < 2) return true;
        if (j >= 2 && boardStone[i, j - 1].stoneType == type
                   && boardStone[i, j - 2].stoneType == type) return false;
        if (i >= 2 && boardStone[i - 1, j].stoneType == type
                   && boardStone[i - 2, j].stoneType == type) return false;
        return true;
    }

    public void Update()
    {
        if (isProcessing) return;

        if (startFind) StartCoroutine(BoardHandling());
    }

    public IEnumerator BoardHandling()
    {
        isProcessing = true; 
        do
        {
            if (!isExecuteBomb)
            {
                // Tim tat ca match
                List<MatchGroup> matches = FindAllMatches();
                countMatch = matches.Count;
                if (countMatch == 0) break;

                // Xu ly toan bo match
                ProcessMatches(matches);
            }

            while (countStoneDestroy > 0)
            {
                yield return null;
            }

            StartCoroutine(FallStoneAndSlide());

            RefillBoard();

            while (countStoneFallOrSlide > 0)
            {
                yield return null;
            }

            if (isExecuteBomb)
            {
                countMatch = 1;
                isExecuteBomb = false;
            }

        } while (countMatch > 0);

        isProcessing = false;
        startFind = false;
    }

    public MatchType GetMatchType(int length)
    {
        if (length >= 5) return MatchType.Match5;
        if (length == 4) return MatchType.Match4;
        if (length == 3) return MatchType.Match3;
        return MatchType.None;
    }

    public List<MatchGroup> FindAllMatches()
    {
        var allMatches = new List<MatchGroup>();

        // Match Ngang
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (boardStone[r, c] == null || boardStone[r, c].stoneType == StoneType.Ice
                    || boardStone[r, c].stoneType == StoneType.StoneMatch5) continue;

                StoneBehaviour curStone = boardStone[r, c];
                StoneType baseColor = GetBaseColor(curStone.stoneType);
                List<StoneBehaviour> horizontalMatch = new List<StoneBehaviour> {curStone};

                // Đếm chuỗi liên tiếp
                for (int k = c + 1; k < column; k++)
                {
                    if (GetBaseColor(boardStone[r, k].stoneType) == baseColor)
                    {
                        horizontalMatch.Add(boardStone[r, k]);
                    }
                    else break;
                }

                if (horizontalMatch.Count >= 3)
                {
                    StoneBehaviour candidate = null;
                    if (horizontalMatch.Count >= 4)
                    {
                        candidate = horizontalMatch[horizontalMatch.Count / 2];
                    }
                    // Viên đá đầu tiên là ứng cử viên cho viên đặc biệt
                    allMatches.Add(new MatchGroup(horizontalMatch, GetMatchType(horizontalMatch.Count), candidate, true));
                    // Cập nhật vị trí c để bỏ qua các viên đã được tính
                    c += horizontalMatch.Count - 1;
                }
            }
        }

        // Match Doc
        for (int c = 0; c < column; c++)
        {
            for (int r = 0; r < 9; r++)
            {
                if (boardStone[r, c] == null || boardStone[r, c].stoneType == StoneType.Ice
                    || boardStone[r, c].stoneType == StoneType.StoneMatch5) continue;

                StoneBehaviour curStone = boardStone[r, c];
                StoneType baseColor = GetBaseColor(curStone.stoneType);
                List<StoneBehaviour> verticalMatch = new List<StoneBehaviour> {curStone};

                for (int k = r + 1; k < 9; k++)
                {
                    if (GetBaseColor(boardStone[k, c].stoneType) == baseColor)
                    {
                        verticalMatch.Add(boardStone[k, c]);
                    }
                    else break;
                }

                if (verticalMatch.Count >= 3)
                {
                    StoneBehaviour candidate = null;
                    if (verticalMatch.Count >= 4)
                    {
                        candidate = verticalMatch[verticalMatch.Count / 2];
                    }
                    allMatches.Add(new MatchGroup(verticalMatch, GetMatchType(verticalMatch.Count), candidate, false));
                    r += verticalMatch.Count - 1;
                }
            }
        }

        // Tim phan giao nhau de tao match T/L
        HashSet<StoneBehaviour> mergedStones = new HashSet<StoneBehaviour>();
        for (int i = 0; i < allMatches.Count; i++)
        {
            var group1 = allMatches[i];
            if (group1.MatchType == MatchType.Match5) continue;
            for (int j = i + 1; j < allMatches.Count; j++)
            {
                var group2 = allMatches[j];

                if (group2.MatchType == MatchType.Match5) continue;

                // Tìm viên đá giao điểm
                var intersection = group1.MatchedStones.Intersect(group2.MatchedStones);
                StoneBehaviour intersectionStone = intersection.FirstOrDefault();

                if (intersectionStone != null)
                {
                    // Hop thanh 1 match T hoac L
                    mergedStones.UnionWith(group1.MatchedStones);
                    mergedStones.UnionWith(group2.MatchedStones);

                    // Xoa hai nhom match cu
                    allMatches.RemoveAt(j);
                    allMatches.RemoveAt(i);
                    i--;

                    // Thêm MatchGroup mới đã hợp nhất (luôn là Wrapped/Bomb)
                    allMatches.Add(new MatchGroup(mergedStones, MatchType.TorLShape, intersectionStone, false));

                    // Chỉ cần xử lý 1 lần cho giao điểm này và tiếp tục vòng lặp
                    mergedStones.Clear();
                    break;
                }
            }
        }
        return allMatches;
    }

    private StoneType GetBaseColor(StoneType type)
    {
        string typeName = type.ToString();

        if (typeName.Contains("Red")) return StoneType.Red;
        if (typeName.Contains("Blue")) return StoneType.Blue;
        if (typeName.Contains("Green")) return StoneType.Green;
        if (typeName.Contains("Purple")) return StoneType.Purple;
        if (typeName.Contains("Yellow")) return StoneType.Yellow;

        return type; // Trả về chính nó nếu là Ice hoặc StoneMatch5
    }

    public void ProcessMatches(List<MatchGroup> allMatches)
    {
        var processedStones = new HashSet<StoneBehaviour>();

        foreach (var match in allMatches)
        {
            if (match.SpecialStoneCandidate != null)
            {
                StoneBehaviour candidate = match.SpecialStoneCandidate;
                if (!processedStones.Contains(candidate))
                {
                    bool isMatchHasSpecialStone = false;
                    foreach(var stone in match.MatchedStones)
                    {
                        if (IsMatch4(stone.stoneType) || IsMatchTorL(stone.stoneType))
                        {
                            isMatchHasSpecialStone = true;
                            break;
                        }
                        
                    }
                    if (!isMatchHasSpecialStone)
                    {
                        TransformToSpecial(candidate, match.MatchType, match.isHorizontalMatch);
                        processedStones.Add(candidate);
                    }
                }
            }
        }

        foreach (var match in allMatches)
        {
            foreach (var stone in match.MatchedStones)
            {
                // Nếu viên này chưa được xử lý (không phải là candidate đã giữ lại ở trên 
                // và cũng chưa bị xóa bởi match trước đó)
                if (!processedStones.Contains(stone))
                {
                    StartCoroutine(DestroyAndReturnToPool(stone));
                    processedStones.Add(stone); // Đóng dấu: "Viên này đã xóa, không được đụng vào nữa"
                }
            }
        }
    }

    private void TransformToSpecial(StoneBehaviour stone, MatchType matchType, bool isHorizontalMatch)
    {
        int r = stone.r;
        int c = stone.c;
        StoneType originalColorType = stone.stoneType;
        StoneType specialType = originalColorType;

        switch (matchType)
        {
            case MatchType.Match4:
                specialType = GetSpecialStoneTypeForMatch4(originalColorType);
                break;
            case MatchType.TorLShape:
                specialType = GetSpecialStoneTypeForMatchTorL(originalColorType); 
                break;
            case MatchType.Match5:
                specialType = StoneType.StoneMatch5;
                break;
        }

        // Nếu không có sự thay đổi (Match3 hoặc lỗi), thoát ra
        if (specialType == originalColorType) return;

        UnRegisterStone(r, c);
        stonePoolManager.ReturnStoneByType(originalColorType, stone.gameObject);

        Vector2 position = UpdatePositionStone(c, r);
        GameObject specialStoneObj = stonePoolManager.GetStoneByType(specialType, position, c, r);

        StoneBehaviour newStoneBehaviour = specialStoneObj.GetComponent<StoneBehaviour>();
        RegisterStone(newStoneBehaviour, r, c);
        newStoneBehaviour.isHorizontalExplosion = isHorizontalMatch;

    }

    private StoneType GetSpecialStoneTypeForMatch4(StoneType color)
    {
        switch (color)
        {
            case StoneType.Red: return StoneType.RedMatch4;
            case StoneType.Blue: return StoneType.BlueMatch4;
            case StoneType.Green: return StoneType.GreenMatch4;
            case StoneType.Purple: return StoneType.PurpleMatch4;
            case StoneType.Yellow: return StoneType.YellowMatch4;
            default: return color;
        }
    }

    private StoneType GetSpecialStoneTypeForMatchTorL(StoneType color)
    {
        switch (color)
        {
            case StoneType.Red: return StoneType.RedMatchTorL;
            case StoneType.Blue: return StoneType.BlueMatchTorL;
            case StoneType.Green: return StoneType.GreenMatchTorL;
            case StoneType.Purple: return StoneType.PurpleMatchTorL;
            case StoneType.Yellow: return StoneType.YellowMatchTorL;
            default: return color;
        }
    }

    public void UpdateTargetStone(StoneBehaviour stone)
    {
        string typeName = stone.stoneType.ToString();
        if (targetList.ContainsKey(typeName))
        {
            if (targetList[typeName] > 0)
            {
                targetList[typeName] -= 1;
                uiHandler.UpdateCountTargetStoneUI(typeName, targetList[typeName]);
                if (CheckAllTargetsComplete())
                {
                    WinGame();
                }
            }
        }
    }

    public void UpdateMove()
    {
        if (curMove > 0)
        {
            curMove--;
            uiHandler.UpdateMovesUI(curMove);
        }

        if (curMove <= 0)
        {
            Debug.Log("Out of moves!");
        }
    }

    private void WinGame()
    {
        Debug.Log("CHÚC MỪNG! BẠN ĐÃ HOÀN THÀNH CẤP ĐỘ.");

        // Cách 1: Dừng thời gian hệ thống (Game đứng yên)
        Time.timeScale = 0;

        // Cách 2: Hiển thị Panel thông báo thắng (Khuyên dùng)
        // winPanel.SetActive(true); 

        // Cách 3: Chuyển sang Scene mới hoặc Load lại
        // SceneManager.LoadScene("WinScene");
    }

    private bool CheckAllTargetsComplete()
    {
        foreach (var target in targetList)
        {
            if (target.Value > 0)
            {
                return false;
            }
        }
        return true;
    }

    public void DestroyBlockIce(int r, int c)
    {
        // Định nghĩa 4 hướng di chuyển: Phải, Trái, Dưới, Trên
        int[] dr = { 0, 0, 1, -1 };
        int[] dc = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            // Kiểm tra biên (Dùng biến row/column thay vì số cứng như 9)
            if (nr >= 0 && nr < 9 && nc >= 0 && nc < column)
            {
                var targetStone = boardStone[nr, nc];

                if (targetStone != null && targetStone.stoneType == StoneType.Ice)
                {
                    Destroy(targetStone.gameObject);
                    boardStone[nr, nc] = null; // Cực kỳ quan trọng để tránh lỗi logic sau này
                }
            }
        }
    }

    public IEnumerator DestroyAndReturnToPool(StoneBehaviour stone)
    {
        if (stone == null) yield break;

        StoneType type = stone.stoneType;
        int r = stone.r;
        int c = stone.c;
        bool hExplode = stone.isHorizontalExplosion;

        UnRegisterStone(r, c);

        if (IsMatch4(type))
        {
            // Thay vì gọi Destroy tiếp, ta gọi hàm xử lý xóa diện rộng
            if (!hExplode) StartCoroutine(ExecuteColumnExplosion(c));
            else StartCoroutine(ExecuteRowExplosion(r));
        }
        else if (IsMatchTorL(type))
        {
            Execute3x3Explosion(r, c);
        }

        UpdateTargetStone(stone);
        DestroyBlockIce(r, c);

        stonePoolManager.ReturnStoneByType(type, stone.gameObject);
        yield return null;
    }

    private void Execute3x3Explosion(int r, int c)
    {
        for (int i = r - 1; i <= r + 1; i++)
        {
            for (int j = c - 1; j <= c + 1; j++)
            {
                if (i >= 0 && i < 9 && j >= 0 && j < column)
                {
                    StoneBehaviour target = boardStone[i, j];
                    if (target != null)
                    {
                        CleanUpSpecialStone(target);
                    }
                }
            }
        }
    }

    private IEnumerator ExecuteRowExplosion(int r)
    {
        for (int c = 0; c < column; c++)
        {
            StoneBehaviour target = boardStone[r, c];
            if (target != null)
            {
                CleanUpSpecialStone(target);
            }
        }
        yield return null;
    }

    private IEnumerator ExecuteColumnExplosion(int c)
    {
        for (int r = 0; r < 9; r++)
        {
            StoneBehaviour target = boardStone[r, c];
            if (target != null)
            {
                CleanUpSpecialStone(target);
            }
        }
        yield return null;
    }

    public void ExecuteColorBomb(StoneBehaviour bomb, StoneType targetType)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < column; c++)
            {
                StoneBehaviour s = boardStone[r, c];
                if (s != null && s.stoneType == targetType)
                {
                    StartCoroutine(DestroyAndReturnToPool(s));
                }
            }
        }

        UnRegisterStone(bomb.r, bomb.c);
        stonePoolManager.ReturnStoneByType(StoneType.StoneMatch5, bomb.gameObject);
        startFind = true;
        isExecuteBomb = true;
    }

    public void ExecuteUltraBomb()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (boardStone[r, c] != null && boardStone[r, c].stoneType != StoneType.Ice)
                {
                    StartCoroutine(DestroyAndReturnToPool(boardStone[r, c]));
                }
            }
        }
        startFind = true;
        isExecuteBomb = true;
    }

    private void CleanUpSpecialStone(StoneBehaviour stone)
    {
        int r = stone.r;
        int c = stone.c;
        StoneType type = stone.stoneType;

        if(type == StoneType.Ice)
        {
            Destroy(stone.gameObject);
            UnRegisterStone(r, c);
            return;
        }

        UnRegisterStone(r, c);
        UpdateTargetStone(stone);
        stonePoolManager.ReturnStoneByType(type, stone.gameObject);
    }

    private bool IsMatch4(StoneType type)
    {
        return type.ToString().Contains("Match4");
    }

    private bool IsMatchTorL(StoneType type)
    {
        return type.ToString().Contains("MatchTorL");
    }

    public IEnumerator FallStoneAndSlide()
    {
        var moveAllPathOfStone = pathCaculator.GetMovePathOfStones();

        foreach (var movePathOfStone in moveAllPathOfStone)
        {
            StartCoroutine(movePathOfStone.stone.FallAndSlide(movePathOfStone.movePath));
            yield return null;
        }
        
    }
    
    public void RefillBoard()
    {
        for(int c=0; c<column; c++)
        {
            for(int r=row-1; r>=0; r--)
            {
                if (boardStone[r, c] != null) break;
                GameObject stone = stonePoolManager.GetRandomStone(c, r);
                RegisterStone(stone.GetComponent<StoneBehaviour>(), r, c);
            }
        }
    }

    public bool CheckStoneAfterSwap(int rA, int cA, int rB, int cB)
    {
        return HasMatchAt(rA, cA) || HasMatchAt(rB, cB);
    }

    private bool HasMatchAt(int r, int c)
    {
        StoneType type = GetBaseColor(boardStone[r, c].stoneType);

        int horizontalCount = 1;
        for (int i = c - 1; i >= 0; i--)
        {
            if (boardStone[r, i] != null && GetBaseColor(boardStone[r, i].stoneType) == type) horizontalCount++;
            else break;
        }
            
        for (int i = c + 1; i < column; i++)
        {
            if (boardStone[r, i] != null && GetBaseColor(boardStone[r, i].stoneType) == type) horizontalCount++;
            else break;
        }

        if (horizontalCount >= 3) return true;

        int verticalCount = 1;
        for (int i = r - 1; i >= 0; i--)
        {
            if (boardStone[i, c] != null && GetBaseColor(boardStone[i, c].stoneType) == type) verticalCount++;
            else break;
        }
           
        for (int i = r + 1; i < 9; i++)
        {
            if (boardStone[i, c] != null && GetBaseColor(boardStone[i, c].stoneType) == type) verticalCount++;
            else break;
        }

        return verticalCount >= 3;
    }
}


