using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    /// <summary>
    /// ������·��
    /// </summary>
    public string skinPath;
    /// <summary>
    /// �������
    /// </summary>
    public GameObject skin;
    /// <summary>
    /// ���㼶
    /// </summary>
    public PanelManager.Layer layer = PanelManager.Layer.Panel;
    public void Init()
    {
        skin=Instantiate(Resources.Load<GameObject>(skinPath));
    }
    /// <summary>
    /// ����ʼ��ʱִ��
    /// </summary>
    public virtual void OnInit()
    {
        skinPath="UI/"+GetType().Name;
    }
    public virtual void OnShow(params object[] args)
    {

    }
    public virtual void OnClose()
    {
        
    }
    protected void Close()
    {
        string name=GetType().Name;
        PanelManager.Instance.Close(name);
    }
}
