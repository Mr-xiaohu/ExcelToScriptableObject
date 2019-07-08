using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Data;
using System;
using System.Reflection;
using System.Linq;
using System.Text;
using System.IO;
public class ExcelToScriptableObj : EditorWindow
{
    enum GenerateType
    {
        None,
        Single_File,   //操作单个配置表
        Folder_Files,  //操作文件夹下所有配置表
    }

    [MenuItem("Generate/GenerateScriptable")]
    static public void AddWindow()
    {
        ExcelToScriptableObj window = EditorWindow.GetWindow<ExcelToScriptableObj>(true, "Generate");

        window.Show();

        Rect rect = new Rect(Screen.currentResolution.width / 2 - 500, Screen.currentResolution.height / 2 - 400, 1000, 800);
        window.position = rect;
        InitPath();
    }


    private static GenerateType generateType = GenerateType.None;
    private static string csOutPath = "";
    private static string objOutPath = "";
    private static string filePath = "";
    private static string folderPath = "";
    private static string fileRecordPaths = "";
    private static string foldRecordPaths = "";

    List<DataTable> dataTable = new List<DataTable>();

    private static void InitPath()
    {
        if (PlayerPrefs.HasKey("GenerateType"))
            generateType = (GenerateType)Enum.Parse(typeof(GenerateType), PlayerPrefs.GetString("GenerateType"));

        if (PlayerPrefs.HasKey("CSOutPath"))
            csOutPath = PlayerPrefs.GetString("CSOutPath");

        if (PlayerPrefs.HasKey("ObjOutPath"))
            objOutPath = PlayerPrefs.GetString("ObjOutPath");

        if (PlayerPrefs.HasKey("FilePath"))
            filePath = PlayerPrefs.GetString("FilePath");

        if (PlayerPrefs.HasKey("FoldPath"))
            folderPath = PlayerPrefs.GetString("FoldPath");

        if (PlayerPrefs.HasKey("FileRecordPath"))
            fileRecordPaths = PlayerPrefs.GetString("FileRecordPath");

        if (PlayerPrefs.HasKey("FoldRecordPath"))
            foldRecordPaths = PlayerPrefs.GetString("FoldRecordPath");

    }
    private void OnGUI()
    {
        if (generateType == GenerateType.None)
            InitPath();

        #region Title
        //Title Label
        GUILayout.Space(20);
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontSize = 20;
        GUILayout.Label(new GUIContent("Select Excel Files To ScriptableObj"), centeredStyle);
        GUILayout.Space(20);
        #endregion

        GUILayout.BeginVertical();

        #region GenerateType
        //GenerateType
        GenerateType type = (GenerateType)EditorGUILayout.EnumPopup(new GUIContent("GenerateType", "Choose target type to generate for."), generateType);
        if (type != generateType)
        {
            generateType = type;
            PlayerPrefs.SetString("GenerateType", type.ToString());
        }
        #endregion

        GUILayout.Space(5);

        #region CSOutputPath
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(5);
        //CS Output Path
        csOutPath = EditorGUILayout.TextField("CS Output Path:", csOutPath, GUILayout.MaxWidth(900));
        EditorGUILayout.EndVertical();
        //Browse "Output Path"
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
        {
            BrowseForFolder(ref csOutPath);
            PlayerPrefs.SetString("CSOutPath", csOutPath);
        }
        EditorGUILayout.EndHorizontal();
        #endregion


        GUILayout.Space(5);

        #region ObjOutputPath
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(5);
        //Obj Output Path
        objOutPath = EditorGUILayout.TextField("Obj Output Path:", objOutPath, GUILayout.MaxWidth(900));
        EditorGUILayout.EndVertical();
        //Browse "Output Path"
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
        {
            BrowseForFolder(ref objOutPath);
            PlayerPrefs.SetString("ObjOutPath", objOutPath);
        }
        EditorGUILayout.EndHorizontal();
        #endregion


        GUILayout.Space(20);


        DrawLine(new Vector3(0, 180), new Vector3(400, 180), Color.gray);
        DrawLine(new Vector3(400, 180), new Vector3(400, 1000), Color.gray);


        EditorGUILayout.BeginHorizontal();

        #region LeftChooseArea
        //Left Choose Area
        GUILayout.BeginArea(new Rect(0, 180, 400, 1000));

        GUIStyle labelSty = new GUIStyle("HeaderLabel");
        labelSty.fontSize = 15;

        GUILayout.Label("History Log:", labelSty);
        GUILayout.Space(5);

        if (generateType == GenerateType.Single_File)
        {
            string[] records = fileRecordPaths.Split(';');

            for (int i = 0; i < records.Length; i++)
            {
                HistoryLogItem(records[i], generateType);
            }
        }
        else if (generateType == GenerateType.Folder_Files)
        {
            string[] records = foldRecordPaths.Split(';');
            for (int i = 0; i < records.Length; i++)
            {
                HistoryLogItem(records[i], generateType);
            }
        }


        GUILayout.EndArea();
        #endregion

        #region Right Generate Area
        //Right Generate Area
        GUILayout.BeginArea(new Rect(400, 180, 600, 1000));
        if (generateType == GenerateType.Single_File)
        {
            filePath = EditorGUILayout.TextField("SelectExcel:", filePath);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
            {
                BrowseForFiles(ref filePath);
                PlayerPrefs.SetString("FilePath", filePath);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);


            if (GUI.Button(new Rect(130, 200, 150, 20), "Generate CS"))
            {
                GenerateCSFile();
            }
            if (GUI.Button(new Rect(330, 200, 150, 20), "Create Obj"))
            {
                GenerateAsset();
            }
        }
        else if (generateType == GenerateType.Folder_Files)
        {
            folderPath = EditorGUILayout.TextField("SelectFolder:", folderPath);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
            {
                BrowseForFolder(ref folderPath);
                PlayerPrefs.SetString("FoldPath", folderPath);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (GUI.Button(new Rect(130, 200, 150, 20), "Generate CS"))
            {
                GenerateCSFile();
            }
            if (GUI.Button(new Rect(330, 200, 150, 20), "Generate Obj"))
            {
                GenerateAsset();
            }

        }
        GUILayout.EndArea();
        #endregion


        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 历史记录显示
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    private void HistoryLogItem(string path, GenerateType type)
    {
        if (string.IsNullOrEmpty(path)) return;
        GUIStyle labelSty = new GUIStyle("HeaderLabel");
        labelSty.fontSize = 12;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(path, labelSty, GUILayout.MaxWidth(350)))
        {
            if (type == GenerateType.Single_File)
            {
                filePath = path;
                PlayerPrefs.SetString("FilePath", filePath);
            }
            else if (type == GenerateType.Folder_Files)
            {
                folderPath = path;
                PlayerPrefs.SetString("FoldPath", folderPath);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(8);
        if (GUILayout.Button("X"))
        {
            if (type == GenerateType.Single_File)
            {
                fileRecordPaths = RemoveString(fileRecordPaths, path);
                PlayerPrefs.SetString("FileRecordPath", fileRecordPaths);
            }
            else if (type == GenerateType.Folder_Files)
            {
                foldRecordPaths = RemoveString(foldRecordPaths, path);
                PlayerPrefs.SetString("FoldRecordPath", fileRecordPaths);
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 移除历史记录
    /// </summary>
    /// <param name="record"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private string RemoveString(string record, string path)
    {
        int index = record.IndexOf(path);
        record = record.Remove(index, path.Length);
        if (record[index] == ';')
            record = record.Remove(index, 1);
        return record;
    }


    /// <summary>
    /// 浏览文件夹
    /// </summary>
    /// <param name="target"></param>
    private void BrowseForFolder(ref string target)
    {
        var newPath = EditorUtility.OpenFolderPanel("Folder", target, string.Empty);
        if (!string.IsNullOrEmpty(newPath))
        {
            target = newPath;
        }
    }

    /// <summary>
    /// 浏览文件
    /// </summary>
    /// <param name="target"></param>
    private void BrowseForFiles(ref string target)
    {
        string dire = "";
        string[] temp = target.Split(';');
        for (int i = 0; i < temp.Length - 2; i++)
        {
            dire += temp[i];
        }
        string newPath = EditorUtility.OpenFilePanel("Files", dire, string.Empty);
        if (!string.IsNullOrEmpty(newPath))
        {
            target = newPath;
        }
    }

    /// <summary>
    /// 读取文件下表格
    /// </summary>
    private void ReadFoldTable()
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);
        foreach (var item in directory.GetFiles())
        {
            if (item.Name.EndsWith(".xlsx") || item.Name.EndsWith(".xls"))
                ReadExcelData(item.FullName);
        }
        if (!foldRecordPaths.Contains(folderPath))
        {
            foldRecordPaths = foldRecordPaths.Insert(foldRecordPaths.Length, folderPath + ";");
            PlayerPrefs.SetString("FoldRecordPath", foldRecordPaths);
        }
    }

    /// <summary>
    /// 读取表格文件
    /// </summary>
    private void ReadFileTable()
    {
        ReadExcelData(filePath);
        if (!fileRecordPaths.Contains(filePath))
        {
            fileRecordPaths = fileRecordPaths.Insert(fileRecordPaths.Length, filePath + ";");
            PlayerPrefs.SetString("FileRecordPath", fileRecordPaths);
        }
    }

    /// <summary>
    /// 读取表格
    /// </summary>
    /// <param name="path"></param>
    private void ReadExcelData(string path)
    {
        if (dataTable == null)
            dataTable = new List<DataTable>();
        if (dataTable.Count > 0) dataTable.Clear();
        //读取配置表
        DataSet data = ExcelReadData.ReadExcel(path);
        if (data != null)
        {
            for (int i = 0; i < data.Tables.Count; i++)
            {
                if (data.Tables[i].Rows.Count > 0)
                    dataTable.Add(data.Tables[i]);
            }
        }
        Debug.Log("Excel read finish!");

    }

    /// <summary>
    /// 生成配置表对应的脚本文件
    /// </summary>
    private void GenerateCSFile()
    {
        if (dataTable == null || dataTable.Count <= 0)
            if (generateType == GenerateType.Single_File)
                ReadFileTable();
            else if (generateType == GenerateType.Folder_Files)
                ReadFoldTable();
        if (dataTable == null || dataTable.Count <= 0)
        {
            Debug.LogError("Excel read error");
        }
        for (int i = 0; i < dataTable.Count; i++)
        {
            //获取表里的字段参数
            int row = dataTable[i].Rows.Count;
            int col = dataTable[i].Columns.Count;
            List<ParameterDto> parameters = new List<ParameterDto>();
            string parName = "";
            string parNote = "";
            for (int j = 0; j < col; j++)
            {
                parNote = dataTable[i].Rows[2][j].ToString();
                parName = dataTable[i].Rows[1][j].ToString();
                if (string.IsNullOrEmpty(parName)) continue;
                Type parType = GetTypeByString(dataTable[i].Rows[0][j].ToString());
                ParameterDto parameter = new ParameterDto(j, parName, parNote, parType.ToString());
                parameters.Add(parameter);
            }

            //生成配置脚本
            CSharpCodeDom sharpCodeDom = new CSharpCodeDom();
            //根
            sharpCodeDom.CreatCodeDomUnit();
            //命名空间
            sharpCodeDom.AddNamespace("ED");
            //配置脚本类
            object class1 = sharpCodeDom.CreateCustomerClass(dataTable[i].TableName);
            //添加字段
            sharpCodeDom.AddFieldForClass(class1, parameters);
            //添加特性
            sharpCodeDom.AddAttributrDeclaration(class1, typeof(System.SerializableAttribute));

            string outPath = csOutPath + "/" + dataTable[i].TableName + ".cs";
            //生成代码文件
            if (sharpCodeDom.CodeDomGenerate(outPath))
            {
                //生成配置列表类
                GenerateListCS(dataTable[i]);
            }
        }
    }

    /// <summary>
    /// 生成配置表列表文件
    /// </summary>
    /// <param name="dataTable"></param>
    private void GenerateListCS(DataTable dataTable)
    {
        //生成配置脚本
        CSharpCodeDom sharpCodeDom = new CSharpCodeDom();
        //根
        sharpCodeDom.CreatCodeDomUnit();
        //命名空间
        sharpCodeDom.AddNamespace("ED");
        //配置列表信息
        List<ParameterDto> fields = new List<ParameterDto>();
        fields.Add(new ParameterDto(0, "ConfigInfoList", "数据集合", $"List<{dataTable.TableName}>"));
        fields.Add(new ParameterDto(1, "ConfigInfoDic", "数据字典", $"Dictionary<{GetTypeByString(dataTable.Rows[0][0].ToString())},{dataTable.TableName}>"));
        //添加using命名空间
        sharpCodeDom.AddUsingNamespace("UnityEngine", "System.Collections.Generic", "System.Linq");
        //在对应配置cs文件下面添加列表类
        object class2 = sharpCodeDom.CreateCustomerClass(dataTable.TableName + "Config", "ScriptableObject");
        //添加字段
        sharpCodeDom.AddFieldForClass(class2, fields);
        //添加特性
        sharpCodeDom.AddAttributrDeclaration(class2, typeof(System.SerializableAttribute));

        //添加方法
        MethodParameter parameter = new MethodParameter();
        parameter.methodName = "GetConfigByKey";
        Dictionary<string, string> parDic = new Dictionary<string, string>();
        parDic.Add(GetTypeByString(dataTable.Rows[0][0].ToString()).ToString(), "key");
        parameter.methodParameter = parDic;
        parameter.methodReturnType = dataTable.TableName;
        parameter.methodSpinnet = $@"
            if (ConfigInfoList == null || ConfigInfoList.Count <= 0)
                return null;
            if(ConfigInfoDic==null)ConfigInfoDic = new Dictionary<{ GetTypeByString(dataTable.Rows[0][0].ToString())},{ dataTable.TableName}>();
            if(ConfigInfoDic.Count<=0)
                for(int i=0;i<ConfigInfoList.Count;i++)
                    ConfigInfoDic.Add(ConfigInfoList[i].{dataTable.Rows[1][0]},ConfigInfoList[i]);
            {dataTable.TableName} target = null;
            ConfigInfoDic.TryGetValue( key, out target);";
        parameter.methodReturn = "target";

        sharpCodeDom.AddMemberMethod(class2, parameter);
        //生成
        if (sharpCodeDom.CodeDomGenerate(csOutPath + "/" + dataTable.TableName + "Config" + ".cs"))
        {
            AssetDatabase.Refresh();
            Debug.Log($"Generate {dataTable.TableName}.cs is Succeed");
        }
        else
            Debug.LogError($"Generate {dataTable.TableName}.cs is Failed!");

    }

    /// <summary>
    /// 生成.asset
    /// </summary>
    private void GenerateAsset()
    {
        if (dataTable == null || dataTable.Count <= 0)
            if (generateType == GenerateType.Single_File)
                ReadFileTable();
            else if (generateType == GenerateType.Folder_Files)
                ReadFoldTable();
        if (dataTable == null || dataTable.Count <= 0)
        {
            Debug.LogError("Excel read error");
        }
        string array = "";
        for (int i = 0; i < dataTable.Count; i++)
        {
            //列表类名（要与生成时的名字对应）
            array = dataTable[i].TableName + "Config";
            //Type
            Type arrayType = array.GetTypeName();
            //创建实体
            object arrayTypeObj = ScriptableObject.CreateInstance(arrayType);

            //字段信息
            FieldInfo fieldInfo = arrayType.GetField("ConfigInfoList");
            //创建实体
            object listObj = Activator.CreateInstance(fieldInfo.FieldType);
            //获取列表添加元素方法
            MethodInfo method = fieldInfo.FieldType.GetMethod("Add", new Type[] { dataTable[i].TableName.GetTypeName() });

            //配置文件类型
            Type excelType = dataTable[i].TableName.GetTypeName();

            //表里数据加入到列表中
            for (int j = 3; j < dataTable[i].Rows.Count; j++)
            {
                //创建实体
                object excelTypeObj = Activator.CreateInstance(excelType);

                FieldInfo[] fieldInfos = excelType.GetFields();
                for (int k = 0; k < fieldInfos.Length; k++)
                {
                    fieldInfos[k].SetValue(excelTypeObj, Convert.ChangeType(dataTable[i].Rows[j][k], GetTypeByString(dataTable[i].Rows[0][k].ToString())));
                }
                //添加元素（相当于List.Add(item)）
                method.Invoke(listObj, new object[] { excelTypeObj });
            }

            //列表实例赋值给列表字段
            fieldInfo.SetValue(arrayTypeObj, listObj);

            //AssetDatabase.CreateAsset需要路径为"Asset/....."
            string path = objOutPath + "/" + dataTable[i].TableName + "Config.asset";
            int a = path.IndexOf("Assets/");
            path = path.Substring(a);
            //创建资源
            AssetDatabase.CreateAsset((UnityEngine.Object)arrayTypeObj, path);
            AssetDatabase.SaveAssets();
        }
        AssetDatabase.Refresh();

        Debug.Log("Create asset finish");
    }


    public static Type GetTypeByString(string type)
    {
        switch (type.ToLower())
        {
            case "bool":
                return Type.GetType("System.Boolean", true, true);
            case "byte":
                return Type.GetType("System.Byte", true, true);
            case "bytes":
            case "sbyte":
                return Type.GetType("System.SByte", true, true);
            case "char":
                return Type.GetType("System.Char", true, true);
            case "decimal":
                return Type.GetType("System.Decimal", true, true);
            case "double":
                return Type.GetType("System.Double", true, true);
            case "float":
                return Type.GetType("System.Single", true, true);
            case "int32":
            case "int":
                return Type.GetType("System.Int32", true, true);
            case "uint32":
            case "uint":
                return Type.GetType("System.UInt32", true, true);
            case "int64":
            case "long":
                return Type.GetType("System.Int64", true, true);
            case "uint64":
            case "ulong":
                return Type.GetType("System.UInt64", true, true);
            case "object":
                return Type.GetType("System.Object", true, true);
            case "short":
                return Type.GetType("System.Int16", true, true);
            case "ushort":
                return Type.GetType("System.UInt16", true, true);
            case "string":
                return Type.GetType("System.String", true, true);
            case "date":
            case "datetime":
                return Type.GetType("System.DateTime", true, true);
            case "guid":
                return Type.GetType("System.Guid", true, true);
            default:
                return Type.GetType(type, true, true);
        }
    }

    public void DrawLine(Vector3 start, Vector3 end, Color? c = null)
    {
        if (start.x > end.x)
        {
            Vector3 v = start;
            start = end;
            end = v;
        }
#if UNITY_EDITOR
        Handles.color = c.HasValue ? c.Value : Color.red;
        Handles.DrawLine(start, end);
#endif
    }


}

static class StringExtend
{

    public static Type GetTypeName(this string typeName)
    {
        Type type = null;
        Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
        int assemblyArrayLength = assemblyArray.Length;
        for (int i = 0; i < assemblyArrayLength; ++i)
        {
            type = assemblyArray[i].GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        for (int i = 0; i < assemblyArrayLength; ++i)
        {
            Type[] typeArray = assemblyArray[i].GetTypes();
            int typeArrayLength = typeArray.Length;
            for (int j = 0; j < typeArrayLength; ++j)
            {
                if (typeArray[j].Name.Equals(typeName))
                {
                    return typeArray[j];
                }
            }
        }
        return type;
    }

}
