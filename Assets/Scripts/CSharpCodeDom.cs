using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

public class CSharpCodeDom
{
    private CodeCompileUnit compileUnit;
    private CodeNamespace usingNamespace;
    private CodeNamespace _Namespace;
    /// <summary>
    /// 创建codeDom的根 (必需)
    /// </summary>
    public void CreatCodeDomUnit()
    {
        compileUnit = new CodeCompileUnit();
    }

    /// <summary>
    ///添加头部引用命名空间（非必需）
    /// </summary>
    /// <param name="namespaces">命名空间</param>
    public void AddUsingNamespace(params string[] namespaces)
    {
        usingNamespace = new CodeNamespace();
        for (int i = 0; i < namespaces.Length; i++)
        {
            usingNamespace.Imports.Add(new CodeNamespaceImport(namespaces[i]));
        }
        compileUnit.Namespaces.Add(usingNamespace);
    }

    /// <summary>
    /// 添加命名空间（非必需）
    /// </summary>
    /// <param name="namespaces">命名空间</param>
    public void AddNamespace(string _namespace)
    {
        _Namespace = new CodeNamespace(_namespace);
        compileUnit.Namespaces.Add(_Namespace);
    }

    /// <summary>
    /// 创建类
    /// </summary>
    /// <param name="className"></param>
    public object CreateCustomerClass(string className, string baseClassName = "", bool isAddNamespace = true)
    {
        CodeTypeDeclaration customerClass = new CodeTypeDeclaration(className);//类名
        customerClass.IsClass = true;
        customerClass.Attributes = MemberAttributes.Public;
        customerClass.TypeAttributes = TypeAttributes.Public;
        if (!string.IsNullOrEmpty(baseClassName))
            customerClass.BaseTypes.Add(new CodeTypeReference(baseClassName));
        if (isAddNamespace)
            _Namespace.Types.Add(customerClass);
        return customerClass;
    }

    /// <summary>
    /// 给指定类添加构造函数（非必需）
    /// </summary>
    /// <param name="customerClass"></param>
    public void AddConstructor(object _customerClass)
    {
        CodeTypeDeclaration customerClass = (CodeTypeDeclaration)_customerClass;
        //构造函数
        CodeConstructor con = new CodeConstructor();
        con.Comments.Add(new CodeCommentStatement("构造函数"));
        con.Attributes = MemberAttributes.Public;

        customerClass.Members.Add(con);

    }

    /// <summary>
    /// 给指定类中添加字段和属性
    /// </summary>
    /// <param name="customerClass">目标类</param>
    /// <param name="parameters">字段参数</param>
    /// <param name="isAddProp">是否添加属性</param>
    public void AddFieldForClass(object _customerClass, List<ParameterDto> parameters, bool isAddProp = false)
    {
        if (_customerClass == null || parameters == null) return;
        CodeTypeDeclaration customerClass = (CodeTypeDeclaration)_customerClass;
        CodeMemberField field = null;
        CodeMemberProperty property = null;
        //字段名
        string parameterName = "";
        //属性名
        string parameterName_P = "";

        for (int i = 0; i < parameters.Count; i++)
        {
            if (isAddProp)
            {
                parameterName = "_" + parameters[i].ParameterName.Substring(0);
                parameterName_P = parameters[i].ParameterName.Substring(0);
            }
            else
                parameterName = parameters[i].ParameterName.Substring(0);

            //添加字段
            field = new CodeMemberField(parameters[i].ParameterType, parameterName);

            field.Attributes = isAddProp ? MemberAttributes.Private : MemberAttributes.Public;
            //字段注释
            field.Comments.Add(new CodeCommentStatement(parameters[i].ParameterNote));
            customerClass.Members.Add(field);

            if (isAddProp)
            {
                //添加属性
                property = new CodeMemberProperty();
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.Name = parameterName_P;
                property.HasGet = true;
                property.HasSet = true;
                property.Type = new CodeTypeReference(parameters[i].ParameterType);
                property.Comments.Add(new CodeCommentStatement(parameters[i].ParameterNote));
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), parameterName)));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), parameterName), new CodePropertySetValueReferenceExpression()));

                customerClass.Members.Add(property);
            }
        }
    }

    /// <summary>
    /// 给指定类添加特征
    /// </summary>
    /// <param name="customerClass"></param>
    public void AddAttributrDeclaration(object _customerClass, Type serializable)
    {
        CodeTypeDeclaration customerClass = (CodeTypeDeclaration)_customerClass;
        customerClass.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(serializable)));

    }

    public void AddMemberMethod(object _customerClass, MethodParameter parameter)
    {
        CodeMemberMethod method = new CodeMemberMethod();
        method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        method.Name = parameter.methodName;
        method.ReturnType = new CodeTypeReference(parameter.methodReturnType);
        foreach (var item in parameter.methodParameter)
        {
            method.Parameters.Add(new CodeParameterDeclarationExpression(item.Key, item.Value));
        }
        method.Statements.Add(new CodeSnippetStatement(parameter.methodSpinnet));
        method.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(parameter.methodReturn)));

        CodeTypeDeclaration customerClass = (CodeTypeDeclaration)_customerClass;
        customerClass.Members.Add(method);
    }

    /// <summary>
    /// 最终步，生成脚本（必需）
    /// </summary>
    /// <param name="outPath">输出路径</param>
    /// <returns></returns>
    public bool CodeDomGenerate(string outPath)
    {
        bool result = false;
        //选择生成的语言
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        //生成的一些选项
        CodeGeneratorOptions options = new CodeGeneratorOptions();
        options.BracingStyle = "C";
        options.BlankLinesBetweenMembers = true;

        if (string.IsNullOrEmpty(outPath))
        {
            result = false;
        }
        else
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outPath))
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
                result = true;
            }
        }

        return result;
    }

}

public class MethodParameter
{
    public string methodName;
    public string methodReturnType;
    public string methodSpinnet;
    public Dictionary<string, string> methodParameter;
    public string methodReturn;

}

public class ParameterDto
{
    private int parameterId;
    private string parameterName;
    private string parameterNote;
    private string parameterType;
    public ParameterDto(int pid, string pname, string pnote, string ptype)
    {
        parameterId = pid;
        parameterName = pname;
        parameterNote = pnote;
        parameterType = ptype;
    }

    public int ParameterId
    {
        get
        {
            return parameterId;
        }
        set
        {
            parameterId = value;
        }
    }

    public string ParameterName
    {
        get
        {
            return parameterName;
        }
        set
        {
            parameterName = value;
        }
    }
    public string ParameterNote
    {
        get
        {
            return parameterNote;
        }
        set
        {
            parameterNote = value;
        }
    }
    public string ParameterType
    {
        get
        {
            return parameterType;
        }
        set
        {
            parameterType = value;
        }
    }


}
