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
    string sheetAPIurl = "https://script.google.com/macros/s/AKfycbzdRsGIH-ptKHvWWIYAqvXgw2o1L2OlkzyACCzWfhLd_gGflyMl0Lt-DyJv4ON50T7v/exec";
    string sheeturl = "https://docs.google.com/spreadsheets/d/15LgGPgcJTUdnBY0oHn6y0l47Y6YHvV6oKILyh8l6cpk";

    private List<SheetData> sheets = new List<SheetData>();
    private int selectedSheetIndex = 0;
    private bool isFetching = false;

    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
{
    "class","namespace","public","private","protected","internal","static","void",
    "int","float","double","string","bool","new","return","null","true","false",
    "if","else","switch","case","for","foreach","while","do","break","continue",
    "this","base","using","try","catch","finally","throw","out","ref","in","is","as"
};
    
    /// <summary>
    /// TSV 필드명을 C#에서 사용 가능한 식별자로 변환한다.
    /// 특수문자 제거, 숫자 시작 방지, 예약어 처리 등을 수행한다.
    /// </summary>
    /// <param name="raw">원본 필드명</param>
    /// <returns>정제된 식별자 문자열</returns>
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

    
    /// <summary>
    /// Apps Script API에서 스프레드시트 목록(JSON)을 요청하여 가져온다.
    /// 요청이 성공하면 ProcessSheetsData를 통해 시트 목록을 파싱한다.
    /// </summary>
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


    /// <summary>
    /// Apps Script로부터 전달받은 JSON 문자열을 SheetDataList로 역직렬화하여
    /// 내부 sheets 리스트에 시트 정보를 저장한다.
    /// </summary>
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


    /// <summary>
    /// 현재 선택된 시트를 기준으로 Google Sheets TSV 데이터를 요청하고
    /// JSON 파일 생성 및 데이터 클래스 생성을 시작한다.
    /// </summary>
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

    /// <summary>
    /// 선택된 시트의 TSV 데이터를 다운로드하여 파싱한다.
    /// DB_IGNORE 및 타입 정보를 기반으로 JSON 데이터를 생성하고,
    /// Resources/JsonFiles 경로에 저장한 후 대응되는 C# 데이터 클래스를 생성한다.
    /// </summary>
    /// <param name="jsonFileName">저장할 JSON 파일 및 클래스 이름</param>
    /// <param name="gid">Google Sheet의 시트 GID</param>
    /// <param name="notice">완료 시 성공 다이얼로그 표시 여부</param>
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


    /// <summary>
    /// TSV 문자열을 줄 단위로 분리하여 각 행을 리스트로 반환한다.
    /// 줄바꿈(\r\n, \n)을 기준으로 분리한다.
    /// </summary>
    /// <param name="data">TSV 전체 문자열</param>
    /// <returns>행 단위 문자열 리스트</returns>
    private List<string> ParseTSVData(string data)
    {
        return data
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .ToList();
    }


    /// <summary>
    /// 첫 번째 행에서 "DB_IGNORE"로 표시된 컬럼 인덱스를 수집하여 반환한다.
    /// 해당 컬럼은 JSON 및 클래스 생성 시 제외된다.
    /// </summary>
    /// <param name="headerRow">TSV의 첫 번째 행 문자열</param>
    /// <returns>무시해야 할 컬럼 인덱스 집합</returns>
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


    /// <summary>
    /// 하나의 TSV 행을 키와 타입 정보를 기준으로 JObject로 변환한다.
    /// DB_IGNORE 컬럼은 제외되며, 값은 타입에 맞게 ConvertValue로 변환된다.
    /// </summary>
    /// <param name="keys">필드명 리스트</param>
    /// <param name="types">타입명 리스트</param>
    /// <param name="rowData">현재 행의 데이터 리스트</param>
    /// <param name="dbIgnoreColumns">무시할 컬럼 인덱스 집합</param>
    /// <returns>변환된 JObject (유효 데이터가 없으면 null)</returns>
    private JObject ParseRow(List<string> keys, List<string> types, List<string> rowData, HashSet<int> dbIgnoreColumns)
    {
        var rowObject = new JObject();

        for (int j = 0; j < keys.Count && j < rowData.Count; j++)
        {
            if (dbIgnoreColumns.Contains(j)) continue;

            string key = keys[j];
            string type = types[j];
            string rawValue = rowData[j].Trim();

            if (string.IsNullOrEmpty(key)) continue;

            string value = rawValue;

            if (type.Trim() != "string")
            {
                value = rawValue.Trim();
            }

            if (string.IsNullOrEmpty(value)) continue;

            rowObject[key] = ConvertValue(value, type);
        }

        return rowObject.HasValues ? rowObject : null;
    }


    /// <summary>
    /// 문자열 값을 지정된 타입에 맞게 변환하여 JToken으로 반환한다.
    /// 기본 타입(int, float, bool 등)과 배열 타입을 지원한다.
    /// 변환 실패 시 기본값을 반환한다.
    /// </summary>
    /// <param name="value">변환할 문자열 값</param>
    /// <param name="type">타입 문자열</param>
    /// <returns>변환된 JToken 값</returns>
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


    /// <summary>
    /// 생성된 JArray 데이터를 Resources/JsonFiles 경로에 JSON 파일로 저장한다.
    /// 폴더가 없으면 자동으로 생성한다.
    /// </summary>
    /// <param name="jsonFileName">저장할 파일 이름 (확장자 제외)</param>
    /// <param name="jArray">저장할 JSON 배열 데이터</param>
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


    /// <summary>
    /// TSV의 키 및 타입 정보를 기반으로 C# 데이터 클래스를 생성한다.
    /// DB_IGNORE 컬럼은 제외되며,
    /// Resources/DataClass 경로에 .cs 파일로 저장된다.
    /// </summary>
    /// <param name="fileName">생성할 클래스 이름</param>
    /// <param name="keys">필드명 리스트</param>
    /// <param name="types">타입명 리스트</param>
    /// <param name="dbIgnoreColumns">무시할 컬럼 인덱스 집합</param>
    /// <returns>생성된 클래스 이름</returns>
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
                    writer.WriteLine($"\tpublic {fieldType} {fieldName};");
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


    /// <summary>
    /// 문자열 타입 정보를 C# 타입 문자열로 변환한다.
    /// 지원하지 않는 타입은 기본적으로 string으로 처리한다.
    /// </summary>
    /// <param name="type">타입 문자열</param>
    /// <returns>C# 타입 문자열</returns>
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
#endif