using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{
    /// <summary>
    /// �㼶
    /// </summary>
    public enum Layer
    {
        Panel,
        Tip
    }
    /// <summary>
    /// �㼶�б�
    /// </summary>
    private Dictionary<Layer,Transform> layers = new Dictionary<Layer,Transform>();
    /// <summary>
    /// ����б�
    /// </summary>
    private Dictionary<string,BasePanel> panels = new Dictionary<string,BasePanel>();
    /// <summary>
    /// ������Ԫ��
    /// </summary>
    private Transform root;
    /// <summary>
    /// ����
    /// </summary>
    private Transform canvas;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        layers.Add(Layer.Panel, canvas.Find("Panel"));
        layers.Add(Layer.Tip, canvas.Find("Tip"));
    }
    /// <summary>
    /// �����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    public void Open<T>(params object[] args) where T : BasePanel
    {
        string name=typeof(T).Name;
        //�Ѿ���
        if(panels.ContainsKey(name)) 
        {
            return;
        }
        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.OnInit();
        panel.Init();

        Transform layer = layers[panel.layer];
        panel.skin.transform.SetParent(layer,false);
        panels.Add(name, panel);
        panel.OnShow();
    }
    /// <summary>
    /// �ر����
    /// </summary>
    /// <param name="name">�������</param>
    public void Close(string name)
    {
        //�ж��Ƿ���������
        if(!panels.ContainsKey(name))
        {
            return;
        }
        BasePanel panel = panels[name];
        panel.OnClose();
        panels.Remove(name);
        Destroy(panel.skin);
        Destroy(panel);
    }
}
