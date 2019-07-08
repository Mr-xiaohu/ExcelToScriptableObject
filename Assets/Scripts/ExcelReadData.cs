using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Excel;
using System.Data;
using System.IO;
using UnityEditor;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;

public class ExcelReadData : MonoBehaviour
{


    public static DataSet ReadExcel(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        IExcelDataReader excelReader = null;
        if (filePath.EndsWith(".xls"))
            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);  //.xls
        else if (filePath.EndsWith(".xlsx"))
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream); //.xlsx

        if (excelReader == null)
        {
            Debug.LogError($"Read Excel Failed...filepath : {filePath}");
            return null;
        }

        //DataSet result = excelReader.AsDataSet(); //excel有空值会报错

        DataSet result = new DataSet();

        do
        {
            DataTable dt = GetTable(excelReader);
            result.Merge(dt);
        } while (excelReader.NextResult());

        excelReader.Close();
        excelReader.Dispose();
        stream.Close();
        stream.Dispose();

        return result;
    }

    private static DataTable GetTable(IExcelDataReader excelReader)
    {
        DataTable dt = new DataTable();
        dt.TableName = excelReader.Name;

        bool isInit = false;
        string[] ItemArray = null;
        int rowsNum = 0;
        while (excelReader.Read())
        {
            rowsNum++;

            if (!isInit)
            {
                isInit = true;
                for (int i = 0; i < excelReader.FieldCount + 1; i++)
                {
                    dt.Columns.Add("", typeof(string));
                }
                ItemArray = new string[excelReader.FieldCount];
            }

            if (excelReader.IsDBNull(0))
            {
                continue;
            }
            for (int i = 0; i < excelReader.FieldCount; i++)
            {
                string value = excelReader.IsDBNull(i) ? "" : excelReader.GetString(i);
                ItemArray[i] = value;
            }
            dt.Rows.Add(ItemArray);
        }
        return dt;
    }

}



