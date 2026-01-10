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
                      purpleDiamonPrefab, yellowDiamonPrefab, icePrefab,
                      blueMatch4;

    public FirestoreReader firestoreReader;
    public StonePoolManager stonePoolManager;
    private LevelData levalData;
    private PathCaculator pathCaculator;
    public StoneBehaviour[,] boardStone;
    public TargetUIHandler uiHandler;
    public Sprite[] allStoneSprites;
    public Transform stoneContainer;

    public int row, column;
    public int curMove;
    public Dictionary<string, int> targetList;
    public int countMatch = 0;
    public int countStoneDestroy = 0;
    public static bool startFind = false;
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
        foreach (string rule in ruleList)
        {
            switch (rule)
            {
                case "spawn3Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_Type("spawn3Type");
                    break;
                case "spawn4Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_Type("spawn4Type");
                    break;
                case "spawn5Type":
                    stoneList = await firestoreReader.LoadRuleSpawn_x_Type("spawn5Type");
                    break;
            }
        }
        if (stoneList != null)
        {
            // Khoi tao pool
            Dictionary<StoneType, GameObject> stonePrefab = new Dictionary<StoneType, GameObject>();
            foreach (var i in stoneList)
            {
                stonePrefab[i] = GetStonePrefabByType(i);
            }
            stonePoolManager.InitPools(stonePrefab, 60);

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
            // Tim tat ca match
            List<MatchGroup> matches = FindAllMatches();
            countMatch = matches.Count;
            if (countMatch == 0) break;

            // Xu ly toan bo match
            ProcessMatches(matches);

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

        } while (countMatch > 0);

        isProcessing = false;
        startFind = false;
    }

    public MatchType GetMatchType(int length, bool isHorizontal)
    {
        if (length >= 5) return MatchType.Match5;
        if (length == 4) return MatchType.Match4;
        if (length == 3) return MatchType.Match3;
        return MatchType.None;
    }

    public List<MatchGroup> FindAllMatches()
    {
        var allMatches = new List<MatchGroup>();
        var stonesInTempMatches = new HashSet<StoneBehaviour>();

        // Match Ngang
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (boardStone[r, c] == null || boardStone[r, c].stoneType == StoneType.Ice) continue;

                StoneBehaviour curStone = boardStone[r, c];
                StoneType type = curStone.stoneType;
                List<StoneBehaviour> horizontalMatch = new List<StoneBehaviour> {curStone};

                // Đếm chuỗi liên tiếp
                for (int k = c + 1; k < column; k++)
                {
                    if (boardStone[r, k] != null && boardStone[r, k].stoneType == type)
                    {
                        horizontalMatch.Add(boardStone[r, k]);
                    }
                    else break;
                }

                if (horizontalMatch.Count >= 3)
                {
                    // Viên đá đầu tiên là ứng cử viên cho viên đặc biệt
                    allMatches.Add(new MatchGroup(horizontalMatch, GetMatchType(horizontalMatch.Count, true)));
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
                if (boardStone[r, c] == null || boardStone[r, c].stoneType == StoneType.Ice) continue;

                StoneBehaviour curStone = boardStone[r, c];
                StoneType type = curStone.stoneType;
                List<StoneBehaviour> verticalMatch = new List<StoneBehaviour> {curStone};

                for (int k = r + 1; k < 9; k++)
                {
                    if (boardStone[k, c] != null && boardStone[k, c].stoneType == type)
                    {
                        verticalMatch.Add(boardStone[k, c]);
                    }
                    else break;
                }

                if (verticalMatch.Count >= 3)
                {
                    allMatches.Add(new MatchGroup(verticalMatch, GetMatchType(verticalMatch.Count, false)));
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
                    allMatches.Add(new MatchGroup(mergedStones, MatchType.TorLShape));

                    // Chỉ cần xử lý 1 lần cho giao điểm này và tiếp tục vòng lặp
                    mergedStones.Clear();
                    break;
                }
            }
        }
        return allMatches;
    }

    public void ProcessMatches(List<MatchGroup> allMatches)
    {
        var processedStones = new HashSet<StoneBehaviour>();

        foreach (var match in allMatches)
        {
            //if (match.MatchType != MatchType.Match3)
            //{
            //    // --- Tạo viên đặc biệt ---
            //    //StoneBehaviour specialStoneSpot = match.SpecialStoneCandidate;

            //    // Nếu viên này chưa bị xử lý trong một nhóm đặc biệt khác (chỉ xảy ra với phức tạp > T/L)
            //    //if (processedStones.Contains(specialStoneSpot)) continue;

            //    //StoneType newType = ConvertMatchTypeToSpecialStone(match.MatchType);

            //    // Giả định StoneBehaviour có hàm chuyển đổi loại (ConvertType) hoặc tạo/thay thế
            //    // Ở đây, ta dùng cách đơn giản là hủy và tạo lại (hoặc gọi hàm chuyển đổi)

            //    // 1. Hủy viên cũ tại chỗ
            //    //UnRegisterStone(specialStoneSpot.Row, specialStoneSpot.Col);
            //    //stonePoolManager.ReturnStoneByType(specialStoneSpot.stoneType, specialStoneSpot.gameObject);

            //    // 2. Spawn viên đặc biệt mới
            //    //Vector2 positionStone = new Vector2(specialStoneSpot.Col, specialStoneSpot.Row);
            //    //GameObject newStoneObj = stonePoolManager.GetStoneByType(newType, positionStone);
            //    //StoneBehaviour newSpecialStone = newStoneObj.GetComponent<StoneBehaviour>();
            //    //newSpecialStone.Row = specialStoneSpot.Row; // Cập nhật hàng, cột
            //    //newSpecialStone.Col = specialStoneSpot.Col;
            //    //RegisterStone(newSpecialStone, specialStoneSpot.Row, specialStoneSpot.Col);

            //    //processedStones.Add(newSpecialStone); // Đánh dấu viên mới

            //    // 3. Xóa các viên đá còn lại trong nhóm match
            //    foreach (var stone in match.MatchedStones)
            //    {
            //        //if (stone != specialStoneSpot && !processedStones.Contains(stone))
            //        //{
            //            StartCoroutine(DestroyAndReturnToPool(stone));
            //            processedStones.Add(stone);
            //            countMatch++;
            //        //}
            //    }
            //}
            //else // Match 3 binh thuong
            //{
                foreach (var stone in match.MatchedStones)
                {
                    if (!processedStones.Contains(stone))
                    {
                        StartCoroutine(DestroyAndReturnToPool(stone));
                        processedStones.Add(stone);
                    }
                }
            //}
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
        countStoneDestroy++;
        UpdateTargetStone(stone);
        // Đảm bảo StoneBehaviour có thuộc tính Row và Col được cập nhật
        StoneType type = stone.stoneType;
        DestroyBlockIce(stone.r, stone.c);
        UnRegisterStone(stone.r, stone.c); // Xóa khỏi mảng boardStone
        stonePoolManager.ReturnStoneByType(type, stone.gameObject); // Trả về Pool

        // **Bước 1: Kích hoạt Hiệu ứng nổ**
        // Giả định stone.PlayDestroyEffect() đã được cài đặt trong StoneBehaviour
        // stone.PlayDestroyEffect(); 

        // Chờ đợi animation nổ/hủy hoàn thành (ví dụ: 0.2 giây)
        yield return null;
       

        countStoneDestroy--;
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

    //// Ham nay kiem tra 4 huong cua stone tai hang r, cot c
    //public bool CheckMatch3(int r, int c)
    //{
    //    if (r + 2 <= 8 && boardStone[r + 1, c] != null && boardStone[r + 2, c] != null &&
    //        boardStone[r, c].stoneType == boardStone[r + 1, c].stoneType &&
    //        boardStone[r + 1, c].stoneType == boardStone[r + 2, c].stoneType)
    //    {
    //        directionDeleteMatch = "up";
    //        return true;
    //    }

    //    else if (c + 2 <= column - 1 && boardStone[r, c + 1] != null && boardStone[r, c + 2] != null &&
    //        boardStone[r, c].stoneType == boardStone[r, c + 1].stoneType &&
    //        boardStone[r, c + 1].stoneType == boardStone[r, c + 2].stoneType)
    //    {
    //        directionDeleteMatch = "right";
    //        return true;
    //    }

    //    else if (r - 2 >= 0 && boardStone[r - 1, c] != null && boardStone[r - 2, c] != null &&
    //        boardStone[r, c].stoneType == boardStone[r - 1, c].stoneType &&
    //        boardStone[r - 1, c].stoneType == boardStone[r - 2, c].stoneType)
    //    {
    //        directionDeleteMatch = "down";
    //        return true;
    //    }

    //    else if (c - 2 >= 0 && boardStone[r, c - 1] != null && boardStone[r, c - 2] != null &&
    //        boardStone[r, c].stoneType == boardStone[r, c - 1].stoneType &&
    //        boardStone[r, c - 1].stoneType == boardStone[r, c - 2].stoneType)
    //    {
    //        directionDeleteMatch = "left";
    //        return true;
    //    }
    //    return false;
    //}

    //// Ham nay giup kiem tra match3 khi stone o trung tam chi dung de kiem tra sau swap
    //public bool CheckMatch3IfStoneCenter(int r, int c)
    //{
    //    if (r - 1 >= 0 && r + 1 <= row - 1 && boardStone[r + 1, c] != null && boardStone[r - 1, c] != null &&
    //        boardStone[r, c].stoneType == boardStone[r + 1, c].stoneType &&
    //        boardStone[r, c].stoneType == boardStone[r - 1, c].stoneType) return true;

    //    if (c - 1 >= 0 && c + 1 <= column - 1 && boardStone[r, c - 1] != null && boardStone[r, c + 1] != null &&
    //        boardStone[r, c].stoneType == boardStone[r, c + 1].stoneType &&
    //        boardStone[r, c].stoneType == boardStone[r, c - 1].stoneType) return true;

    //    return false;
    //}

    //private void DeleteMatch3(int r, int c, string direction)
    //{
    //    int[] d = { 0, 0, 0, 0, 0, 0 };
    //    switch (direction)
    //    {
    //        case "up":
    //            {
    //                d[0] = 0; d[1] = 1; d[2] = 2;
    //                break;
    //            }
    //        case "down":
    //            {
    //                d[0] = 0; d[1] = -1; d[2] = -2;
    //                break;
    //            }
    //        case "left":
    //            {
    //                d[3] = 0; d[4] = -1; d[5] = -2;
    //                break;
    //            }
    //        case "right":
    //            {
    //                d[3] = 0; d[4] = 1; d[5] = 2;
    //                break;
    //            }
    //    }
    //    for (int i = 0; i < 3; i++)
    //    {
    //        StoneBehaviour stone = boardStone[r + d[i], c + d[i + 3]];
    //        stonePoolManager.ReturnStoneByType(stone.stoneType, stone.gameObject);
    //        boardStone[r + d[i], c + d[i + 3]] = null;
    //    }

    //    countMatch += 1;

    //}
}


