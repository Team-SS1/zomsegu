using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;


public partial class SheetParsing : EditorWindow
{
    string sheetAPIurl = "** 배포 받은 url 링크 **";
    string sheeturl = "** 구글스프레드시트 링크 **";

    private List<SheetData> sheets = new List<SheetData>();
    private int selectedSheetIndex = 0;
    private bool isFetching = false;

    [MenuItem("Tools/Google Sheet Parsing Tool")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(SheetParsing));
        window.titleContent = new GUIContent("Google Sheet Parser");
        window.maxSize = new Vector2(600, 400);
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        if (isFetching)
        {
            EditorGUILayout.LabelField("Fetching data...");
        }
        else
        {
            if (sheets.Count > 0)
            {
                string[] sheetNames = sheets.Select(s => s.sheetName).ToArray();
                selectedSheetIndex = EditorGUILayout.Popup("Select Sheet", selectedSheetIndex, sheetNames);
            }
            else
            {
                EditorGUILayout.LabelField("No sheets found.");
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Fetch Sheets Data", GUILayout.Height(40)))
            {
                EditorCoroutineUtility.StartCoroutine(FetchSheetsData(), this);
            }
        }

        GUILayout.Space(30);
        if (GUILayout.Button("Parse Selected Sheet and Create Class", GUILayout.Height(40)))
        {
            if (sheets.Count > 0)
            {
                ParseSelectedSheet();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please fetch sheet names and select a sheet.", "OK");
            }
        }
    }
    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
{
    "class","namespace","public","private","protected","internal","static","void",
    "int","float","double","string","bool","new","return","null","true","false",
    "if","else","switch","case","for","foreach","while","do","break","continue",
    "this","base","using","try","catch","finally","throw","out","ref","in","is","as"
};

    private string SanitizeIdentifier(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "_field";

        // 1) 공백/특수문자 -> '_'로 바꾸기
        string s = Regex.Replace(raw.Trim(), @"[^a-zA-Z0-9_]", "_");

        // 2) 숫자로 시작하면 '_' 붙이기
        if (char.IsDigit(s[0])) s = "_" + s;

        // 3) 연속된 '_' 정리
        s = Regex.Replace(s, @"_+", "_");

        // 4) 예약어면 '_' 붙이기 (또는 @class 같은 방식도 가능)
        if (CSharpKeywords.Contains(s)) s = "_" + s;

        return s;
    }

    private IEnumerator FetchSheetsData()
    {
        isFetching = true;
        UnityWebRequest request = UnityWebRequest.Get(sheetAPIurl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ProcessSheetsData(request.downloadHandler.text);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Error fetching data: " + request.error);
#endif
        }

        isFetching = false;
        Repaint();
    }

    private void ProcessSheetsData(string json)
    {
        var sheetsData = JsonUtility.FromJson<SheetDataList>(json);
        sheets.Clear();
        sheets.AddRange(sheetsData.sheetData);

        if (sheets.Count > 0)
        {
            selectedSheetIndex = 0;
        }
    }

    private void ParseSelectedSheet()
    {
        var selectedSheet = sheets[selectedSheetIndex];
        string jsonFileName = RemoveSpecialCharacters(selectedSheet.sheetName);
#if UNITY_EDITOR
        Debug.Log($"Selected Sheet: {selectedSheet.sheetName}, Sheet ID: {selectedSheet.sheetId}");
#endif

        EditorCoroutineUtility.StartCoroutine(ParseGoogleSheet(jsonFileName, selectedSheet.sheetId.ToString()), this);
    }

    private string RemoveSpecialCharacters(string sheetName)
    {
        return Regex.Replace(sheetName, @"[^a-zA-Z0-9\s]", "").Replace(" ", "_");
    }

    private IEnumerator ParseGoogleSheet(string jsonFileName, string gid, bool notice = true)
    {
        // Build export URL explicitly (sheeturl already = https://docs.google.com/spreadsheets/d/ID)
        string sheetUrl = $"{sheeturl}/export?format=tsv&gid={gid}";
#if UNITY_EDITOR
        Debug.Log($"TSV Request URL = {sheetUrl}");

        Debug.Log($"[SheetParsing] Requesting TSV URL: {sheetUrl}");
#endif

        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        // 네트워크 에러나 HTML 응답 등 감지용
        if (request.result != UnityWebRequest.Result.Success)
        {
#if UNITY_EDITOR
            Debug.LogError($"[SheetParsing] Request failed. error={request.error}, code={request.responseCode}, url={sheetUrl}");
#endif
            EditorUtility.DisplayDialog("Fail", $"Request failed. code={request.responseCode}\nSee Console.", "OK");
            yield break;
        }

        string data = request.downloadHandler.text;

        // Content-Type 확인 (서버가 어떤 형식으로 줬는지)
        string contentType = request.GetResponseHeader("Content-Type") ?? "null";
#if UNITY_EDITOR
        Debug.Log($"[SheetParsing] responseCode={request.responseCode}, Content-Type={contentType}");
#endif

        // HTML 수신 감지 강화
        string head = data.Length > 300 ? data.Substring(0, 300).ToLowerInvariant() : data.ToLowerInvariant();
        bool looksLikeHtml =
            head.Contains("<!doctype") || head.Contains("<html") || head.Contains("<head") ||
            head.Contains("window.location") || head.Contains("<script");

        if (looksLikeHtml || contentType.ToLowerInvariant().Contains("text/html"))
        {
            string snippet = data.Length > 1000 ? data.Substring(0, 1000) : data;
#if UNITY_EDITOR
            Debug.LogError($"[SheetParsing] Received HTML (not TSV). code={request.responseCode}, Content-Type={contentType}\n{snippet}");
#endif
            EditorUtility.DisplayDialog("Fail", "Received HTML instead of TSV.\nCheck share/publish permissions.", "OK");
            yield break;
        }

        List<string> rows = ParseTSVData(data);

#if UNITY_EDITOR
        Debug.Log($"[SheetParsing] rows.Count = {(rows == null ? 0 : rows.Count)}");
#endif

        if (rows == null || rows.Count < 3) // 최소 3행 확인 (keys, types, data header)
        {
#if UNITY_EDITOR
            Debug.LogError($"[SheetParsing] Not enough data rows to parse. rows.Count={rows?.Count ?? 0}");
#endif
            EditorUtility.DisplayDialog("Fail", "Not enough rows. Ensure sheet has at least 3 rows (DB_IGNORE/keys/types).", "OK");
            yield break;
        }

        // Diagnostic: 출력 상위 5줄 (안전하게)
        for (int i = 0; i < Math.Min(6, rows.Count); i++)
        {
#if UNITY_EDITOR
            Debug.Log($"[SheetParsing] row[{i}] => \"{rows[i].Replace("\t", "\\t")}\"");
#endif
        }

        HashSet<int> dbIgnoreColumns = GetDBIgnoreColumns(rows[0]);
        var keys = rows[1].Split('\t').ToList();
        var types = rows[2].Split('\t').ToList();

        // 타입 없는 컬럼 자동 ignore
        for (int i = 0; i < keys.Count; i++)
        {
            // types.Count보다 크거나 타입 문자열이 비어있으면 ignore
            if (i >= types.Count || string.IsNullOrWhiteSpace(types[i]))
            {
                dbIgnoreColumns.Add(i);
#if UNITY_EDITOR
                Debug.LogWarning($"Column {i + 1} ignored because type is missing. key=\"{keys[i]}\"");
#endif
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[SheetParsing] keys.Count={keys.Count}, types.Count={types.Count}, dbIgnoreColumns.Count={dbIgnoreColumns.Count}");
#endif

        // 안전 파싱: 각 데이터 행이 keys보다 짧으면 채워넣기(빈 값)
        JArray jArray = new JArray();
        for (int i = 3; i < rows.Count; i++)
        {
            var rowData = rows[i].Split('\t').ToList();

            // 길이가 keys보다 짧으면 빈 문자열로 확장
            if (rowData.Count < keys.Count)
            {
                int missing = keys.Count - rowData.Count;
                for (int k = 0; k < missing; k++) rowData.Add(string.Empty);
            }

            // 첫 열이 DB_IGNORE라면 행 제외
            if (rowData.Count > 0 && rowData[0].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
#if UNITY_EDITOR
                Debug.Log($"[SheetParsing] Row {i + 1} ignored due to DB_IGNORE (first cell)");
#endif
                continue;
            }

            var rowObject = ParseRow(keys, types, rowData, dbIgnoreColumns);
            if (rowObject != null)
            {
                jArray.Add(rowObject);
            }
        }

        SaveJsonToFile(jsonFileName, jArray);
        string className = CreateDataClass(jsonFileName, keys, types, dbIgnoreColumns);  // C# 클래스 생성

        if (notice)
        {
            EditorUtility.DisplayDialog("Success", "Sheet parsed and saved as JSON successfully!", "OK");
            AssetDatabase.Refresh();
        }
    }


    // TSV 데이터 파싱
    private List<string> ParseTSVData(string data)
    {
        return data.Split('\n')
           .Select(l => l.TrimEnd('\r'))
           .ToList();
    }

    // DB_IGNORE 열 필터링
    private HashSet<int> GetDBIgnoreColumns(string headerRow)
    {
        var dbIgnoreColumns = new HashSet<int>();
        var firstRow = headerRow.Split('\t').ToList();

        for (int i = 0; i < firstRow.Count; i++)
        {
            if (firstRow[i].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                dbIgnoreColumns.Add(i);
#if UNITY_EDITOR
                Debug.Log($"Column {i + 1} ignored due to DB_IGNORE");
#endif
            }
        }

        return dbIgnoreColumns;
    }

    // 개별 행 파싱
    private JObject ParseRow(List<string> keys, List<string> types, List<string> rowData, HashSet<int> dbIgnoreColumns)
    {
        var rowObject = new JObject();

        for (int j = 0; j < keys.Count && j < rowData.Count; j++)
        {
            if (dbIgnoreColumns.Contains(j)) continue;

            string key = keys[j];
            string type = types[j];
            string value = rowData[j].Trim();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

            rowObject[key] = ConvertValue(value, type);
        }

        return rowObject.HasValues ? rowObject : null;
    }

    // 값을 적절한 형식으로 변환하는 메서드
    private JToken ConvertValue(string value, string type)
    {
        switch (type.Trim()) // 불필요한 공백 제거
        {
            case "int": return int.TryParse(value, out int intValue) ? intValue : 0;
            case "long": return long.TryParse(value, out long longValue) ? longValue : 0L;
            case "float":
                return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue)
                    ? floatValue
                    : 0.0f;

            case "double":
                return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue)
                    ? doubleValue
                    : 0.0d;
            case "bool":
                {
                    var v = value.Trim().ToLowerInvariant();
                    if (v == "1" || v == "y" || v == "yes" || v == "true") return true;
                    if (v == "0" || v == "n" || v == "no" || v == "false") return false;
                    return bool.TryParse(value, out var b) ? b : false;
                }
            case "byte": return byte.TryParse(value, out byte byteValue) ? byteValue : (byte)0;
            case "int[]":
                {
                    var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => v.Trim());
                    return new JArray(parts.Select(v => int.TryParse(v, out var n) ? n : 0));
                }

            case "float[]":
                {
                    var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => v.Trim());
                    return new JArray(parts.Select(v =>
                        float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f));
                }

            case "string[]":
                {
                    var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => v.Trim());
                    return new JArray(parts);
                }
            case "DateTime": return DateTime.TryParse(value, out DateTime dateTimeValue) ? dateTimeValue : DateTime.MinValue; // DateTime 변환
            case "TimeSpan": return TimeSpan.TryParse(value, out TimeSpan timeSpanValue) ? timeSpanValue : TimeSpan.Zero;
            case "Guid": return Guid.TryParse(value, out Guid guidValue) ? guidValue.ToString() : Guid.Empty.ToString();
            default: return value; // 기본적으로 문자열로 반환
        }
    }

    // JSON 파일 저장 메서드
    private void SaveJsonToFile(string jsonFileName, JArray jArray)
    {
        string directoryPath = Path.Combine(Application.dataPath, "Resources", "JsonFiles");

        // 폴더가 존재하지 않으면 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string jsonFilePath = Path.Combine(directoryPath, $"{jsonFileName}.json");

        File.WriteAllText(jsonFilePath, jArray.ToString());
#if UNITY_EDITOR
        Debug.Log($"Saved JSON to: {jsonFilePath}");
#endif
    }

    // C# 클래스 생성 메서드
    private string CreateDataClass(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
    {
        string className = fileName; // 파일 이름을 클래스 이름으로 사용
        string directoryPath = Path.Combine(Application.dataPath, "Resources/DataClass");

        // 폴더가 존재하지 않으면 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dataClassPath = Path.Combine(directoryPath, $"{className}.cs");

        using (StreamWriter writer = new StreamWriter(dataClassPath))
        {
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("[System.Serializable]");
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{");

            // 클래스 필드 생성
            for (int i = 0; i < keys.Count; i++)
            {
                if (dbIgnoreColumns.Contains(i)) continue; // DB_IGNORE가 설정된 컬럼 건너뜀

                string fieldType = ConvertTypeToCSharp(types[i]);
                string fieldNameRaw = keys[i];
                string fieldName = SanitizeIdentifier(fieldNameRaw);

                if (!string.IsNullOrEmpty(fieldName))
                {
                    writer.WriteLine($"\tpublic {fieldType} {fieldName}; // from \"{fieldNameRaw}\"");
                }
            }

            writer.WriteLine();

            // Dictionary 생성
            string keyType = ConvertTypeToCSharp(types[0]); // 첫 번째 컬럼을 Dictionary 키로 사용
            writer.WriteLine($"\tpublic static Dictionary<{keyType}, {className}> tableDic = new Dictionary<{keyType}, {className}>();");

            writer.WriteLine("}");
        }

#if UNITY_EDITOR
        Debug.Log($"Saved C# class to: {dataClassPath}");
#endif
        AssetDatabase.Refresh(); // 새로 생성된 클래스를 에디터에서 인식하도록 리프레시

        return className; // 생성된 클래스 이름을 반환
    }

    private string ConvertTypeToCSharp(string type)
    {
        switch (type.Trim()) // 불필요한 공백 제거
        {
            case "int": return "int";
            case "long": return "long";
            case "float": return "float";
            case "double": return "double";
            case "bool": return "bool";
            case "byte": return "byte";
            case "int[]": return "int[]";
            case "float[]": return "float[]";
            case "string[]": return "string[]";
            case "DateTime": return "System.DateTime"; // DateTime에 대한 올바른 반환값
            case "TimeSpan": return "System.TimeSpan";
            case "Guid": return "System.Guid";
            default: return "string"; // 기본적으로 string으로 처리
        }
    }
}

// SheetData 클래스
[System.Serializable]
public class SheetData
{
    public string sheetName;
    public int sheetId;
}

// SheetDataList 클래스
[System.Serializable]
public class SheetDataList
{
    public SheetData[] sheetData;
}
#endif